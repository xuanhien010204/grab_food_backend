namespace FoodOrderingCore.Exceptions
{
    // Exception for wallet operations
    public class WalletException : Exception
    {
        public WalletException(string message) : base(message) { }
        
        public WalletException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    // Exception when deposit fails
    public class DepositFailedException : WalletException
    {
        public DepositFailedException(string message) : base(message) { }
    }

    // Exception when IPN signature is invalid     
    public class InvalidSignatureException : WalletException
    {
        public InvalidSignatureException() 
            : base("Chữ ký MoMo không hợp lệ") { }
    }
}
