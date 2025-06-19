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

        // DbSets for each entity type
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<Genre> BookGenres { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<FavoriteBook> FavoriteBooks { get; set; }
        public virtual DbSet<BookGenreAssignment> BookGenreAssignments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User Entity Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId); // Primary Key

                // Index Creation
                entity.HasIndex(u => u.Email, "UQ__Email")
                    .HasFilter("[DeletedTime] IS NULL")
                    .IsUnique();
                entity.HasIndex(u => u.Username, "UQ__Username")
                    .HasFilter("[DeletedTime] IS NULL")
                    .IsUnique();
                entity.HasIndex(u => u.CreatedBy, "IX_CreatedBy")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(u => u.UpdatedBy, "IX_UpdatedBy")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(u => u.CreatedTime, "IX_CreatedTime")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(u => u.Role, "IX_Role")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(u => u.AccessStatus, "IX_AccessStatus")
                    .HasFilter("[DeletedTime] IS NULL");

                // Property Configuration
                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(u => u.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                // Enum Conversion
                entity.Property(e => e.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                entity.Property(e => e.AccessStatus)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // Foreign Key Configurations
                entity.HasOne(u => u.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(u => u.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(u => u.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.BookId); // Primary Key

                // Index Creation
                entity.HasIndex(b => b.BookId, "UQ__BookID")
                    .HasFilter("[DeletedTime] IS NULL")
                    .IsUnique();
                entity.HasIndex(b => b.ISBN, "UQ__ISBN")
                    .HasFilter("[DeletedTime] IS NULL")
                    .IsUnique();
                entity.HasIndex(b => b.Title, "IX_Title")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(b => b.Author, "IX_Author")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(b => b.Publisher, "IX_Publisher")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(b => b.CreatedBy, "IX_CreatedBy")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(b => b.UpdatedBy, "IX_UpdatedBy")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(b => b.CreatedTime, "IX_CreatedTime")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(b => b.AverageRating, "IX_AverageRating")
                    .HasFilter("[DeletedTime] IS NULL");

                // Property Configuration
                entity.Property(b => b.ISBN)
                    .IsRequired()
                    .HasMaxLength(17);
                entity.Property(b => b.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(b => b.Author)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(b => b.Publisher)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(b => b.AverageRating)
                    .HasColumnType("decimal(3, 2)");

                // Foreign Key Configurations
                entity.HasOne(b => b.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(b => b.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(b => b.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(b => b.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(b => b.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(b => b.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(g => g.GenreId); // Primary Key

                // Index Creation
                entity.HasIndex(g => g.Name)
                  .IsUnique()
                  .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(g => g.CreatedBy, "IX_CreatedBy")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(g => g.UpdatedBy, "IX_UpdatedBy")
                    .HasFilter("[DeletedTime] IS NULL");
                entity.HasIndex(g => g.CreatedTime, "IX_CreatedTime")
                    .HasFilter("[DeletedTime] IS NULL");

                // Properties Configuration
                entity.Property(g => g.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(g => g.Description)
                    .HasMaxLength(500);

                // Foreign Key Configuration
                entity.HasOne(g => g.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(g => g.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(g => g.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(g => g.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => new { r.BookId, r.UserId }); // Primary Key

                // Foreign Key Configuration
                entity.HasOne(r => r.Book)
                  .WithMany(b => b.BookReviews)
                  .HasForeignKey(r => r.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(r => r.User)
                      .WithMany(u => u.UserReviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            modelBuilder.Entity<FavoriteBook>(entity =>
            {
                entity.HasKey(fb => new { fb.UserId, fb.BookId }); // Primary Key

                // Foreign Key Configuration
                entity.HasOne(f => f.Book)
                      .WithMany(b => b.FavoritedbyUsers)
                      .HasForeignKey(f => f.BookId)
                      .OnDelete(DeleteBehavior.Cascade); 
                entity.HasOne(f => f.User)
                      .WithMany(u => u.UserFavoriteBooks)
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });


             modelBuilder.Entity<BookGenreAssignment>(entity =>
             {
                 entity.HasKey(bga => new { bga.BookId, bga.GenreId }); // Primary Key

                 // Foreign Key Configuration
                 entity.HasOne(bga => bga.Book)
                       .WithMany(b => b.GenreAssociations)
                       .HasForeignKey(bga => bga.BookId)
                       .OnDelete(DeleteBehavior.Cascade); 
                 entity.HasOne(bga => bga.Genre)
                       .WithMany(bg => bg.Books) 
                       .HasForeignKey(bga => bga.GenreId)
                       .OnDelete(DeleteBehavior.Cascade); 
             });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
