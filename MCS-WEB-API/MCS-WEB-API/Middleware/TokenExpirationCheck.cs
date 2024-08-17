using System.IdentityModel.Tokens.Jwt;

namespace MCS_WEB_API.Middleware
{
    public class TokenExpirationCheck
    {

        private readonly RequestDelegate _next;

        public TokenExpirationCheck(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var expiration = jwtToken.ValidTo;

                    if (expiration <= DateTime.UtcNow)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }
                }
                catch (Exception)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class TokenExpirationCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenExpirationCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenExpirationCheck>();
        }
    }



}

    

