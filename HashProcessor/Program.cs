using HashFlow.Application.Common.Interfaces;
using HashFlow.Infrastructure;
using HashFlow.Infrastructure.Persistance.Repositories;
using HashProcessor;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<HashQueueProcessor>();
        services.AddDbContext<HashesDbContext>(
            options => options.UseSqlServer(
                hostContext.Configuration.GetConnectionString("Default")));
        services.AddScoped<IHashesRepository, HashesRepository>();
    })
    .Build();

await host.RunAsync();