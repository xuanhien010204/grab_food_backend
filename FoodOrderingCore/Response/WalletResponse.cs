namespace FoodOrderingCore.Response
{
    /// Response for wallet balance and info
    public class WalletResponse
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public decimal Balance { get; set; }
        public string FormattedBalance => Balance.ToString("N0") + " VND";
        public DateTime LastUpdated { get; set; }
    }
}
