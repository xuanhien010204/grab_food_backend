using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IWithdrawalRepository
    {
        // Manager creates a withdrawal request
        Task<WithdrawalRequestDto> CreateWithdrawalRequestAsync(long managerId, CreateWithdrawalRequest request);

        // Manager views their own withdrawal requests
        Task<IEnumerable<WithdrawalRequestDto>> GetManagerWithdrawalRequestsAsync(long managerId);

        // Admin views all pending requests
        Task<IEnumerable<WithdrawalRequestDto>> GetAllPendingRequestsAsync();

        // Admin approves a withdrawal request
        Task<WithdrawalRequestDto> ApproveWithdrawalAsync(Guid requestId, long adminId, string adminNote);

        // Admin rejects a withdrawal request
        Task<WithdrawalRequestDto> RejectWithdrawalAsync(Guid requestId, long adminId, string adminNote);
    }
}
