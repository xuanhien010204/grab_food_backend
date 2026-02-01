using FoodOrderingCore.Constants;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingPRM392.Extensions;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/vouchers")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherRepository _voucherRepository;

        public VoucherController(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherRequest request)
        {
            var voucher = await _voucherRepository.CreateVoucherAsync(request);

            return Ok(new ParentResultResponse
            {
                Message = VoucherMessages.CreateSuccess,
                Result = voucher
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVoucher(Guid id)
        {
            var voucher = await _voucherRepository.GetVoucherByIdAsync(id);

            if (voucher == null)
                return NotFound(new ParentResponse { Message = VoucherMessages.NotFound });

            return Ok(new ParentResultResponse
            {
                Message = VoucherMessages.GetSuccess,
                Result = voucher
            });
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetVoucherByCode(string code)
        {
            var voucher = await _voucherRepository.GetVoucherByCodeAsync(code);

            if (voucher == null)
                return NotFound(new ParentResponse { Message = VoucherMessages.NotFound });

            return Ok(new ParentResultResponse
            {
                Message = VoucherMessages.GetSuccess,
                Result = voucher
            });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveVouchers([FromQuery] long? storeId = null)
        {
            var vouchers = await _voucherRepository.GetActiveVouchersAsync(storeId);

            return Ok(new ParentResultResponse
            {
                Message = VoucherMessages.GetSuccess,
                Result = vouchers
            });
        }

        // Get available vouchers for user based on order amount
        [HttpGet("available")]
        [Authorize]
        public async Task<IActionResult> GetAvailableVouchers(
            [FromQuery] decimal orderAmount,
            [FromQuery] long? storeId = null)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var vouchers = await _voucherRepository.GetUserAvailableVouchersAsync(
                userId.Value, orderAmount, storeId);

            return Ok(new ParentResultResponse
            {
                Message = VoucherMessages.GetSuccess,
                Result = vouchers
            });
        }

        // Apply voucher to calculate discount
        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _voucherRepository.ApplyVoucherAsync(request, userId.Value);

            if (!result.Success)
                return BadRequest(new ParentResponse { Message = result.Message });

            return Ok(new ParentResultResponse
            {
                Message = result.Message,
                Result = new
                {
                    Voucher = result.Voucher,
                    DiscountAmount = result.DiscountAmount,
                    FinalAmount = result.FinalAmount
                }
            });
        }

        // Update voucher (Admin/Manager only)
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherRequest request)
        {
            var voucher = await _voucherRepository.UpdateVoucherAsync(id, request);

            return Ok(new ParentResultResponse
            {
                Message = VoucherMessages.UpdateSuccess,
                Result = voucher
            });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeactivateVoucher(Guid id)
        {
            var result = await _voucherRepository.DeactivateVoucherAsync(id);

            if (!result)
                return NotFound(new ParentResponse { Message = VoucherMessages.NotFound });

            return Ok(new ParentResponse { Message = VoucherMessages.DeactivateSuccess });
        }
    }
}
