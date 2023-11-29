using System;
using DotnetProject.BLL;
using DotnetProject.DAL;
using CustomDictionaryLibrary;

namespace DotnetProject.PL {
    class ConsoleInteraction
    {
        private readonly UsersService usersService;
        private readonly MessagesService messagesService;
        private readonly FriendshipService friendshipService;

        public ConsoleInteraction(DotnetProjectDbContext context)
        {
            usersService = new UsersService(context);
            messagesService = new MessagesService(context);
            friendshipService = new FriendshipService(context);
        }

        public void RunCustomDictionaryOperations()
        {
            CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();

            dictionary.ItemAdded += (sender, args) =>
            {
                Console.WriteLine($"Item added: Key = {args.Key}, Value = {args.Value}");
            };

            dictionary.Add(1, "One");
            dictionary.Add(2, "Two");

            try
            {
                string value = dictionary[3];
                Console.WriteLine($"Item found: {value}");
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine(dictionary.ContainsKey(1));
            dictionary.Remove(1);
            Console.WriteLine(dictionary.ContainsKey(1));
        }

        public void ImitateSocialMediaActivity() {
            Dictionary<string, User> usersDictionary = new Dictionary<string, User>();
            List<string> names = new List<string> { "David", "Eve", "Frank", "Bob", "Marry", "Hector" };

            foreach (string name in names)
            {
                User user = this.usersService.getUserByUsername(name);
                if (user == null)
                {
                    user = this.usersService.createUser(name, "123456");
                }

                usersDictionary.Add(name, user);
            }

            if (usersDictionary.TryGetValue("David", out User david))
            {
                if (usersDictionary.TryGetValue("Bob", out User bob))
                {
                    int davidId = david.userId;
                    int bobId = bob.userId;
                    this.messagesService.sendMessage(davidId, bobId, "Hello, how is it going?");
                    this.messagesService.sendMessage(bobId, davidId, "Hey Bob, fine! Let's be friends!");

                    this.friendshipService.sendFriendshipRequest(bobId, davidId);
                    this.friendshipService.acceptFriendshipRequest(bobId, davidId);

                    Dictionary<User, List<Message>> bobsMessages = this.messagesService.getMessages(bobId);
                    
                    foreach (var kvp in bobsMessages)
                    {
                        User user = kvp.Key;
                        List<Message> messages = kvp.Value;

                        Console.WriteLine($"Messages for user '{user.username}':");

                        foreach (var message in messages)
                        {
                            Console.WriteLine($"  Message {message.messageId}: {message.message}");
                        }
                    }

                    List<User> friends = this.friendshipService.getFriends(bobId);

                    foreach (var friend in friends)
                    {
                        Console.WriteLine($"  User ID: {friend.userId}, Username: {friend.username}");
                    }
                }
            }
        }
    }
}