using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Constants;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Helpers;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingPRM392.Extensions;
using FoodOrderingRepository.Implement;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly MomoOption _momoOptions;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IWalletService walletService, 
            IOptions<MomoOption> momoOptions,
            ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _momoOptions = momoOptions.Value;
            _logger = logger;
        }

        // Get current wallet balance
        [HttpGet("balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WalletMessages.Unauthorized });

            var wallet = await _walletService.GetWalletBalanceAsync(userId.Value);
            return Ok(new ParentResultResponse { Message = WalletMessages.GetBalanceSuccess, Result = wallet });
        }


        // Create deposit request via MoMo
        [HttpPost("deposit")]
        [Authorize]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WalletMessages.Unauthorized });
            try
            {
                _logger.LogInformation("Creating deposit: UserId={UserId}, Amount={Amount}",
                    userId, request.Amount);

                var response = await _walletService.CreateDepositRequestAsync(request, userId.Value);

                if (!response.Success)
                {
                    return BadRequest(new ParentResultResponse
                    {
                        Message = response.Message ?? WalletMessages.DepositRequestFailed,
                        Result = response
                    });
                }
                return Ok(new ParentResultResponse { Message = WalletMessages.DepositRequestSuccess, Result = response });
            }
            catch (DepositFailedException ex)
            {
                _logger.LogWarning(ex, "Deposit failed: UserId={UserId}", userId);
                return BadRequest(new ParentResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deposit: UserId={UserId}", userId);
                throw;
            }
        }

        // MoMo IPN (Instant Payment Notification) webhook for deposit
        [HttpPost("momo/ipn")]
        public async Task<IActionResult> MomoIpn([FromBody] MomoIpnRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "MoMo IPN: OrderId={OrderId}, ResultCode={ResultCode}, TransId={TransId}",
                    request.OrderId, request.ResultCode, request.TransId);

                // Verify signature
                if (!VerifyIpnSignature(request))
                {
                    _logger.LogWarning("Invalid IPN signature: OrderId={OrderId}", request.OrderId);
                    return BadRequest(new ParentResponse { Message = WalletMessages.InvalidSignature });
                }

                // Validate transaction type
                if (!WalletService.IsDepositTransaction(request.OrderId))
                {
                    _logger.LogWarning("Not a deposit transaction: OrderId={OrderId}", request.OrderId);
                    return BadRequest(new ParentResponse { Message = WalletMessages.InvalidTransactionType });
                }

                // Check payment result
                if (request.ResultCode != 0)
                {
                    _logger.LogWarning("❌ Deposit failed: OrderId={OrderId}, Message={Message}",
                        request.OrderId, request.Message);
                    return NoContent(); // MoMo expects 204 for failed payments too
                }

                // Extract and validate userId
                var userId = WalletService.ExtractUserIdFromDepositOrderId(request.OrderId);
                if (userId == null)
                {
                    _logger.LogWarning("Cannot extract UserId from OrderId={OrderId}", request.OrderId);
                    return BadRequest(new ParentResponse { Message = WalletMessages.InvalidOrderId });
                }

                // Process deposit
                await _walletService.ProcessDepositAsync(
                    userId.Value,
                    request.Amount,
                    request.OrderId,
                    $"Nạp tiền qua MoMo - TransId: {request.TransId}"
                );

                _logger.LogInformation("✅ Deposit success: UserId={UserId}, Amount={Amount}", 
                    userId, request.Amount);
                
                return NoContent();
            }
            catch (WalletException ex)
            {
                _logger.LogWarning(ex, "Wallet error processing IPN: OrderId={OrderId}", request.OrderId);
                return BadRequest(new ParentResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error processing IPN: OrderId={OrderId}", request.OrderId);
                // Still return 204 to avoid MoMo retry
                return NoContent();
            }
        }

        // MoMo Return URL (redirect after payment)
        [HttpGet("momo/return")]
        public IActionResult MomoReturn(
            [FromQuery] string orderId, 
            [FromQuery] int resultCode, 
            [FromQuery] string message)
        {
            _logger.LogInformation("MoMo return: OrderId={OrderId}, ResultCode={ResultCode}", 
                orderId, resultCode);

            var responseMessage = resultCode == 0 
                ? WalletMessages.DepositSuccess 
                : message ?? WalletMessages.DepositFailed;

            var status = resultCode == 0 ? "Success" : "Failed";
            return Ok(new ParentResultResponse{ Message = responseMessage, 
                Result = new
                {
                    OrderId = orderId,
                    Status = status,
                    ResultCode = resultCode
                }
                });
        }

        // Get transaction history
        [HttpGet("transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactionHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WalletMessages.Unauthorized });

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var transactions = await _walletService.GetTransactionHistoryAsync(
                userId.Value, pageNumber, pageSize);
            return Ok(new ParentResultResponse
            {
                Message = WalletMessages.GetTransactionsSuccess,
                Result = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Transactions = transactions
                }
            });
        }

        // Check if user has sufficient balance
        [HttpGet("check-balance/{amount}")]
        [Authorize]
        public async Task<IActionResult> CheckBalance(decimal amount)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WalletMessages.Unauthorized });

            var hasSufficient = await _walletService.HasSufficientBalanceAsync(userId.Value, amount);

            return Ok(new ParentResultResponse
            {
                Message = hasSufficient
                    ? WalletMessages.SufficientBalance
                    : WalletMessages.InsufficientBalance,
                Result = new
                {
                    Amount = amount,
                    HasSufficientBalance = hasSufficient
                }
            });
        }

        // Verify IPN signature from MoMo
        private bool VerifyIpnSignature(MomoIpnRequest request)
        {
            try
            {
                var rawSignature = MomoPaymentHelper.BuildIpnRawSignature(
                    accessKey: _momoOptions.AccessKey,
                    amount: request.Amount,
                    extraData: request.ExtraData ?? string.Empty,
                    message: request.Message ?? string.Empty,
                    orderId: request.OrderId,
                    orderInfo: request.OrderInfo ?? string.Empty,
                    orderType: request.OrderType ?? string.Empty,
                    partnerCode: request.PartnerCode,
                    payType: request.PayType ?? string.Empty,
                    requestId: request.RequestId,
                    responseTime: request.ResponseTime,
                    resultCode: request.ResultCode,
                    transId: request.TransId
                );

                var computedSignature = MomoPaymentHelper.ComputeHmacSha256(
                    rawSignature, _momoOptions.SecretKey);
                
                return MomoPaymentHelper.VerifySignature(request.Signature, computedSignature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying IPN signature");
                return false;
            }
        }
    }
}
