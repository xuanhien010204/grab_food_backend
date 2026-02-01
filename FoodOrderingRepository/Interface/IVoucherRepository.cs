using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IVoucherRepository
    {
        Task<VoucherDto> CreateVoucherAsync(CreateVoucherRequest request);
        Task<VoucherDto> GetVoucherByIdAsync(Guid voucherId);
        Task<VoucherDto> GetVoucherByCodeAsync(string code);
        Task<IEnumerable<VoucherDto>> GetActiveVouchersAsync(long? storeId = null);
        Task<IEnumerable<VoucherDto>> GetUserAvailableVouchersAsync(long userId, decimal orderAmount, long? storeId = null);
        Task<VoucherDto> UpdateVoucherAsync(Guid voucherId, UpdateVoucherRequest request);
        Task<bool> DeactivateVoucherAsync(Guid voucherId);
        
        // Apply voucher to calculate discount
        Task<VoucherApplyResult> ApplyVoucherAsync(ApplyVoucherRequest request, long userId);
        
        // Record voucher usage after order is placed
        Task<bool> RecordVoucherUsageAsync(Guid voucherId, long userId, Guid orderId, decimal discountAmount);
    }

    public class VoucherApplyResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public VoucherDto Voucher { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
