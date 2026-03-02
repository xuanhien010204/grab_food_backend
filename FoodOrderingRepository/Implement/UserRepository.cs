using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Extensions;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Dapper.SqlMapper;

namespace FoodOrderingRepository.Implement
{
    public class UserRepository : IUserRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRepository(
            FoodOrderingContext context, 
            IOptions<ConnectionOption> connectionOption,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            IEnumerable<UserDto> list = null;
            await Task.CompletedTask;
            return list;
        }

        public async Task<UserDto> GetById(long id)
        {
            UserDto user = null;

            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                string sql =
                    @" SELECT u.Id, u.Name, u.Email, u.Phone, u.RoleId, r.Name 'RoleName'
                       FROM Users u JOIN Roles r ON u.RoleId = r.Id
                       WHERE u.Id = @id ";
                object param = new { id };

                user = await con.QueryFirstOrDefaultAsync<UserDto>(sql, param);
            }

            return user;
        }

        public async Task<UserDto> LoginAsync(LoginRequest request)
        {
            UserDto user = null;

            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                // First, get user by email only
                string sql =
                    @" SELECT u.Id, u.Name, u.Email, u.Phone, u.RoleId, u.Password,
                              r.Name 'RoleName', u.TempCartMeta
                       FROM Users u JOIN Roles r ON u.RoleId = r.Id
                       WHERE u.Email = @email ";
                object param = new { request.Email };

                var userWithPassword = await con.QueryFirstOrDefaultAsync<dynamic>(sql, param);

                // Verify password hash
                if (userWithPassword != null)
                {
                    var tempUser = new User { Password = userWithPassword.Password };
                    var verificationResult = _passwordHasher.VerifyHashedPassword(
                        tempUser, 
                        userWithPassword.Password, 
                        request.Password
                    );

                    if (verificationResult == PasswordVerificationResult.Success ||
                        verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        user = new UserDto
                        {
                            Id = userWithPassword.Id,
                            Name = userWithPassword.Name,
                            Email = userWithPassword.Email,
                            Phone = userWithPassword.Phone,
                            //WalletAmount = userWithPassword.WalletAmount,
                            RoleId = userWithPassword.RoleId,
                            RoleName = userWithPassword.RoleName,
                            TempCartMeta = userWithPassword.TempCartMeta
                        };
                    }
                }
            }

            return user;
        }

        public async Task<int> RegisterAsync(RegisterRequest request)
        {
            int count = 0;

            // Hash password with PBKDF2 of identity framework
            var user = new User { Password = request.Password };
            string hashedPassword = _passwordHasher.HashPassword(user, request.Password);

            using(var con = new SqlConnection(_connectionOption.FOOD))
            {
                string sql =
                    @" INSERT INTO Users (Name, Email, Phone, Password, WalletAmount, RoleId) 
                       VALUES (@Name, @Email, @Phone, @Password, 0, @RoleId) ";

                object param = new 
                { 
                    request.Name, 
                    request.Email, 
                    request.Phone, 
                    Password = hashedPassword,
                    RoleId = RoleEnum.User 
                };
                count = await con.ExecuteAsync(sql, param);
            }

            return count;
        }

        public async Task<Cart> GetTempCartAsync(long userId)
        {
            string tempCartMeta = null;

            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                string sql = "SELECT TempCartMeta FROM Users WHERE Id = @userId";
                tempCartMeta = await con.QueryFirstOrDefaultAsync<string>(sql, new { userId });
            }

            if (string.IsNullOrEmpty(tempCartMeta))
                return new Cart { OrderList = new Dictionary<string, CartItem>() };

            try
            {
                return tempCartMeta.ToObject<Cart>() ?? new Cart { OrderList = new Dictionary<string, CartItem>() };
            }
            catch
            {
                return new Cart { OrderList = new Dictionary<string, CartItem>() };
            }
        }

        public async Task UpdateTempCartMetaAsync(Cart cart, long userId)
        {
            User user = await _context.Users.FindAsync(userId);
            user.TempCartMeta = cart.ToJsonString();

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTempCartMetaAsync(long userId)
        {
            User user = await _context.Users.FindAsync(userId);
            user.TempCartMeta = null;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateUser(long userId, UserEdit user)
        {
            User entity = await _context.Users.FindAsync(userId);
            if (entity == null) return false;
            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                entity.Name = user.Name;
            }
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                entity.Email = user.Email;
            }
            if (!string.IsNullOrWhiteSpace(user.Phone))
            {
                entity.Phone = user.Phone;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> LockUser(long userId)
        {
            User entity = await _context.Users.FindAsync(userId);
            if (entity == null) return false;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
