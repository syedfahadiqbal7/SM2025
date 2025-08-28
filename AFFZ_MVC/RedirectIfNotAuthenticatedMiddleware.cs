using Microsoft.AspNetCore.Http.Extensions;

namespace AFFZ_Customer
{
    public class RedirectIfNotAuthenticatedMiddleware
    {
        private readonly RequestDelegate _next;

        public RedirectIfNotAuthenticatedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is for the MerchantList and the user is not authenticated
            //var userId = context.Session.GetEncryptedString("UserId");

            // Check if the request is for the MerchantList and the user is not authenticated
            if (context.Request.Path.StartsWithSegments("/MerchantList/SelectedMerchantList") && !context.Session.Keys.Contains("UserId"))
            {
                var returnUrl = Uri.EscapeDataString(context.Request.GetDisplayUrl());
                var loginUrl = $"http://192.168.0.115:7195/Login/Index?returnUrl={returnUrl}";
                context.Response.Redirect(loginUrl);
                //return;
            }

            await _next(context);
        }
    }
}
