using System;
using Microsoft.AspNetCore.Http;

namespace DotnetProject.Exceptions 
{
    public class HttpException : Exception
    {
        public int StatusCode { get; }

        public HttpException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : HttpException
    {
        public NotFoundException(string message) : base(StatusCodes.Status404NotFound, message) { }
    }

    public class BadRequestException : HttpException
    {
        public BadRequestException(string message) : base(StatusCodes.Status400BadRequest, message) { }
    }

    public class ForbiddenException : HttpException
    {
        public ForbiddenException(string message) : base(StatusCodes.Status403Forbidden, message) { }
    }

    public class UnauthorizedException : HttpException
    {
        public UnauthorizedException(string message) : base(StatusCodes.Status401Unauthorized, message) { }
    }
}