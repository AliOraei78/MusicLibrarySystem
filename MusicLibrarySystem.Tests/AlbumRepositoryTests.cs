using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using MusicLibrarySystem.Core.Models;
using MusicLibrarySystem.Data.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MusicLibrarySystem.Tests.Repositories
{
    public class AlbumRepositoryTests : IAsyncLifetime
    {
        // ────────────────────────────────────────────────
        // CHANGE THESE VALUES to match your local PostgreSQL
        // ────────────────────────────────────────────────
        private const string ConnectionString =
            "Host=localhost;" +
            "Port=5432;" +
            "Database=MusicLibraryDb;" +           // ← create this database first!
            "Username=postgres;" +             // ← your username
            "Password=password123";     // ← your password

        private readonly Mock<IConfiguration> _configMock = new();
        private readonly Mock<IMemoryCache> _cacheMock = new();

        public AlbumRepositoryTests()
        {
            // Mock configuration so repository reads the real local connection string
            _configMock
                .Setup(c => c["ConnectionStrings:DefaultConnection"])
                .Returns(ConnectionString);

            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s["DefaultConnection"]).Returns(ConnectionString);

            _configMock
                .Setup(c => c.GetSection("ConnectionStrings"))
                .Returns(mockSection.Object);
        }

        public async Task InitializeAsync()
        {
            // Optional: Ensure clean state before tests
            // You can drop/create tables here if you want full isolation
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            // Drop & recreate table (careful: deletes all data!)
            await conn.ExecuteAsync(@"
                DROP TABLE IF EXISTS ""Albums"" CASCADE;

                CREATE TABLE ""Albums"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Title"" TEXT NOT NULL,
                    ""Artist"" TEXT NOT NULL,
                    ""Year"" INTEGER NOT NULL,
                    ""Rating"" NUMERIC NOT NULL
                );
            ");
        }

        public Task DisposeAsync()
        {
            // Optional: cleanup after all tests (usually not needed for local dev)
            return Task.CompletedTask;
        }

        private AlbumRepository CreateSut()
        {
            return new AlbumRepository(
                _configMock.Object,
                _cacheMock.Object,
                ambientContext: null   // assuming ambient is optional/skipped
            );
        }

        private async Task SeedAlbumsAsync(params object[] seedData)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            foreach (var data in seedData)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO ""Albums"" (""Title"", ""Artist"", ""Year"", ""Rating"")
                    VALUES (@Title, @Artist, @Year, @Rating);
                ", data);
            }
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllAlbums()
        {
            await SeedAlbumsAsync(
                new { Title = "Thriller", Artist = "Michael Jackson", Year = 1982, Rating = 4.8m },
                new { Title = "Back in Black", Artist = "AC/DC", Year = 1980, Rating = 4.6m },
                new { Title = "The Dark Side of the Moon", Artist = "Pink Floyd", Year = 1973, Rating = 4.9m }
            );

            var sut = CreateSut();

            var result = await sut.GetAllAsync();

            var albums = result.ToList();
            Assert.Equal(3, albums.Count);
            Assert.Contains(albums, a => a.Title == "Thriller");
            Assert.Contains(albums, a => a.Title == "Back in Black");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectAlbum_WhenExists()
        {
            await SeedAlbumsAsync(
                new { Title = "Abbey Road", Artist = "The Beatles", Year = 1969, Rating = 4.7m }
            );

            var sut = CreateSut();

            var result = await sut.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Abbey Road", result.Title);
            Assert.Equal("The Beatles", result.Artist);
            Assert.Equal(1969, result.Year);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            await SeedAlbumsAsync(
                new { Title = "Rumours", Artist = "Fleetwood Mac", Year = 1977, Rating = 4.5m }
            );

            var sut = CreateSut();

            var result = await sut.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByArtistAsync_ReturnsMatchingAlbums_IgnoreCase()
        {
            await SeedAlbumsAsync(
                new { Title = "Nevermind", Artist = "Nirvana", Year = 1991, Rating = 4.8m },
                new { Title = "In Utero", Artist = "Nirvana", Year = 1993, Rating = 4.6m },
                new { Title = "OK Computer", Artist = "Radiohead", Year = 1997, Rating = 4.9m }
            );

            var sut = CreateSut();

            var result = await sut.GetByArtistAsync("nirvana");

            var albums = result.ToList();
            Assert.Equal(2, albums.Count);

            Assert.All(albums, a =>
                Assert.Equal("Nirvana", a.Artist, StringComparer.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoAlbums()
        {
            var sut = CreateSut();

            var result = await sut.GetAllAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}