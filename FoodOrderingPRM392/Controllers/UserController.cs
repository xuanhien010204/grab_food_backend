using FoodOrderingCore.Dto;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IStoreRepository _storeRepository;

        public UserController(IUserRepository userRepository, IStoreRepository storeRepository)
        {
            _userRepository = userRepository;
            _storeRepository = storeRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            UserDto user = await _userRepository.LoginAsync(request);

            if (user == null) throw new BadRequestException();

            await RegisterCookie(user);

            return Ok(new ParentResultResponse { Message = "Success", Result = user});
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            int count = await _userRepository.RegisterAsync(request);

            if (count == 0) return BadRequest();

            return Ok(new ParentResponse { Message = "Register success" });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfileAsync()
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            UserDto user = await _userRepository.GetById(userId);

            return Ok(new ParentResultResponse { Message = "Success", Result = user });
        }

        private async Task RegisterCookie(UserDto user)
        {
            Claim[] claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim("RoleId", user.RoleId.ToString()),
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        [HttpGet("sign-out")]
        [Authorize]
        public async Task SignOutAsync()
        {
            await HttpContext.SignOutAsync();
        }

        [HttpPatch("temp-data")]
        [Authorize]
        public async Task<IActionResult> SaveTempCartMetaAsync([FromBody][Required] Cart cart)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            await _userRepository.UpdateTempCartMetaAsync(cart, userId);

            return Ok(new ParentResponse{ Message = "Success" });
        }

        [HttpDelete("temp-data")]
        [Authorize]
        public async Task<IActionResult> DeleteTempCartMetaAsync()
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            await _userRepository.DeleteTempCartMetaAsync(userId);

            return Ok(new ParentResponse { Message = "Success" });
        }
        [HttpPut("edit-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfle([FromBody] UserEdit user)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var success = await _userRepository.UpdateUser(userId, user);

            return success
                ? Ok(new ParentResponse { Message = "Success" })
                : BadRequest(new ParentResponse { Message = "Fail" });

        }
        [HttpPut("lock{userId}")]
        [Authorize]
        public async Task<IActionResult> LockUser(long userId)
        {
            var success = await _userRepository.LockUser(userId);

            return success
                ? Ok(new ParentResponse { Message = "Success" })
                : BadRequest(new ParentResponse { Message = "Fail" });

        }

        // User register as Manager - create store pending approval
        [HttpPost("register-manager")]
        [Authorize]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterManagerRequest request)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            // Check if the user already has a store
            var existingStore = await _storeRepository.GetStoreByManagerId(userId);
            if (existingStore != null)
                return BadRequest(new ParentResponse { Message = "You have already registered a store" });

            var store = await _storeRepository.CreateStoreAsync(request, userId);

            if (store == null)
                return BadRequest(new ParentResponse { Message = "Register failed" });

            return Ok(new ParentResultResponse
            {
                Message = "Register Manager successfully, waiting for Admin approval",
                Result = store
            });
        }

        // Admin approve store - user will become Manager
        [HttpPut("approve-store/{storeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveStore(long storeId)
        {
            var success = await _storeRepository.ApproveStoreAsync(storeId);

            return success
                ? Ok(new ParentResponse { Message = "Approve store successfully, user has become Manager" })
                : BadRequest(new ParentResponse { Message = "Approve failed" });
        }

        // Admin view list of pending stores
        [HttpGet("pending-stores")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingStores()
        {
            var list = await _storeRepository.GetPendingStores();

            return Ok(new ParentResultResponse { Message = "Success", Result = list });
        }
    }
}
