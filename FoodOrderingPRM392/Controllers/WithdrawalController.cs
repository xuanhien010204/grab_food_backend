using FoodOrderingCore.Constants;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using FoodOrderingPRM392.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/withdrawal")]
    [ApiController]
    [Authorize]
    public class WithdrawalController : ControllerBase
    {
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IUserRepository _userRepository;

        public WithdrawalController(IWithdrawalRepository withdrawalRepository, IUserRepository userRepository)
        {
            _withdrawalRepository = withdrawalRepository;
            _userRepository = userRepository;
        }

        // Manager creates a withdrawal request
        [HttpPost]
        public async Task<IActionResult> CreateWithdrawalRequest([FromBody] CreateWithdrawalRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WithdrawalMessages.Unauthorized });

            var result = await _withdrawalRepository.CreateWithdrawalRequestAsync(userId.Value, request);
            return Ok(new ParentResultResponse { Message = WithdrawalMessages.CreateSuccess, Result = result });
        }

        // Manager views their withdrawal requests
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WithdrawalMessages.Unauthorized });

            var requests = await _withdrawalRepository.GetManagerWithdrawalRequestsAsync(userId.Value);
            return Ok(new ParentResultResponse { Message = WithdrawalMessages.GetSuccess, Result = requests });
        }

        // Admin views all pending requests
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WithdrawalMessages.Unauthorized });

            // Verify user is admin
            var user = await _userRepository.GetById(userId.Value);
            if (user == null || user.RoleId != (int)RoleEnum.Admin)
                return Forbid();

            var requests = await _withdrawalRepository.GetAllPendingRequestsAsync();
            return Ok(new ParentResultResponse { Message = WithdrawalMessages.GetSuccess, Result = requests });
        }

        // Admin approves a withdrawal request
        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> ApproveWithdrawal(Guid id, [FromBody] ProcessWithdrawalRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WithdrawalMessages.Unauthorized });

            var user = await _userRepository.GetById(userId.Value);
            if (user == null || user.RoleId != (int)RoleEnum.Admin)
                return Forbid();

            var result = await _withdrawalRepository.ApproveWithdrawalAsync(id, userId.Value, request?.AdminNote);
            return Ok(new ParentResultResponse { Message = WithdrawalMessages.ApproveSuccess, Result = result });
        }

        // Admin rejects a withdrawal request
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> RejectWithdrawal(Guid id, [FromBody] ProcessWithdrawalRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = WithdrawalMessages.Unauthorized });

            var user = await _userRepository.GetById(userId.Value);
            if (user == null || user.RoleId != (int)RoleEnum.Admin)
                return Forbid();

            var result = await _withdrawalRepository.RejectWithdrawalAsync(id, userId.Value, request?.AdminNote);
            return Ok(new ParentResultResponse { Message = WithdrawalMessages.RejectSuccess, Result = result });
        }
    }
}
