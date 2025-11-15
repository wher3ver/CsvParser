using CsvParser.Models;

using Microsoft.Data.SqlClient;

using System.Data;

namespace CsvParser.Database;

public class DatabaseManager(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public async Task<int> BulkInsertAsync(DataTable dataTable)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = nameof(CabData),
            BatchSize = 10000,
        };

        bulkCopy.ColumnMappings.Add(nameof(CabData.TpepPickupDatetime), nameof(CabData.TpepPickupDatetime));
        bulkCopy.ColumnMappings.Add(nameof(CabData.TpepDropoffDatetime), nameof(CabData.TpepDropoffDatetime));
        bulkCopy.ColumnMappings.Add(nameof(CabData.PassengerCount), nameof(CabData.PassengerCount));
        bulkCopy.ColumnMappings.Add(nameof(CabData.TripDistance), nameof(CabData.TripDistance));
        bulkCopy.ColumnMappings.Add(nameof(CabData.StoreAndFwdFlag), nameof(CabData.StoreAndFwdFlag));
        bulkCopy.ColumnMappings.Add(nameof(CabData.PULocationID), nameof(CabData.PULocationID));
        bulkCopy.ColumnMappings.Add(nameof(CabData.DOLocationID), nameof(CabData.DOLocationID));
        bulkCopy.ColumnMappings.Add(nameof(CabData.FareAmount), nameof(CabData.FareAmount));
        bulkCopy.ColumnMappings.Add(nameof(CabData.TipAmount), nameof(CabData.TipAmount));

        await bulkCopy.WriteToServerAsync(dataTable);
        return dataTable.Rows.Count;
    }
}
