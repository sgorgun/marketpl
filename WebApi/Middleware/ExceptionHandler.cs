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
            catch (KeyNotFoundException ex)
            {
                await HandleExceptionAsync(context, ex);
            }
            catch (ArgumentNullException ex)
            {
                await HandleExceptionAsync(context, ex);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, ex);
            }
            catch (MarketException ex)
            {
                await HandleExceptionAsync(context, ex);
            }
            catch (InvalidOperationException ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context == null)
            {
                throw new MarketException(nameof(context));
            }

            var code = GetStatusCode(exception);

            context.Response.ContentType = "application/text";
            context.Response.StatusCode = (int)code;

            await context.Response.WriteAsync(exception.Message);
        }

        private static HttpStatusCode GetStatusCode(Exception exception)
        {
            return exception switch
            {
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentNullException or ArgumentException or MarketException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}
