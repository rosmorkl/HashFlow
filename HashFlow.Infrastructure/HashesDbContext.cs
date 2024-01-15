
using HashFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HashFlow.Infrastructure;

public class HashesDbContext : DbContext
{
    public HashesDbContext(DbContextOptions<HashesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hash> Hashes { get; set; } = null!;
}