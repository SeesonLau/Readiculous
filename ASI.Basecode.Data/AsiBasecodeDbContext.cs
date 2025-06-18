using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User Entity Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D5F4A160F")
                    .HasFilter("IsUpdated = CAST(0 AS BIT) AND IsDeleted = CAST(0 AS BIT)")
                    .IsUnique();

                entity.Property(e => e.UserId) // UserId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Username) // Username
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Email) // Email
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Password) // Password
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Role) // Role
                    .IsRequired()
                    .HasColumnType("int"); 
                entity.Property(e => e.ProfilePictureUrl) // ProfilePictureUrl
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedBy) // CreatedBy
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedTime).HasColumnType("datetime"); // CreatedTime
                entity.Property(e => e.IsUpdated) // IsUpdated
                    .IsRequired()
                    .HasDefaultValue(false);
                entity.Property(e => e.UpdatedBy) // UpdatedBy
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedTime).HasColumnType("datetime"); // UpdatedTime
                entity.Property(e => e.IsDeleted) // IsDeleted
                    .IsRequired()
                    .HasDefaultValue(false);
                entity.Property(e => e.IsBlocked) // IsBlocked
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookId, "UQ__Books__1788CC4D5F4A160F")
                    .HasFilter("IsUpdated = CAST(0 AS BIT) AND IsDeleted = CAST(0 AS BIT)")
                    .IsUnique();

                entity.Property(e => e.BookId) // BookId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.BookGenreId) // BookGenreId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.ISBN) // ISBN
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.Title) // Title
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Description) // Description
                    .HasColumnType("text");
                entity.Property(e => e.Author) // Author
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.SeriesNumber) // SeriesNumber
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false); 
                entity.Property(e => e.Publisher) // Publisher
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.PublicationDate)// PublicationDate
                    .HasColumnType("datetime"); 
                entity.Property(e => e.AverageRating) // AverageRating
                    .IsRequired()
                    .HasColumnType("decimal")
                    .HasDefaultValue(0); 
                entity.Property(e => e.CreatedBy) // CreatedBy
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedTime) // CreatedTime
                    .IsRequired()
                    .HasColumnType("datetime"); 
                entity.Property(e => e.IsUpdated) // IsUpdated
                    .IsRequired()
                    .HasDefaultValue(false);
                entity.Property(e => e.UpdatedBy) // UpdatedBy
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedTime) // UpdatedTime
                    .HasColumnType("datetime"); 
                entity.Property(e => e.IsDeleted) // IsDeleted
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<BookGenre>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookGenreId, "UQ__BookGenres__1788CC4D5F4A160F")
                    .HasFilter("IsUpdated = CAST(0 AS BIT) AND IsDeleted = CAST(0 AS BIT)")
                    .IsUnique();

                entity.Property(e => e.BookGenreId) // BookGenreId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Name) // Name
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedBy) // CreatedBy
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedTime) // CreatedTime
                    .HasColumnType("datetime"); 
                entity.Property(e => e.IsUpdated) // IsUpdated
                    .IsRequired()
                    .HasDefaultValue(false);
                entity.Property(e => e.UpdatedBy) // UpdatedBy
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedTime) // UpdatedTime
                    .HasColumnType("datetime"); 
                entity.Property(e => e.IsDeleted) // IsDeleted
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookId, "IX_Reviews_BookId"); // Index for Book Searches
                entity.HasIndex(e => e.UserId, "IX_Reviews_UserId"); // Index for User Searches

                entity.Property(e => e.BookId) // BookId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UserId) // UserId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Comment) // Comment
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false);
                entity.Property(e => e.Rating) // Rating
                    .IsRequired();
                entity.HasCheckConstraint( // Check constraint for Rating if it is within 1 to 5
                    "CK_Reviews_RatingRange",
                    "Rating >= 1 AND Rating <= 5");
                entity.Property(e => e.CreatedTime) // CreatedTime
                    .IsRequired()
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<FavoriteBook>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookId, "IX_FavoriteBooks_BookId"); // Index for Book Searches
                entity.HasIndex(e => e.UserId, "IX_FavoriteBooks_UserId"); // Index for User Searches
                entity.Property(e => e.BookId) // BookId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UserId) // UserId
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedTime) // CreatedTime
                    .IsRequired()
                    .HasColumnType("datetime");
                entity.Property(e => e.IsDeleted) // IsDeleted
                    .IsRequired()
                    .HasDefaultValue(false);
                entity.Property(e => e.DeletedTime) // DeletedTime
                    .HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
