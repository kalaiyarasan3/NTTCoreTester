using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;


namespace NTTCoreTester.Core.Helper 
{
    public class CsvService
    {
        public async Task<string> WriteCsv<T>(List<T> reportData, string path)
        {
            try
            {
                if (reportData == null || !reportData.Any())
                    return "";

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using var writer = new StreamWriter(path);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ","
                });

                await csv.WriteRecordsAsync(reportData);

                return "";
            }
            catch (Exception ex)
            {
                return ( $"Error writing CSV: {ex.Message}");
            }
        }
    }
}
