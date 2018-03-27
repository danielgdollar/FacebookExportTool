using Microsoft.EntityFrameworkCore;
using FacebookExportEntities.Entities;
namespace FacebookExportEntities
{
    public class FacebookExportDbContext : DbContext
    {
        public FacebookExportDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Friend> Friend { get; set; }
        public DbSet<TimelinePost> TimelinePost { get; set; }

        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimelinePost>()
                .HasOne(e => e.PostFriend)
                .WithMany()
                .HasForeignKey(e => e.PostFriendId);
        }
    }
}