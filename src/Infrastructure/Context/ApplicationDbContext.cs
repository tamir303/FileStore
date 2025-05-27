using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using FileStoreService.Infrastructure.Entities;
using FileMetadata = FileStoreService.Features.Storage.DTOs.FileMetadata;

namespace FileStoreService.Infrastructure.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<FileMetadata> Files { get; set; }
    public DbSet<FileVersion> FileVersions { get; set; }
    public DbSet<FilePermission> FilePermissions { get; set; }
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // FileMetadata configuration
        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.UploadedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.UploadedBy);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.UploadedAt);
            entity
                .Property(e => e.CustomMetadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string,string>>(v, (JsonSerializerOptions?)null)
                )
                .HasColumnType("nvarchar(max)");
        });

        // FileVersion configuration
        modelBuilder.Entity<FileVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).IsRequired();
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.UploadedBy).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.FileMetadata)
                .WithMany(f => f.Versions)
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FilePermission configuration
        modelBuilder.Entity<FilePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Permission).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.FileMetadata)
                .WithMany(f => f.Permissions)
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}