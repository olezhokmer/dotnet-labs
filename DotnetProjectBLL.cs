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
using DotnetProject.DTO;

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

        public User getUserByUserId(int userId)
        {
            return _context.Users.SingleOrDefault(u => u.userId == userId);
        }

        public PublicProfileInfo getUserPublicProfile(int userId)
        {
            User user = this.getUserByUserId(userId);

            return this.mapToPublicProfile(user);
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

        public PublicProfileInfo mapToPublicProfile(User user)
        {
            PublicProfileInfo profile = new PublicProfileInfo();

            profile.userId = user.userId;
            profile.Username = user.username;

            return profile;
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
        private readonly UsersService _usersService;

        public MessagesService(DotnetProjectDbContext context, UsersService usersService)
        {
            _context = context;
            _usersService = usersService;
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

        public List<MessagesDto> getMessagesDtosList(int userId)
        {
            Dictionary<User, List<Message>> userMessagesDict = this.getMessages(userId);

            List<MessagesDto> messagesDtoList = userMessagesDict
                .Select(pair => new MessagesDto
                {
                    user = this._usersService.mapToPublicProfile(pair.Key),
                    messages = pair.Value
                })
                .ToList();

            return messagesDtoList;
        }
    }

    public class FriendshipService
    {
        private readonly DotnetProjectDbContext _context;
        private readonly UsersService _usersService;

        public FriendshipService(DotnetProjectDbContext context, UsersService usersService)
        {
            _context = context;
            _usersService = usersService;
        }

        public FriendshipRequest sendFriendshipRequest(int from, int to)
        {
            FriendshipRequest found = _context
                .FriendshipRequests
                .SingleOrDefault(
                    r => r.fromUserId == from && r.toUserId == to
                );

            if (found != null)
            {
                throw new BadRequestException("You have already sent a friendship request.");
            }

            FriendshipRequest reversedRequest = _context
                .FriendshipRequests
                .SingleOrDefault(
                    r => r.fromUserId == to && r.toUserId == from
                );

            if (reversedRequest != null)
            {
                throw new BadRequestException("This person has already sent you a friendship request.");
            }

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

            if (friendshipRequests.Count == 0)
            {
                throw new NotFoundException("Friendship request was not found.");
            }

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

        public List<PublicProfileInfo> getFriendProfiles(int userId)
        {
            List<User> friends = this.getFriends(userId);

            List<PublicProfileInfo> profiles = friends.ConvertAll(
                f => this._usersService.mapToPublicProfile(f)
            );

            return profiles;
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