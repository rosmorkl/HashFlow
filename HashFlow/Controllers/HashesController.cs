using System.Security.Cryptography;
using System.Text;
using HashFlow.Application.Common.Interfaces;
using HashFlow.Domain.Models;
using HashFlow.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HashFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HashesController : ControllerBase
{
    private readonly IHashesRepository _hashesRepository;
    private readonly IMessageProducer _messageProducer;

    public HashesController(IMessageProducer messageProducer, IHashesRepository hashesRepository)
    {
        _messageProducer = messageProducer;
        _hashesRepository = hashesRepository;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateHashes()
    {
        int totalHashes = 40000;
        int batchSize = 1000;
        int totalBatches = totalHashes / batchSize;

        var tasks = new List<Task>();

        for (int i = 0; i < totalBatches; i++)
        {
            tasks.Add(ProcessBatchAsync(batchSize));
        }

        await Task.WhenAll(tasks);

        return Ok("Hashes generated and sent to queue in batches.");
    }

    private async Task ProcessBatchAsync(int batchSize)
    {
        var batch = await GenerateHashBatch(batchSize);
        _messageProducer.SendBatchToQueueAsync(batch);
    }

    private Task<string[]> GenerateHashBatch(int batchSize)
    {
        var tasks = Enumerable
            .Range(0, batchSize).Select(_ => Task.Run(GenerateRandomSHA1Hash));
        return Task.WhenAll(tasks);
    }

    private string GenerateRandomSHA1Hash()
    {
        using var sha1 = SHA1.Create();
        var randomData = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
        var hash = sha1.ComputeHash(randomData);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
    }

    [HttpGet]
    public async Task<IActionResult> GetHashes()
    {
        var a = await _hashesRepository.GetHashCountGroupedByDateAsync();
        return Ok(a);
    }
}