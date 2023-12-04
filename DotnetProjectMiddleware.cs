using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using DotnetProject.Exceptions;
using DotnetProject.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DotnetProject.Middleware 
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpException httpException)
            {
                await HandleHttpExceptionAsync(context, httpException);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleHttpExceptionAsync(HttpContext context, HttpException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception.StatusCode;

            var errorDetails = new ErrorDetails
            {
                StatusCode = exception.StatusCode,
                Message = exception.Message
            };

            await context.Response.WriteAsync(errorDetails.ToString());
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Console.WriteLine(exception.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var errorDetails = new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error."
            };

            await context.Response.WriteAsync(errorDetails.ToString());
        }
    }
}