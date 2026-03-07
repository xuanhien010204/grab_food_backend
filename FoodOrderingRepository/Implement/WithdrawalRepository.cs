using FoodOrderingCore.Constants;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingRepository.Implement
{
    public class WithdrawalRepository : IWithdrawalRepository
    {
        private readonly FoodOrderingContext _context;

        public WithdrawalRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<WithdrawalRequestDto> CreateWithdrawalRequestAsync(long managerId, CreateWithdrawalRequest request)
        {
            var manager = await _context.Users.FindAsync(managerId);
            if (manager == null || manager.RoleId != (int)RoleEnum.Manager)
                throw new BadRequestException(WithdrawalMessages.OnlyManagerCanWithdraw);

            if (manager.WalletAmount < request.Amount)
                throw new BadRequestException(WithdrawalMessages.InsufficientBalance);

            var withdrawal = new WithdrawalRequest
            {
                Id = Guid.NewGuid(),
                ManagerId = managerId,
                Amount = request.Amount,
                BankAccount = request.BankAccount,
                BankName = request.BankName,
                Note = request.Note,
                Status = WithdrawalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.WithdrawalRequests.Add(withdrawal);
            await _context.SaveChangesAsync();

            return MapToDto(withdrawal, manager.Name, null);
        }

        public async Task<IEnumerable<WithdrawalRequestDto>> GetManagerWithdrawalRequestsAsync(long managerId)
        {
            return await _context.WithdrawalRequests
                .Where(wr => wr.ManagerId == managerId)
                .OrderByDescending(wr => wr.CreatedAt)
                .Select(wr => new WithdrawalRequestDto
                {
                    Id = wr.Id,
                    ManagerId = wr.ManagerId,
                    ManagerName = wr.Manager.Name,
                    Amount = wr.Amount,
                    BankAccount = wr.BankAccount,
                    BankName = wr.BankName,
                    Note = wr.Note,
                    Status = wr.Status,
                    AdminNote = wr.AdminNote,
                    CreatedAt = wr.CreatedAt,
                    ProcessedAt = wr.ProcessedAt,
                    ProcessedByAdminName = wr.ProcessedByAdmin != null ? wr.ProcessedByAdmin.Name : null
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<WithdrawalRequestDto>> GetAllPendingRequestsAsync()
        {
            return await _context.WithdrawalRequests
                .Where(wr => wr.Status == WithdrawalStatus.Pending)
                .OrderBy(wr => wr.CreatedAt)
                .Select(wr => new WithdrawalRequestDto
                {
                    Id = wr.Id,
                    ManagerId = wr.ManagerId,
                    ManagerName = wr.Manager.Name,
                    Amount = wr.Amount,
                    BankAccount = wr.BankAccount,
                    BankName = wr.BankName,
                    Note = wr.Note,
                    Status = wr.Status,
                    AdminNote = wr.AdminNote,
                    CreatedAt = wr.CreatedAt,
                    ProcessedAt = wr.ProcessedAt,
                    ProcessedByAdminName = wr.ProcessedByAdmin != null ? wr.ProcessedByAdmin.Name : null
                })
                .ToListAsync();
        }

        public async Task<WithdrawalRequestDto> ApproveWithdrawalAsync(Guid requestId, long adminId, string adminNote)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var withdrawal = await _context.WithdrawalRequests
                    .Include(wr => wr.Manager)
                    .FirstOrDefaultAsync(wr => wr.Id == requestId);

                if (withdrawal == null)
                    throw new BadRequestException(WithdrawalMessages.NotFound);

                if (withdrawal.Status != WithdrawalStatus.Pending)
                    throw new BadRequestException(WithdrawalMessages.AlreadyProcessed);

                var manager = withdrawal.Manager;
                if (manager.WalletAmount < withdrawal.Amount)
                    throw new BadRequestException(WithdrawalMessages.InsufficientBalance);

                // Deduct from manager's wallet
                var managerBalanceBefore = manager.WalletAmount;
                manager.WalletAmount -= withdrawal.Amount;

                // Create wallet transaction for manager (withdrawal)
                var managerTx = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = manager.Id,
                    TransactionType = TransactionType.Withdrawal,
                    Amount = -withdrawal.Amount,
                    BalanceBefore = managerBalanceBefore,
                    BalanceAfter = manager.WalletAmount,
                    Status = TransactionStatus.Completed,
                    Description = $"Rút tiền - Yêu cầu #{withdrawal.Id.ToString()[..8]}",
                    ExternalReference = withdrawal.Id.ToString(),
                    PaymentMethod = $"{withdrawal.BankName} - {withdrawal.BankAccount}",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                };
                _context.WalletTransactions.Add(managerTx);

                // Update withdrawal request
                withdrawal.Status = WithdrawalStatus.Approved;
                withdrawal.AdminNote = adminNote;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.ProcessedByAdminId = adminId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var admin = await _context.Users.FindAsync(adminId);
                return MapToDto(withdrawal, manager.Name, admin?.Name);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WithdrawalRequestDto> RejectWithdrawalAsync(Guid requestId, long adminId, string adminNote)
        {
            var withdrawal = await _context.WithdrawalRequests
                .Include(wr => wr.Manager)
                .FirstOrDefaultAsync(wr => wr.Id == requestId);

            if (withdrawal == null)
                throw new BadRequestException(WithdrawalMessages.NotFound);

            if (withdrawal.Status != WithdrawalStatus.Pending)
                throw new BadRequestException(WithdrawalMessages.AlreadyProcessed);

            withdrawal.Status = WithdrawalStatus.Rejected;
            withdrawal.AdminNote = adminNote;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            withdrawal.ProcessedByAdminId = adminId;

            await _context.SaveChangesAsync();

            var admin = await _context.Users.FindAsync(adminId);
            return MapToDto(withdrawal, withdrawal.Manager.Name, admin?.Name);
        }

        private static WithdrawalRequestDto MapToDto(WithdrawalRequest wr, string managerName, string adminName)
        {
            return new WithdrawalRequestDto
            {
                Id = wr.Id,
                ManagerId = wr.ManagerId,
                ManagerName = managerName,
                Amount = wr.Amount,
                BankAccount = wr.BankAccount,
                BankName = wr.BankName,
                Note = wr.Note,
                Status = wr.Status,
                AdminNote = wr.AdminNote,
                CreatedAt = wr.CreatedAt,
                ProcessedAt = wr.ProcessedAt,
                ProcessedByAdminName = adminName
            };
        }
    }
}
