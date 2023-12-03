using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DotnetProject.DAL;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetProject.Exceptions;
using DotnetProject.Configuration;
using Microsoft.Extensions.Options;


namespace DotnetProject.BLL
{
    public class UsersService 
    {
        private readonly DotnetProjectDbContext _context;
        private readonly JwtService _jwtService;

        public UsersService(JwtService jwtService, DotnetProjectDbContext context)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public User getUserByUsername(string userName)
        {
            return _context.Users.SingleOrDefault(u => u.username == userName);
        }

        public string Login(string userName, string pass)
        {
            User user = this.getUserByUsername(userName);

            if (user == null)
            {
                throw new NotFoundException("User by this username was not found.");
            }

            if (user.password != pass)
            {
                throw new ForbiddenException("Password is wrong.");
            }

            return this._jwtService.GenerateJwtToken(user.userId.ToString());
        }

        public string signUp(string userName, string pass)
        {
            User user = this.createUser(userName, pass);

            return this._jwtService.GenerateJwtToken(user.userId.ToString());
        }

        public User createUser(string userName, string pass) 
        {
            if (_context.Users.Any(u => u.username == userName))
            {
                throw new BadRequestException("User with the same username already exists.");
            }

            User user = new User { username = userName, password = pass };
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }
    }

    public class MessagesService
    {
        private readonly DotnetProjectDbContext _context;

        public MessagesService(DotnetProjectDbContext context)
        {
            _context = context;
        }

        public Message sendMessage(int to, int from, string messageText) {
            Message message = new Message { fromUserId = from, toUserId = to, message = messageText };

            _context.Messages.Add(message);
            _context.SaveChanges();

            return message;
        }

        public Dictionary<User, List<Message>> getMessages(int userId)
        {
            var userMessages = _context.Messages
                .Where(message => message.fromUserId == userId || message.toUserId == userId)
                .ToList();

            var distinctUserIds = userMessages.Select(message => message.fromUserId == userId ? message.toUserId : message.fromUserId).Distinct();

            var userMessageDictionary = new Dictionary<User, List<Message>>();

            foreach (var otherUserId in distinctUserIds)
            {
                var otherUser = _context.Users.FirstOrDefault(user => user.userId == otherUserId);

                if (otherUser != null)
                {
                    var messagesBetweenUsers = userMessages
                        .Where(message =>
                            (message.fromUserId == userId && message.toUserId == otherUserId) ||
                            (message.fromUserId == otherUserId && message.toUserId == userId))
                        .ToList();

                    userMessageDictionary.Add(otherUser, messagesBetweenUsers);
                }
            }

            return userMessageDictionary;
        }

    }

    public class FriendshipService
    {
        private readonly DotnetProjectDbContext _context;

        public FriendshipService(DotnetProjectDbContext context)
        {
            _context = context;
        }

        public FriendshipRequest sendFriendshipRequest(int from, int to)
        {
            FriendshipRequest request = new FriendshipRequest
            {
                fromUserId = from,
                toUserId = to,
                isAccepted = false,
            };

            _context.FriendshipRequests.Add(request);
            _context.SaveChanges();

            return request;
        }

        public void acceptFriendshipRequest(int friendId, int targetId)
        {
            var friendshipRequests = _context.FriendshipRequests
                .Where(r => (r.fromUserId == friendId && r.toUserId == targetId) || (r.toUserId == friendId && r.fromUserId == targetId) )
                .ToList();

            foreach (var friendshipRequest in friendshipRequests)
            {
                friendshipRequest.isAccepted = true;
            }

            _context.SaveChanges();
        }

        public List<User> getFriends(int userId)
        {
            var friendRequests = _context.FriendshipRequests
                .Where(request => (request.toUserId == userId || request.fromUserId == userId) && request.isAccepted)
                .ToList();

            var friends = (from request in friendRequests
                        join user in _context.Users on request.fromUserId equals user.userId
                        select user)
                        .Distinct()
                        .ToList();

            return friends;
        }
    }

    public class JwtService
    {
        private readonly string _key;

        public JwtService(IOptions<ProjectConfiguration> configuration)
        {
            _key = configuration.Value.JwtSecret;
        }

        public string GenerateJwtToken(string userId)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(_key);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}