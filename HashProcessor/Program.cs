using HashFlow.Application.Common.Interfaces;
using HashFlow.Domain.DTOs;
using HashFlow.Infrastructure;
using HashFlow.Infrastructure.Persistance.Repositories;
using HashProcessor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<HashQueueProcessor>();
        services.AddDbContext<HashesDbContext>(
            options => options.UseSqlServer(
                hostContext.Configuration.GetConnectionString("Default")));
        services.AddScoped<IHashesRepository, HashesRepository>();
        services.Configure<MessageBrokerSettingsDto>(
            hostContext.Configuration.GetSection("MessageBroker"));
        services.AddSingleton(sp
            => sp.GetRequiredService<IOptions<MessageBrokerSettingsDto>>().Value);

    })
    .Build();

await host.RunAsync();