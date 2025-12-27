using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PaymentScheduler.Infrastructure.Context;
using System.Reflection;
using System.Text.Json;

namespace PaymentScheduler.Infrastructure.Common;

public static class DbSeeder
{
    public static async Task Initialize(AppDbContext context, IHostEnvironment environment, IConfiguration configuration)
    {
        string databaseDirectory;

        if (environment.IsDevelopment())
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(currentDirectory, @"../../../../");
            databaseDirectory = Path.GetFullPath(Path.Combine(solutionDirectory, "PaymentScheduler.Infrastructure"));
        }
        else
        {
            databaseDirectory = Directory.GetCurrentDirectory();
        }

        string dataDir = Path.Combine(databaseDirectory, "Data");

        if (!Directory.Exists(dataDir))
        {
            Console.WriteLine("Data folder not found. Skipping normal table seeding.");
            return;
        }

        // Define seeding order explicitly
        var seedingOrder = new string[]
        {
            "UserAccount.json",
            "Frequency.json",
            "Status.json",
            "Payment.json"
        };

        // Seed ordered files first
        foreach (var fileName in seedingOrder)
        {
            var filePath = Path.Combine(dataDir, fileName);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"{fileName} not found. Skipping.");
                continue;
            }

            await SeedJsonFile(context, dataDir, fileName);
        }

        // Seed remaining files dynamically
        var allFiles = Directory.GetFiles(dataDir, "*.json").Select(Path.GetFileName);
        var remainingFiles = allFiles.Except(seedingOrder);

        foreach (var fileName in remainingFiles)
        {
            await SeedJsonFile(context, dataDir, fileName);
        }
    }

    private static async Task SeedJsonFile(AppDbContext context, string dataDir, string fileName)
    {
        var entityName = Path.GetFileNameWithoutExtension(fileName);

        var entityAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetTypes().Any(t => t.Namespace?.StartsWith("PaymentScheduler.Domain.Models") == true));

        var entityType = entityAssembly?
            .GetTypes()
            .FirstOrDefault(t => t.Namespace == "PaymentScheduler.Domain.Models"
                                 && t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase)
                                 && !t.IsEnum);

        if (entityType == null)
        {
            Console.WriteLine($"Entity class '{entityName}' not found. Skipping {fileName}.");
            return;
        }

        // Skip enums
        if (entityType.IsEnum)
        {
            Console.WriteLine($"{entityType.Name} is an enum. Skipping seeding.");
            return;
        }

        // Call SeedTable<T> dynamically
        var method = typeof(DbSeeder).GetMethod(nameof(SeedTable), BindingFlags.NonPublic | BindingFlags.Static);
        var generic = method.MakeGenericMethod(entityType);
        await (Task)generic.Invoke(null, new object[] { context, dataDir, fileName });
    }

    private static async Task SeedTable<T>(AppDbContext context, string databaseDirectory, string fileName) where T : class
    {
        if (context.Set<T>().Any())
        {
            Console.WriteLine($"{typeof(T).Name} table already has data. Skipping.");
            return;
        }

        var jsonFilePath = Path.Combine(databaseDirectory, fileName);

        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"{fileName} not found. Skipping.");
            return;
        }

        var jsonData = await File.ReadAllTextAsync(jsonFilePath);
        var data = JsonSerializer.Deserialize<List<T>>(jsonData);

        if (data == null || !data.Any()) return;

        foreach (var item in data)
        {
            var createdOnProp = item.GetType().GetProperty("CreatedOn");
            if (createdOnProp != null && createdOnProp.CanWrite)
                createdOnProp.SetValue(item, DateTime.UtcNow);

            var isDeletedProp = item.GetType().GetProperty("IsDeleted");
            if (isDeletedProp != null && isDeletedProp.CanWrite)
                isDeletedProp.SetValue(item, false);
        }

        await context.Set<T>().AddRangeAsync(data);
        await context.SaveChangesAsync();

        Console.WriteLine($"{data.Count} records inserted into {typeof(T).Name} table.");
    }
}