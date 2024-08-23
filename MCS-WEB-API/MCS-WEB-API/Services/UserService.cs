using System.Security.Claims;

namespace MCS_WEB_API.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetMyUsername()
        {
            var result = string.Empty;
            var user = _httpContextAccessor.HttpContext?.User; // Null check for HttpContext.User
            if (user != null)
            {
                result = user.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }

        public string GetMyDesignation()
        {
            var result = string.Empty;
            var user = _httpContextAccessor.HttpContext?.User; // Null check for HttpContext.Designation
            if (user != null)
            {
                result = user.FindFirstValue(ClaimTypes.Role);
            }
            return result;
        }

    }
}
