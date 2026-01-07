using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Extensions;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class UserRepository : IUserRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;

        public UserRepository(FoodOrderingContext context, IOptions<ConnectionOption> connectionOption)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
        }

        public Task CreateAsync<E>(E request)
        {
            throw new NotImplementedException();
        }

        public Task Delete<E>(E id)
        {
            throw new NotImplementedException();
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
                    @" SELECT u.Id, u.Name, u.Email, u.Phone, u.WalletAmount, u.RoleId, r.Name 'RoleName'
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
                string sql =
                    @" SELECT u.Id, u.Name, u.Email, u.Phone, u.WalletAmount, u.RoleId, r.Name 'RoleName', u.TempCartMeta
                       FROM Users u JOIN Roles r ON u.RoleId = r.Id
                       WHERE u.Email = @email AND u.Password = @password ";
                object param = new { request.Email, request.Password };

                user = await con.QueryFirstOrDefaultAsync<UserDto>(sql, param);
            }

            return user;
        }

        public async Task<int> RegisterAsync(RegisterRequest request)
        {
            int count = 0;

            using(var con = new SqlConnection(_connectionOption.FOOD))
            {
                string sql =
                    @" INSERT INTO Users (Name, Email, Phone, Password, WalletAmount, RoleId) 
                       VALUES (@Name, @Email, @Phone, @Password, 0, @RoleId) ";

                object param = new { request.Name, request.Email, request.Phone, request.Password, RoleId = RoleEnum.User };
                count = await con.ExecuteAsync(sql, param);
            }

            return count;
        }

        public async Task RecharseWalletAmountAsync(decimal money, long userId)
        {
            User user = await _context.Users.FindAsync(userId);

            user.WalletAmount += money;

            await _context.SaveChangesAsync();
        }
        public Task UpdateAysnc<E>(E request)
        {
            throw new NotImplementedException();
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
    }
}
