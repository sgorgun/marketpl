using Business.Validation;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;

namespace WebApi.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next ?? throw new MarketException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new MarketException(nameof(context));
            }

            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context == null)
            {
                throw new MarketException(nameof(context));
            }

            var code = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case KeyNotFoundException:
                    code = HttpStatusCode.NotFound;
                    break;
                case ArgumentNullException:
                case ArgumentException:
                case MarketException:
                    code = HttpStatusCode.BadRequest;
                    break;
            }

            context.Response.ContentType = "application/text";
            context.Response.StatusCode = (int)code;

            var result = exception.Message;

            return context.Response.WriteAsync(result);
        }
    }
}
