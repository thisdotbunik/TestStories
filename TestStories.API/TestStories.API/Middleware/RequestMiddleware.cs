using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Serilog.Context;

using TestStories.API.Services.Errors;
using TestStories.Common;
using System.Linq;

namespace TestStories.API
{
    public class RequestMiddleware
    {
        readonly RequestDelegate _next;

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<RequestMiddleware> logger)
        {
            var stopWatch = Stopwatch.StartNew();
            stopWatch.Start();
            context.Request.EnableBuffering();
            var requestBody = await context.Request.ReadBody();

            try
            {
                await _next(context);

                PushLogProperties(context, requestBody, stopWatch);
                logger.LogDebug("StatusCode = {StatusCode}" + ", Method = {Method}, UserId = {UserId}, Duration = {Duration}");
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)ex.GetStatusCode();
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ApiError(context.Response.StatusCode, ex.Message)));

                PushLogProperties(context, requestBody, stopWatch);

                if (context.Response.StatusCode != 422)
                {
                    logger.LogCritical(ex, "StatusCode = {StatusCode}, Method = {Method}, UserId = {UserId}, RequestBody = {RequestBody}, Message = " + ex.Message);
                }
                else
                {
                    logger.LogDebug("StatusCode = {StatusCode}" + ", Method = {Method}, UserId = {UserId}, Duration = {Duration}");
                }
            }
        }

        private static void PushLogProperties(HttpContext context, string requestBody, Stopwatch stopWatch)
        {
            stopWatch.Stop();
            LogContext.PushProperty("StatusCode", context.Response.StatusCode.ToString());
            LogContext.PushProperty("Duration", stopWatch.ElapsedMilliseconds.ToString());
            LogContext.PushProperty("Method", context.Request.Method + " " + context.Request.Path + context.Request.QueryString.Value);
            LogContext.PushProperty("RequestHost", context.Request.Host);
            LogContext.PushProperty("RequestProtocol", context.Request.Protocol);
            LogContext.PushProperty("RequestScheme", context.Request.Scheme);


            if (!string.IsNullOrEmpty(requestBody) && context.Request.ContentType == "application/json")
            {
                LogContext.PushProperty("RequestBody", requestBody);
            }

            var user = context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(user))
            {
                LogContext.PushProperty("UserEmail", user);
            }
        }
    }
}
