using System.Security.Claims;

namespace HelpDeskHQ.API.Common
{
    /// <summary>
    /// Extension methods for reading the current user's identity out of
    /// JWT claims. Removes the need for every controller to redefine
    /// GetUserId()/GetUserRole() privately.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User identity is missing or invalid.");
            }

            return userId;
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }
    }
}