using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string token = null;

        // Lấy token từ header Authorization
        StringValues authorizationHeader;
        if (context.Request.Headers.TryGetValue("Authorization", 
            out authorizationHeader) && authorizationHeader.Count > 0)
        {
            var bearerToken = authorizationHeader[0];
            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
        }

        if (token != null)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken != null)
                {
                    var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                    var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value.ToString();
                    var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value.ToString();


                    var expTimestamp = jwtToken.ValidTo;
                    var expiration = expTimestamp.ToUniversalTime(); // Convert to UTC
                    

                    if (DateTime.UtcNow > expiration)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Token has expired");
                        return;
                    }

                    // Lưu thông tin người dùng vào context để sử dụng trong các API controllers
                    context.Items["UserId"] = userId;
                    context.Items["Username"] = username;
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token");
                return;
            }
        }

        // Chuyển tiếp yêu cầu đến middleware tiếp theo hoặc controller
        await _next(context);
    }
}
