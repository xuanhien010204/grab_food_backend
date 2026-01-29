using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;

namespace FoodOrderingRepository.Interface
{
    public interface IWalletService
    {
        // Get user's wallet balance
        Task<WalletResponse> GetWalletBalanceAsync(long userId);

        // Create deposit request via MoMo
        Task<PaymentResponse> CreateDepositRequestAsync(DepositRequest request, long userId);

        // Process successful deposit (called from MoMo IPN)
        Task<decimal> ProcessDepositAsync(long userId, decimal amount, string transactionId, string description);

        // Get transaction history
        Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(long userId, int pageNumber = 1, int pageSize = 20);

        // Check if user has sufficient balance
        Task<bool> HasSufficientBalanceAsync(long userId, decimal amount);
    }
}
