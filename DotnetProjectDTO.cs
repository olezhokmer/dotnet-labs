using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using DotnetProject.DAL;

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

    public class SendFriendshipRequestDto
    {
        public int toUserId { get; set; }
    }

    public class AcceptFriendshipRequestDto
    {
        public int friendId { get; set; }
    }

    public class MessagesDto
    {
        public PublicProfileInfo user { get; set; }
        public List<Message> messages { get; set; }
    }

    public class SendMessageDto
    {
        public int to { get; set; }
        public string Message { get; set; }
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