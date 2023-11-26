using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace DotnetProject.DAL
{
    public class DotnetProjectDbContext : DbContext
    {
        static readonly string connectionString = "Server=localhost; User ID=root; Password=rootroot; Database=dotnet";

        public DbSet<User> Users { get; set; }
        public DbSet<FriendshipRequest> FriendshipRequests { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FriendshipRequest>()
                .HasOne(fr => fr.FromUser)
                .WithMany(u => u.SentFriendshipRequests)
                .HasForeignKey(fr => fr.fromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendshipRequest>()
                .HasOne(fr => fr.ToUser)
                .WithMany(u => u.ReceivedFriendshipRequests)
                .HasForeignKey(fr => fr.toUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.FromUser)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.fromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.ToUser)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.toUserId)
                .OnDelete(DeleteBehavior.Restrict);
            base.OnModelCreating(modelBuilder);
        }
    }

    public class User
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public List<FriendshipRequest> SentFriendshipRequests { get; set; }

        public List<FriendshipRequest> ReceivedFriendshipRequests { get; set; }

        public List<Message> SentMessages { get; set; }

        public List<Message> ReceivedMessages { get; set; }
    }

    public class FriendshipRequest
    {
        public int friendshipRequestId { get; set; }
        public int fromUserId { get; set; }
        public int toUserId { get; set; }
        public bool isAccepted { get; set; }

        public User FromUser { get; set; }

        public User ToUser { get; set; }
    }

    public class Message
    {
        public int messageId { get; set; }
        public int fromUserId { get; set; }
        public int toUserId { get; set; }
        public string message { get; set; }

        public User FromUser { get; set; }

        public User ToUser { get; set; }
    }
}