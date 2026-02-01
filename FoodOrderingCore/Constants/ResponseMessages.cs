namespace FoodOrderingCore.Constants
{
    public static class ResponseMessages
    {
        // Generic Success
        public const string Success = "Success";
        public const string OperationSuccessful = "Operation completed successfully";
        
        // Generic Errors
        public const string ResourceNotFound = "Resource not found";
        public const string Unauthorized = "Unauthorized access";
        public const string BadRequest = "Invalid request";
        public const string InternalError = "An internal error occurred";
        
        // Authentication
        public const string LoginSuccess = "Login successful";
        public const string RegisterSuccess = "Registration successful";
        public const string LogoutSuccess = "Logout successful";
        public const string InvalidCredentials = "Invalid email or password";
    }
}
