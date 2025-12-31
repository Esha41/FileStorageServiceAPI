using FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace FileStorage.Infrastructure.AppDbContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StoredObject>(entity =>
            {
                entity.HasKey(e => e.Id); 
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
        public DbSet<StoredObject> StoredObjects { get; set; } = null!;
    }
}
