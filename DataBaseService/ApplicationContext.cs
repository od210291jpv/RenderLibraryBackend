using DataBaseService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataBaseService
{
    public class ApplicationContext : DbContext
    {
        public DbSet<PublicationModel> Publications { get; set; }

        public DbSet<UserModel> Users { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>()
                .HasMany(user => user.AuthoredPublications)  // A User has many AuthoredPublications
                .WithOne(pub => pub.Author)                  // Each Publication has one Author
                .HasForeignKey(pub => pub.AuthorId)          // The foreign key is AuthorId
                .OnDelete(DeleteBehavior.Restrict);          // Prevents deleting a user if they have publications

            modelBuilder.Entity<UserModel>()
                .HasMany(user => user.FavoritePublications) // A User has many FavoritePublications
                .WithMany(pub => pub.FavoritedByUsers)      // A Publication is favorited by many Users
                .UsingEntity(j => j.ToTable("UserFavoritePublications")); // Names the join table
        }
    }
}
