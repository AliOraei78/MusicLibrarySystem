using Microsoft.EntityFrameworkCore;
using MusicLibrarySystem.Core.Models;

namespace MusicLibrarySystem.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Album> Albums { get; set; }
    public DbSet<Track> Tracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Album>()
            .HasMany(a => a.Tracks)
            .WithOne(t => t.Album)
            .HasForeignKey(t => t.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Album>()
            .Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Track>()
            .Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);
    }
}