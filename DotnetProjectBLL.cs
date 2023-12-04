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

        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public List<PublicProfileInfo> mapUsersToProfiles(List<User> users)
        {
            List<PublicProfileInfo> profiles = users.ConvertAll(
                u => this.mapToPublicProfile(u)
            );

            return profiles;
        }

        public List<PublicProfileInfo> getAllUserProfiles()
        {
            List<User> allUsers = this.GetAllUsers();

            List<PublicProfileInfo> profiles = this.mapUsersToProfiles(allUsers);

            return profiles;
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

        public MessagesDto GetMessagesBetweenUsers(int currentUserId, int messengerId)
        {
            List<Message> userMessages = _context.Messages
                .Where(message =>
                    (message.fromUserId == currentUserId && message.toUserId == messengerId) ||
                    (message.fromUserId == messengerId && message.toUserId == currentUserId))
                .ToList();

            User otherUser = _context.Users.FirstOrDefault(user => user.userId == messengerId);

            List<Message> messagesBetweenUsers = userMessages.OrderBy(message => message.messageId).ToList();

            MessagesDto messagesDto = new MessagesDto
            {
                user = this._usersService.mapToPublicProfile(otherUser),
                messages = messagesBetweenUsers
            };

            return messagesDto;
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

        public List<User> GetUsersWhoSentFriendshipRequest(int userId)
        {
            List<User> usersWithRequests = _context.FriendshipRequests
                .Where(req => req.toUserId == userId && req.isAccepted == false)
                .Select(req => req.FromUser)
                .Distinct()
                .ToList();

            return usersWithRequests;
        }

        public List<PublicProfileInfo> GetProfilesWhoSentFriendshipRequest(int userId)
        {
            List<User> usersWithRequests = this.GetUsersWhoSentFriendshipRequest(userId);

            List<PublicProfileInfo> profiles = this._usersService.mapUsersToProfiles(usersWithRequests);

            return profiles;
        }

        public List<User> getPossibleFriends(int userId) {
            List<User> usersWithoutRequests = _context.Users
                .Where(u => u.userId != userId && 
                            !u.SentFriendshipRequests.Any(fr => fr.ToUser.userId == userId) &&
                            !u.ReceivedFriendshipRequests.Any(fr => fr.FromUser.userId == userId))
                .ToList();

            return usersWithoutRequests;
        }

        public List<PublicProfileInfo> getPossibleFriendProfiles(int userId)
        {
            List<User> friends = this.getPossibleFriends(userId);

            List<PublicProfileInfo> profiles = this._usersService.mapUsersToProfiles(friends);

            return profiles;
        }

        public List<PublicProfileInfo> getFriendProfiles(int userId)
        {
            List<User> friends = this.getFriends(userId);

            List<PublicProfileInfo> profiles = this._usersService.mapUsersToProfiles(friends);

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