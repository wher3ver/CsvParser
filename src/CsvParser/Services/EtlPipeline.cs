using CsvParser.Database;

namespace CsvParser.Services;

public class EtlPipeline(string connectionString, string duplicatesOutputPath)
{
    private readonly DatabaseManager _databaseManager = new(connectionString);
    private readonly CsvProcessor _csvProcessor = new();
    private readonly string _duplicatesOutputPath = duplicatesOutputPath;

    public async Task RunAsync(string csvFilePath)
    {
        var (dataTable, duplicates) = await _csvProcessor.ProcessCsvAsync(csvFilePath);

        Console.WriteLine($"Processed {dataTable.Rows.Count} valid records");

        if (duplicates.Count > 0)
            await _csvProcessor.WriteDuplicatesToCsvAsync(duplicates, _duplicatesOutputPath);

        var insertedCount = await _databaseManager.BulkInsertAsync(dataTable);
        Console.WriteLine($"Bulk inserted {insertedCount} rows");
    }
}
