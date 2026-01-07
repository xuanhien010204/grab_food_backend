using FoodOrderingCore.Dto;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FoodOrderingCore.Extensions;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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

        [HttpPut("top-up")]
        [Authorize]
        public async Task<IActionResult> RecharseWalletAmountAsybc([FromBody][Range(1000, double.MaxValue, ErrorMessage = "Amount must be more than or equal 1000")] decimal money)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            await _userRepository.RecharseWalletAmountAsync(money, userId);

            return Ok(new ParentResponse { Message = "Success" });
        }


        private async Task RegisterCookie(UserDto user)
        {
            Claim[] claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim("WalletAmount", user.WalletAmount.ToString()),
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
    }
}
