using HashFlow.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HashFlow.Common.Utils;

public static class MigrateDbContext
{
    public static async Task MigrateAsync(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<HashesDbContext>();
        await context?.Database.MigrateAsync()!;
    }
}