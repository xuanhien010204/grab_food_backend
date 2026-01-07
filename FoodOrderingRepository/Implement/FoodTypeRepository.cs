using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Dto;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class FoodTypeRepository : IFoodTypeRepository
    {
        private readonly FoodOrderingContext context;
        private readonly ConnectionOption connectionOption;

        public FoodTypeRepository(FoodOrderingContext context, IOptions<ConnectionOption> connectionOption)
        {
            this.context = context;
            this.connectionOption = connectionOption.Value;
        }

        public async Task<IEnumerable<FoodTypeDto>> GetAllFoodTypeAsync()
        {
            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, Name, ImgSrc 
                       FROM FoodTypes
                     ";
                return await con.QueryAsync<FoodTypeDto>(sql);
            }
        }
    }
}
