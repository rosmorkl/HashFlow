using HashFlow.Application.Common.Interfaces;
using HashFlow.Domain.DTOs;
using HashFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HashFlow.Infrastructure.Persistance.Repositories;

public class HashesRepository : IHashesRepository
{
    private readonly HashesDbContext _context;

    public HashesRepository(HashesDbContext context)
    {
        _context = context;
    }

    public Task<List<HashCountDto>> GetHashCountGroupedByDateAsync()
        => _context.Hashes
            .AsNoTracking()
            .GroupBy(h => h.Date.Date)
            .Select(gr => new HashCountDto
            {
                Date = gr.Key,
                Count = gr.Count()
            })
            .ToListAsync();

    public async Task AddHashRange(IEnumerable<Hash> hash)
    {
        await _context.Hashes.AddRangeAsync(hash);
        await _context.SaveChangesAsync();
    }
}