using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace MCS_WEB_API.Middleware
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var expiration = jwtToken.ValidTo;

                if (expiration <= DateTime.UtcNow)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
            catch (Exception)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
