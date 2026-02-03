using System.Security.Claims;

namespace FoodOrderingPRM392.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static long? GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                return userId;
            return null;
        }

        public static long GetUserIdOrThrow(this ClaimsPrincipal user)
        {
            var userId = user.GetUserId();
            if (userId == null)
                throw new UnauthorizedAccessException("User is not authenticated");
            return userId.Value;
        }
    }
}
