using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Constants;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;

        public VoucherRepository(
            FoodOrderingContext context,
            IOptions<ConnectionOption> connectionOption)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherRequest request)
        {
            // Check if code already exists
            var exists = await _context.Vouchers.AnyAsync(v => v.Code == request.Code.ToUpper());
            if (exists)
                throw new BadRequestException(VoucherMessages.CodeExists);

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = request.Code.ToUpper(),
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                Value = request.Value,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscount = request.MaxDiscount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                UsageLimitPerUser = request.UsageLimitPerUser ?? 1,
                UsedCount = 0,
                IsActive = true,
                StoreId = request.StoreId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return await GetVoucherByIdAsync(voucher.Id);
        }

        public async Task<VoucherDto> GetVoucherByIdAsync(Guid voucherId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT v.Id, v.Code, v.Name, v.Description, v.Type, v.Value,
                       v.MinOrderAmount, v.MaxDiscount, v.StartDate, v.EndDate,
                       v.UsageLimit, v.UsageLimitPerUser, v.UsedCount, v.IsActive,
                       v.StoreId, s.Name as StoreName
                FROM Vouchers v
                LEFT JOIN Stores s ON v.StoreId = s.Id
                WHERE v.Id = @voucherId";

            return await con.QueryFirstOrDefaultAsync<VoucherDto>(sql, new { voucherId });
        }

        public async Task<VoucherDto> GetVoucherByCodeAsync(string code)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT v.Id, v.Code, v.Name, v.Description, v.Type, v.Value,
                       v.MinOrderAmount, v.MaxDiscount, v.StartDate, v.EndDate,
                       v.UsageLimit, v.UsageLimitPerUser, v.UsedCount, v.IsActive,
                       v.StoreId, s.Name as StoreName
                FROM Vouchers v
                LEFT JOIN Stores s ON v.StoreId = s.Id
                WHERE v.Code = @code";

            return await con.QueryFirstOrDefaultAsync<VoucherDto>(sql, new { code = code.ToUpper() });
        }

        public async Task<IEnumerable<VoucherDto>> GetActiveVouchersAsync(long? storeId = null)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var now = DateTime.UtcNow;

            var sql = @"
                SELECT v.Id, v.Code, v.Name, v.Description, v.Type, v.Value,
                       v.MinOrderAmount, v.MaxDiscount, v.StartDate, v.EndDate,
                       v.UsageLimit, v.UsageLimitPerUser, v.UsedCount, v.IsActive,
                       v.StoreId, s.Name as StoreName
                FROM Vouchers v
                LEFT JOIN Stores s ON v.StoreId = s.Id
                WHERE v.IsActive = 1 
                  AND v.StartDate <= @now 
                  AND v.EndDate >= @now
                  AND (v.UsageLimit IS NULL OR v.UsedCount < v.UsageLimit)" +
                (storeId.HasValue ? " AND (v.StoreId IS NULL OR v.StoreId = @storeId)" : "") +
                " ORDER BY v.EndDate ASC";

            return await con.QueryAsync<VoucherDto>(sql, new { now, storeId });
        }

        public async Task<IEnumerable<VoucherDto>> GetUserAvailableVouchersAsync(long userId, decimal orderAmount, long? storeId = null)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var now = DateTime.UtcNow;

            var sql = @"
                SELECT v.Id, v.Code, v.Name, v.Description, v.Type, v.Value,
                       v.MinOrderAmount, v.MaxDiscount, v.StartDate, v.EndDate,
                       v.UsageLimit, v.UsageLimitPerUser, v.UsedCount, v.IsActive,
                       v.StoreId, s.Name as StoreName
                FROM Vouchers v
                LEFT JOIN Stores s ON v.StoreId = s.Id
                WHERE v.IsActive = 1 
                  AND v.StartDate <= @now 
                  AND v.EndDate >= @now
                  AND v.MinOrderAmount <= @orderAmount
                  AND (v.UsageLimit IS NULL OR v.UsedCount < v.UsageLimit)
                  AND (v.StoreId IS NULL" + (storeId.HasValue ? " OR v.StoreId = @storeId" : "") + @")
                  AND (v.UsageLimitPerUser IS NULL OR 
                       (SELECT COUNT(*) FROM VoucherUsages WHERE VoucherId = v.Id AND UserId = @userId) < v.UsageLimitPerUser)
                ORDER BY v.Value DESC";

            return await con.QueryAsync<VoucherDto>(sql, new { now, orderAmount, storeId, userId });
        }

        public async Task<VoucherDto> UpdateVoucherAsync(Guid voucherId, UpdateVoucherRequest request)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);

            if (voucher == null)
                throw new BadRequestException(VoucherMessages.NotFound);

            if (!string.IsNullOrEmpty(request.Name))
                voucher.Name = request.Name;
            
            if (!string.IsNullOrEmpty(request.Description))
                voucher.Description = request.Description;
            
            if (request.MinOrderAmount.HasValue)
                voucher.MinOrderAmount = request.MinOrderAmount.Value;
            
            if (request.MaxDiscount.HasValue)
                voucher.MaxDiscount = request.MaxDiscount.Value;
            
            if (request.EndDate.HasValue)
                voucher.EndDate = request.EndDate.Value;
            
            if (request.UsageLimit.HasValue)
                voucher.UsageLimit = request.UsageLimit.Value;
            
            if (request.UsageLimitPerUser.HasValue)
                voucher.UsageLimitPerUser = request.UsageLimitPerUser.Value;
            
            if (request.IsActive.HasValue)
                voucher.IsActive = request.IsActive.Value;

            voucher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetVoucherByIdAsync(voucherId);
        }

        public async Task<bool> DeactivateVoucherAsync(Guid voucherId)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);

            if (voucher == null)
                return false;

            voucher.IsActive = false;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VoucherApplyResult> ApplyVoucherAsync(ApplyVoucherRequest request, long userId)
        {
            var voucher = await GetVoucherByCodeAsync(request.Code);

            if (voucher == null)
                return new VoucherApplyResult { Success = false, Message = VoucherMessages.NotFound };

            // Validate voucher
            var now = DateTime.UtcNow;

            if (!voucher.IsActive)
                return new VoucherApplyResult { Success = false, Message = VoucherMessages.NotFound };

            if (now < voucher.StartDate)
                return new VoucherApplyResult { Success = false, Message = VoucherMessages.NotStarted };

            if (now > voucher.EndDate)
                return new VoucherApplyResult { Success = false, Message = VoucherMessages.Expired };

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit)
                return new VoucherApplyResult { Success = false, Message = VoucherMessages.UsageLimitReached };

            if (request.OrderAmount < voucher.MinOrderAmount)
                return new VoucherApplyResult 
                { 
                    Success = false,
                    Message = $"Minimum order amount is {voucher.MinOrderAmount:N0} VND"
                };

            if (voucher.StoreId.HasValue && request.StoreId.HasValue && voucher.StoreId != request.StoreId)
                return new VoucherApplyResult { Success = false, Message = VoucherMessages.StoreNotMatch };

            // Check user usage
            if (voucher.UsageLimitPerUser.HasValue)
            {
                var userUsageCount = await _context.VoucherUsages
                    .CountAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == userId);

                if (userUsageCount >= voucher.UsageLimitPerUser)
                    return new VoucherApplyResult { Success = false, Message = VoucherMessages.AlreadyUsed };
            }

            // Calculate discount
            decimal discount = voucher.Type switch
            {
                VoucherType.Percent => request.OrderAmount * voucher.Value / 100,
                VoucherType.FixedAmount => voucher.Value,
                VoucherType.FreeShipping => 0, // Handle separately
                _ => 0
            };

            // Apply max discount cap
            if (voucher.MaxDiscount.HasValue && discount > voucher.MaxDiscount.Value)
                discount = voucher.MaxDiscount.Value;

            // Don't exceed order amount
            if (discount > request.OrderAmount)
                discount = request.OrderAmount;

            return new VoucherApplyResult
            {
                Success = true,
                Message = VoucherMessages.ApplySuccess,
                Voucher = voucher,
                DiscountAmount = discount,
                FinalAmount = request.OrderAmount - discount
            };
        }

        public async Task<bool> RecordVoucherUsageAsync(Guid voucherId, long userId, Guid orderId, decimal discountAmount)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);

            if (voucher == null)
                return false;

            var usage = new VoucherUsage
            {
                Id = Guid.NewGuid(),
                VoucherId = voucherId,
                UserId = userId,
                OrderId = orderId,
                DiscountAmount = discountAmount,
                UsedAt = DateTime.UtcNow
            };

            _context.VoucherUsages.Add(usage);

            // Increment usage count
            voucher.UsedCount++;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
