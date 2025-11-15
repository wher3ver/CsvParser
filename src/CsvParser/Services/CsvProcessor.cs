using CsvParser.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;

namespace CsvParser.Services;

public class CsvProcessor
{
    private readonly HashSet<string> _seenRecords = [];
    private readonly List<CabData> _duplicates = [];
    private static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
    private int _skippedRecords = 0;

    public async Task<(DataTable dataTable, List<CabData> duplicates)> ProcessCsvAsync(string filePath)
    {
        var dataTable = CreateDataTable();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null,
            ReadingExceptionOccurred = args =>
            {
                return false;
            }
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        
        csv.Context.RegisterClassMap<CabDataCsvMap>();

        var records = csv.GetRecordsAsync<CabData>();
        
        await foreach (var record in records)
        {
            try
            {
                if (!HasRequiredFields(record))
                {
                    _skippedRecords++;
                    continue;
                }

                if (!ValidateRecord(record))
                {
                    _skippedRecords++;
                    continue;
                }

                var key = CreateDuplicateKey(record);
                if (_seenRecords.Contains(key))
                {
                    _duplicates.Add(record);
                    continue;
                }

                _seenRecords.Add(key);

                TransformRecord(record);

                AddToDataTable(dataTable, record);
            }
            catch (Exception)
            {
                _skippedRecords++;
            }
        }

        if (_skippedRecords > 0)
            Console.WriteLine($"Skipped {_skippedRecords} records due to missing or invalid data");

        return (dataTable, _duplicates);
    }

    private DataTable CreateDataTable()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add(nameof(CabData.TpepPickupDatetime), typeof(DateTime));
        dataTable.Columns.Add(nameof(CabData.TpepDropoffDatetime), typeof(DateTime));
        dataTable.Columns.Add(nameof(CabData.PassengerCount), typeof(byte));
        dataTable.Columns.Add(nameof(CabData.TripDistance), typeof(decimal));
        dataTable.Columns.Add(nameof(CabData.StoreAndFwdFlag), typeof(string));
        dataTable.Columns.Add(nameof(CabData.PULocationID), typeof(short));
        dataTable.Columns.Add(nameof(CabData.DOLocationID), typeof(short));
        dataTable.Columns.Add(nameof(CabData.FareAmount), typeof(decimal));
        dataTable.Columns.Add(nameof(CabData.TipAmount), typeof(decimal));
        return dataTable;
    }

    private bool HasRequiredFields(CabData record)
    {
        if (record.TpepPickupDatetime == default || record.TpepDropoffDatetime == default)
            return false;

        if (string.IsNullOrWhiteSpace(record.StoreAndFwdFlag))
            return false;

        if (!record.PassengerCount.HasValue)
            return false;

        if (!record.TripDistance.HasValue)
            return false;

        if (!record.PULocationID.HasValue || !record.DOLocationID.HasValue)
            return false;

        if (!record.FareAmount.HasValue || !record.TipAmount.HasValue)
            return false;

        return true;
    }

    private bool ValidateRecord(CabData record)
    {
        if (record.PassengerCount < 0 || record.PassengerCount > 255)
            return false;

        if (record.TripDistance < 0)
            return false;

        if (record.PULocationID < 0 || record.DOLocationID < 0)
            return false;

        if (record.FareAmount < 0 || record.TipAmount < 0)
            return false;

        if (record.TpepDropoffDatetime < record.TpepPickupDatetime)
            return false;

        return true;
    }

    private void TransformRecord(CabData record)
    {
        if (record.StoreAndFwdFlag!.Equals("Y", StringComparison.OrdinalIgnoreCase))
            record.StoreAndFwdFlag = "Yes";
        else if (record.StoreAndFwdFlag.Equals("N", StringComparison.OrdinalIgnoreCase))
            record.StoreAndFwdFlag = "No";

        record.TpepPickupDatetime = ConvertEstToUtc((DateTime)record.TpepPickupDatetime!);
        record.TpepDropoffDatetime = ConvertEstToUtc((DateTime)record.TpepDropoffDatetime!);
    }

    private DateTime ConvertEstToUtc(DateTime estDateTime)
    {
        try
        {
            var estTime = DateTime.SpecifyKind(estDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(estTime, EstTimeZone);
        }
        catch
        {
            return DateTime.SpecifyKind(estDateTime, DateTimeKind.Utc);
        }
    }

    private string CreateDuplicateKey(CabData record)
    {
        return $"{record.TpepPickupDatetime:O}|{record.TpepDropoffDatetime:O}|{record.PassengerCount}";
    }

    private void AddToDataTable(DataTable dataTable, CabData record)
    {
        var row = dataTable.NewRow();
        row[nameof(CabData.TpepPickupDatetime)] = record.TpepPickupDatetime;
        row[nameof(CabData.TpepDropoffDatetime)] = record.TpepDropoffDatetime;
        row[nameof(CabData.PassengerCount)] = (byte)record.PassengerCount!.Value;
        row[nameof(CabData.TripDistance)] = record.TripDistance!.Value;
        row[nameof(CabData.StoreAndFwdFlag)] = record.StoreAndFwdFlag ?? "No";
        row[nameof(CabData.PULocationID)] = (short)record.PULocationID!.Value;
        row[nameof(CabData.DOLocationID)] = (short)record.DOLocationID!.Value;
        row[nameof(CabData.FareAmount)] = record.FareAmount!.Value;
        row[nameof(CabData.TipAmount)] = record.TipAmount!.Value;
        dataTable.Rows.Add(row);
    }

    public async Task WriteDuplicatesToCsvAsync(List<CabData> duplicates, string outputPath)
    {
        var fullPath = Path.GetFullPath(outputPath);
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        await using var writer = new StreamWriter(fullPath);
        await using var csv = new CsvWriter(writer, config);
        
        csv.Context.RegisterClassMap<CabDataCsvMap>();
        await csv.WriteRecordsAsync(duplicates);
        
        Console.WriteLine($"Written {duplicates.Count} duplicate records to {fullPath}");
    }
}
