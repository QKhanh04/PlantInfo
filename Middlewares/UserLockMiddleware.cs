using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Middlewares
{
    public class UserLockMiddleware
    {
        private readonly RequestDelegate _next;

        public UserLockMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IAuthService userService)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userIdStr = context.User.FindFirst("UserId")?.Value;
                System.Console.WriteLine(" user của người dùng khi bị lock" + userIdStr);
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        var userResult = await userService.GetUserByIdAsync(userId);
                        if (userResult != null && userResult.Data != null && userResult.Data.IsLocked)
                        {
                            await context.SignOutAsync();

                            context.Response.Redirect("/Auth/Authentication?locked=true");
                            return;
                        }
                    }
                }
            }
            await _next(context);
        }
    }
}