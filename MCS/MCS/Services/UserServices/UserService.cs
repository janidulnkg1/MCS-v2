using System.Security.Claims;

namespace MCS.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor? _contextAccessor;

        public UserService(IHttpContextAccessor? contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string GetMyDesignation()
        {
            var result = string.Empty;
            var httpContext = _contextAccessor?.HttpContext;

            if (httpContext != null)
            {
                var user = httpContext.User;
                if (user != null)
                {
                    result = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                }
            }

            return result;
        }
    }
}

