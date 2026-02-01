using FoodOrderingCore.Constants;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingPRM392.Extensions;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/addresses")]
    [ApiController]
    [Authorize]
    public class DeliveryAddressController : ControllerBase
    {
        private readonly IDeliveryAddressRepository _addressRepository;

        public DeliveryAddressController(IDeliveryAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        // Get all delivery addresses for current user
        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var addresses = await _addressRepository.GetUserAddressesAsync(userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = DeliveryAddressMessages.GetSuccess,
                Result = addresses
            });
        }

        // Get address by ID
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetAddress(long id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _addressRepository.GetAddressByIdAsync(id, userId.Value);

            if (address == null)
                return NotFound(new ParentResponse { Message = DeliveryAddressMessages.NotFound });

            return Ok(new ParentResultResponse
            {
                Message = DeliveryAddressMessages.GetSuccess,
                Result = address
            });
        }

        // Get default address
        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultAddress()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _addressRepository.GetDefaultAddressAsync(userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = DeliveryAddressMessages.GetSuccess,
                Result = address
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateDeliveryAddressRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _addressRepository.CreateAddressAsync(request, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = DeliveryAddressMessages.CreateSuccess,
                Result = address
            });
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateAddress(long id, [FromBody] UpdateDeliveryAddressRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _addressRepository.UpdateAddressAsync(id, request, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = DeliveryAddressMessages.UpdateSuccess,
                Result = address
            });
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _addressRepository.DeleteAddressAsync(id, userId.Value);

            if (!result)
                return NotFound(new ParentResponse { Message = DeliveryAddressMessages.NotFound });

            return Ok(new ParentResponse { Message = DeliveryAddressMessages.DeleteSuccess });
        }

        // Set address as default
        [HttpPut("{id:long}/default")]
        public async Task<IActionResult> SetDefaultAddress(long id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _addressRepository.SetDefaultAddressAsync(id, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = DeliveryAddressMessages.SetDefaultSuccess,
                Result = address
            });
        }
    }
}
