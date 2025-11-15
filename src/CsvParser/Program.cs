using CsvParser.Services;

using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

if (args.Length == 0)
{
    Console.WriteLine("Error: CSV file path not provided");
    Console.WriteLine("Usage: dotnet run <input_csv> [duplicates_output_csv]");
    Console.WriteLine("Example: dotnet run data.csv duplicates.csv");
    Environment.Exit(1);
}

string csvFilePath = args[0];
string duplicatesOutputPath = args.Length > 1 ? args[1] : "duplicates.csv";

if (!File.Exists(csvFilePath))
{
    Console.WriteLine($"Error: File not found at '{csvFilePath}'");
    Environment.Exit(1);
}

try
{
    var pipeline = new EtlPipeline(configuration.GetConnectionString("DefaultConnection")!, duplicatesOutputPath);
    await pipeline.RunAsync(csvFilePath);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex}");
    Environment.Exit(1);
}
