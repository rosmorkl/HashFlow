using HashFlow.Application.Common.Interfaces;
using HashFlow.Application.RabbitMQ.Services;
using HashFlow.Common.Utils;
using HashFlow.Domain.DTOs;
using HashFlow.Infrastructure;
using HashFlow.Infrastructure.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMessageProducer, MessageProducer>();
builder.Services.AddScoped<IHashesRepository, HashesRepository>();

builder.Services.AddDbContext<HashesDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.Configure<MessageBrokerSettingsDto>(
    builder.Configuration.GetSection("MessageBroker"));
builder.Services.AddSingleton(sp
    => sp.GetRequiredService<IOptions<MessageBrokerSettingsDto>>().Value);
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
await MigrateDbContext.MigrateAsync(app);

app.Run();