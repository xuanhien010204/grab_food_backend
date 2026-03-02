using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Helpers;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace FoodOrderingRepository.Implement
{
    // Wallet service
    public class WalletService : IWalletService
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;
        private readonly MomoOption _momoOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WalletService> _logger;

        private const string DEPOSIT_PREFIX = "DEPOSIT";

        public WalletService(
            FoodOrderingContext context,
            IOptions<ConnectionOption> connectionOption,
            IOptions<MomoOption> momoOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<WalletService> logger)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
            _momoOptions = momoOptions.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Get user's wallet balance
        public async Task<WalletResponse> GetWalletBalanceAsync(long userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            return new WalletResponse
            {
                UserId = user.Id,
                UserName = user.Name,
                Balance = user.WalletAmount,
                LastUpdated = DateTime.UtcNow
            };
        }

        // Create deposit request via MoMo
        public async Task<PaymentResponse> CreateDepositRequestAsync(DepositRequest request, long userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var requestId = MomoPaymentHelper.GenerateRequestId();
            var orderId = $"{DEPOSIT_PREFIX}_{userId}_{requestId}";
            var orderInfo = $"Nạp tiền vào ví - {user.Name}";

            if (!string.IsNullOrEmpty(request.Note))
            {
                orderInfo += $" - {request.Note}";
            }

            // Create pending transaction record
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TransactionType = TransactionType.Deposit,
                Amount = request.Amount,
                BalanceBefore = user.WalletAmount,
                BalanceAfter = user.WalletAmount,
                Status = TransactionStatus.Pending,
                Description = orderInfo,
                ExternalReference = orderId,
                PaymentMethod = "MoMo",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Build MoMo signature
            var rawSignature = MomoPaymentHelper.BuildCreatePaymentRawSignature(
                accessKey: _momoOptions.AccessKey,
                amount: request.Amount,
                extraData: string.Empty,
                ipnUrl: _momoOptions.NotifyUrl,
                orderId: orderId,
                orderInfo: orderInfo,
                partnerCode: _momoOptions.PartnerCode,
                redirectUrl: _momoOptions.ReturnUrl,
                requestId: requestId,
                requestType: "captureWallet"
            );

            var signature = MomoPaymentHelper.ComputeHmacSha256(rawSignature, _momoOptions.SecretKey);

            var momoRequest = new MomoCreatePaymentRequest
            {
                PartnerCode = _momoOptions.PartnerCode,
                RequestId = requestId,
                Amount = request.Amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                RedirectUrl = _momoOptions.ReturnUrl,
                IpnUrl = _momoOptions.NotifyUrl,
                RequestType = "captureWallet",
                ExtraData = string.Empty,
                Lang = "vi",
                Signature = signature,
                AutoCapture = true
            };

            try
            {
                var httpClient = _httpClientFactory.CreateClient("MoMo");
                var jsonContent = JsonConvert.SerializeObject(momoRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Creating MoMo deposit: UserId={UserId}, Amount={Amount}", userId, request.Amount);

                var response = await httpClient.PostAsync("/v2/gateway/api/create", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponse>(responseContent);

                if (momoResponse == null || !momoResponse.IsSuccess)
                {
                    transaction.Status = TransactionStatus.Failed;
                    await _context.SaveChangesAsync();
                    _logger.LogWarning("MoMo deposit failed: {Message}", momoResponse?.Message);
                }

                return new PaymentResponse
                {
                    OrderId = orderId,
                    Amount = request.Amount,
                    PayUrl = momoResponse?.PayUrl,
                    DeepLink = momoResponse?.Deeplink,
                    QrCodeUrl = momoResponse?.QrCodeUrl,
                    Message = momoResponse?.Message,
                    Success = momoResponse?.IsSuccess ?? false
                };
            }
            catch (Exception ex)
            {
                transaction.Status = TransactionStatus.Failed;
                await _context.SaveChangesAsync();
                _logger.LogError(ex, "Error creating MoMo deposit");
                throw;
            }
        }

        // Process successful deposit (called from MoMo IPN or return URL fallback)
        public async Task<decimal> ProcessDepositAsync(long userId, decimal amount, string transactionId, string description)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                // Idempotency: if already completed, return current balance without re-processing
                var alreadyCompleted = await _context.WalletTransactions
                    .AnyAsync(t => t.ExternalReference == transactionId && t.Status == TransactionStatus.Completed);

                if (alreadyCompleted)
                {
                    _logger.LogInformation("Deposit already processed (idempotent skip): {TransactionId}", transactionId);
                    await dbTransaction.RollbackAsync();
                    return user.WalletAmount;
                }

                var balanceBefore = user.WalletAmount;

                var pendingTx = await _context.WalletTransactions
                    .FirstOrDefaultAsync(t => t.ExternalReference == transactionId && t.Status == TransactionStatus.Pending);

                // If amount not provided (called from return URL fallback), use the pending tx amount
                var effectiveAmount = (amount > 0) ? amount : pendingTx?.Amount ?? 0;
                if (effectiveAmount <= 0)
                {
                    _logger.LogWarning("Cannot determine deposit amount for {TransactionId}", transactionId);
                    await dbTransaction.RollbackAsync();
                    return balanceBefore;
                }

                var balanceAfter = balanceBefore + effectiveAmount;

                user.WalletAmount = balanceAfter;

                if (pendingTx != null)
                {
                    pendingTx.BalanceAfter = balanceAfter;
                    pendingTx.Status = TransactionStatus.Completed;
                    pendingTx.CompletedAt = DateTime.UtcNow;
                }
                else
                {
                    var transaction = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        TransactionType = TransactionType.Deposit,
                        Amount = effectiveAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        Status = TransactionStatus.Completed,
                        Description = description,
                        ExternalReference = transactionId,
                        PaymentMethod = "MoMo",
                        CreatedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow
                    };
                    _context.WalletTransactions.Add(transaction);
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                _logger.LogInformation("✅ Deposit success: UserId={UserId}, Amount={Amount}, NewBalance={Balance}",
                    userId, amount, balanceAfter);

                return balanceAfter;
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Error processing deposit");
                throw;
            }
        }

        // Get transaction history
        public async Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(long userId, int pageNumber = 1, int pageSize = 20)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var offset = (pageNumber - 1) * pageSize;
            var sql = @"
                SELECT Id, TransactionType, Amount, BalanceBefore, BalanceAfter, 
                       Status, Description, ExternalReference, PaymentMethod, 
                       CreatedAt, CompletedAt
                FROM WalletTransactions
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return await con.QueryAsync<WalletTransactionDto>(sql, new { UserId = userId, Offset = offset, PageSize = pageSize });
        }

        // Check if user has sufficient balance
        public async Task<bool> HasSufficientBalanceAsync(long userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null && user.WalletAmount >= amount;
        }

        // Check if orderId is a deposit transaction
        public static bool IsDepositTransaction(string orderId)
        {
            return !string.IsNullOrEmpty(orderId) && orderId.StartsWith(DEPOSIT_PREFIX);
        }

        // Extract userId from deposit orderId (DEPOSIT_{userId}_{requestId})
        public static long? ExtractUserIdFromDepositOrderId(string orderId)
        {
            if (string.IsNullOrEmpty(orderId) || !orderId.StartsWith(DEPOSIT_PREFIX))
                return null;

            var parts = orderId.Split('_');
            if (parts.Length >= 2 && long.TryParse(parts[1], out var userId))
                return userId;

            return null;
        }
    }
}
