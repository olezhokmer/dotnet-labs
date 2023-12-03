using System;
using DotnetProject.BLL;
using DotnetProject.DAL;
using DotnetProject.DTO;
using CustomDictionaryLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
            return Content("User");
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
        public IActionResult SendRequest()
        {
            return Content("Send request");
        }

        [HttpPatch()]
        public IActionResult Accept()
        {
            return Content("Accept request");
        }

        [HttpGet()]
        public IActionResult GetFriends()
        {
            return Content("Get friends");
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
        public IActionResult SendMessage()
        {
            return Content("Send message");
        }

        [HttpGet()]
        public IActionResult GetMesseges()
        {
            return Content("Get messeges");
        }
    }
}