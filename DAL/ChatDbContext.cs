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

	}
}