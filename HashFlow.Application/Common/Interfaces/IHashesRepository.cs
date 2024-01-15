using HashFlow.Domain.DTOs;
using HashFlow.Domain.Models;

namespace HashFlow.Application.Common.Interfaces;

public interface IHashesRepository
{
    Task<List<HashCountDto>> GetHashCountGroupedByDateAsync();
    Task AddHashRange(IEnumerable<Hash> hash);
}