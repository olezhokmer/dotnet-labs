using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DotnetProject.DAL;

namespace DotnetProject.BLL
{
    class UsersService 
    {
        private readonly DotnetProjectDbContext _context;

        public UsersService(DotnetProjectDbContext context)
        {
            _context = context;
        }

        public User getUserByUsername(string userName)
        {
            return _context.Users.SingleOrDefault(u => u.username == userName);
        }

        public User createUser(string userName, string pass) 
        {
            User user = new User { username = userName, password = pass };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }
    }

    class MessagesService
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

    class FriendshipService
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
}