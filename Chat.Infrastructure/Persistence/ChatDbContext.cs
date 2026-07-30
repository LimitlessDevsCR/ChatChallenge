using Chat.Domain.Entities;
using Chat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Persistence
{
    public class ChatDbContext : IdentityDbContext<ApplicationUser>
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages => Set<Message>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");

                entity.HasKey(message => message.Id);

                entity.Property(message => message.ChatRoomId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(message => message.UserId)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(message => message.UserName)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(message => message.Content)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(message => message.CreatedAtUtc)
                    .IsRequired();

                entity.HasIndex(message => new { message.ChatRoomId, message.CreatedAtUtc });
            });
        }
    }
}
