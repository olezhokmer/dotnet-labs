using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DotnetProject.DTO {
    public class UserAuthRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PublicProfileInfo
    {
        public int userId { get; set; }
        public string Username { get; set; }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}