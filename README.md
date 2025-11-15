# Assumptions
Number of rows in the table after running the program - 29840

In case of using the program on much larger files, I would probably implement batched processing where the DataTable is flushed to the database approximately every 100,000 records via SqlBulkCopy and then cleared.
Besides, I would temporarily drop the database indexes before bulk insertion, as maintaining indexes during insertion significantly slows the process, and rebuild them afterward.

# Usage

Command pattern: `dotnet run <input_csv> [duplicates_output_csv]`

Example: `dotnet run C:\data\input-file.csv C:\data\output-file.csv`