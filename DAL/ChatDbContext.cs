using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
	public class ChatDbContext: DbContext
	{
		public DbSet<ChatRoom> ChatRooms { get; set; }
		public DbSet<ChatMessage> ChatMessages { get; set; }
		public DbSet<User> Users { get; set; }


		public ChatDbContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ChatRoom>()
				.HasMany(e => e.UserList)
				.WithOne(e => e.CurrentChatRoom);
			modelBuilder.Entity<ChatRoom>()
				.HasMany(e => e.ChatMessages)
				.WithOne(e => e.ChatRoom);
			modelBuilder.Entity<ChatMessage>()
				.HasOne(e => e.User)
				.WithMany(e => e.Messages);

		}
	}
}