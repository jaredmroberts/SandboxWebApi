using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SandboxWebApi.Filters
{
    /// <summary>
    /// ExceptionFilter registered globally in Startup.cs
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<CustomExceptionFilterAttribute> _logger;

        public CustomExceptionFilterAttribute(ILogger<CustomExceptionFilterAttribute> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            Thread.CurrentPrincipal = context.HttpContext.User;

            bool isWarning = false;

            var httpStatusCode = HttpStatusCode.InternalServerError;
            var mediaType = "application/json";
            var jsonResult = new JsonResult(new
            {
                Name = context.Exception.GetType().Name,
                Message = context.Exception.Message,
                ExceptionMessage = context.Exception.Message
            });

            switch (context.Exception)
            {
                case ArgumentNullException:
                case UnauthorizedAccessException:
                case InvalidOperationException:
                    httpStatusCode = HttpStatusCode.BadRequest;
                    break;
            }

            #region Log Request Info

            var requestInfo = GetRequestFromContext(context);
            var requestInfoString = string.Join(",", requestInfo.Select(x => string.Join(":", x.Key, x.Value)));

            if (isWarning)
            {
                _logger.LogWarning(context.Exception, $"{context.HttpContext.Request.Method}: '{context.HttpContext.Request.Path}'{requestInfoString}");
            }
            else
            {
                _logger.LogError(context.Exception, $"{context.HttpContext.Request.Method}: '{context.HttpContext.Request.Path}'{requestInfoString}");
            }

            #endregion

            context.HttpContext.Response.StatusCode = (int)httpStatusCode;
            context.HttpContext.Response.ContentType = mediaType;
            context.Result = jsonResult;

            await base.OnExceptionAsync(context);
        }

        private Dictionary<string, string> GetRequestFromContext(ExceptionContext context)
        {
            var requestInfo = new Dictionary<string, string>();

            try
            {
                // Endpoint
                requestInfo.Add("Protocol", context.HttpContext.Request.Protocol);
                requestInfo.Add("Scheme", context.HttpContext.Request.Scheme);
                requestInfo.Add("Host", context.HttpContext.Request.Host.Value);
                requestInfo.Add("Path", context.HttpContext.Request.Path.Value);
                requestInfo.Add("Method", context.HttpContext.Request.Method);
                requestInfo.Add("ContentType", context.HttpContext.Request.ContentType);
                requestInfo.Add("ContentLength", context.HttpContext.Request.ContentLength.HasValue ? context.HttpContext.Request.ContentLength.Value.ToString() : string.Empty);
                // Query
                foreach (var query in context.HttpContext.Request.Query)
                {
                    requestInfo.Add($"Query.{query.Key}", query.Value);
                }

                // Headers
                var notLogHeaders = new string[] { "cookie", "authorization" };

                foreach (var header in context.HttpContext.Request.Headers)
                {
                    // Skip some headers
                    if (notLogHeaders.Contains(header.Key.ToLower()))
                    {
                        continue;
                    }

                    requestInfo.Add($"Header.{header.Key}", header.Value);
                }

                // Body
                context.HttpContext.Request.EnableBuffering();
                context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

                using (var stream = new StreamReader(context.HttpContext.Request.Body))
                {
                    string body = stream.ReadToEndAsync().Result;
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        requestInfo.Add("Body", body);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            return requestInfo;
        }
    }
}
