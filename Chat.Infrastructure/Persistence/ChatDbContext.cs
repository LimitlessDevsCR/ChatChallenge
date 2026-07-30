using Chat.Domain.Entities;
using Chat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Persistence
{
    public class ChatDbContext : IdentityDbContext<ApplicationUser>
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options): base(options)
        {
        }

        public DbSet<Message> Messages => Set<Message>();
    }
}
