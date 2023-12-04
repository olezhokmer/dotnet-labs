using System;
using DotnetProject.BLL;
using DotnetProject.DAL;
using DotnetProject.DTO;
using CustomDictionaryLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DotnetProject.Exceptions;

namespace DotnetProject.PL {
    [Route("users")]
    public class UsersController : Controller
    {   
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpPut()]
        public IActionResult SignUp([FromBody] UserAuthRequest userAuthRequest)
        {   
            string token = this._usersService.signUp(
                userAuthRequest.Username,
                userAuthRequest.Password
            );

            return Ok(new { token });
        }

        [HttpPost()]
        public IActionResult Login([FromBody] UserAuthRequest userAuthRequest)
        {
            string token = this._usersService.Login(
                userAuthRequest.Username,
                userAuthRequest.Password
            );

            return Ok(new { token });
        }

        [HttpGet()]
        [Authorize]
        public IActionResult GetUser()
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            PublicProfileInfo profile = this._usersService.getUserPublicProfile(userId);
                
            return Ok(new { profile });
        }

        [HttpGet("all")]
        public IActionResult GetAllUsers()
        {
            List<PublicProfileInfo> profiles = this._usersService.getAllUserProfiles();
                
            return Ok(new { profiles });
        }
    }

    [Route("friendship")]
    public class FriendshipController : Controller
    {   
        private readonly FriendshipService _friendshipService;

        public FriendshipController(FriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        [HttpPut()]
        [Authorize]
        public IActionResult SendRequest([FromBody] SendFriendshipRequestDto dto)
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            this._friendshipService.sendFriendshipRequest(userId, dto.toUserId);

            string message = "Friendship request has been sent successfuly";

            return Ok(new { message });
        }

        [HttpPatch()]
        [Authorize]
        public IActionResult Accept([FromBody] AcceptFriendshipRequestDto dto)
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            this._friendshipService.acceptFriendshipRequest(dto.friendId, userId);

            string message = "Friendship request was accepted successfuly";

            return Ok(new { message });
        }

        [HttpGet()]
        [Authorize]
        public IActionResult GetFriends()
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PublicProfileInfo> profiles = this._friendshipService.getFriendProfiles(userId);
            
            return Ok(new { profiles });
        }

        [HttpGet("possible-friends")]
        [Authorize]
        public IActionResult GetPossibleFriends()
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PublicProfileInfo> possibleFriends = this._friendshipService.getPossibleFriendProfiles(userId);
            
            return Ok(new { possibleFriends });
        }

        [HttpGet("requests")]
        [Authorize]
        public IActionResult GetFriendshipRequestors()
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PublicProfileInfo> profiles = this._friendshipService.GetProfilesWhoSentFriendshipRequest(userId);
            
            return Ok(new { profiles });
        }
    }

    [Route("messages")]
    public class MessagesController : Controller
    {   
        private readonly MessagesService _messagesService;

        public MessagesController(MessagesService messagesService)
        {
            _messagesService = messagesService;
        }

        [HttpPut()]
        [Authorize]
        public IActionResult SendMessage([FromBody] SendMessageDto dto)
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            this._messagesService.sendMessage(dto.to, userId, dto.Message);

            string message = "Message was delivered successfuly";

            return Ok(new { message });
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetMesseges(int id)
        {
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MessagesDto response = this._messagesService.GetMessagesBetweenUsers(userId, id);
            
            return Ok(new { response });
        }
    }
}