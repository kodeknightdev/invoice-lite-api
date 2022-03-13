using InvoiceLite.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Tests;

public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
