using Dapper;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class Journey
    {
        public string JourneyName { get; set; }
        public int StepsTaken { get; set; }
        public int Participants { get; set; }
        public DateTime Date { get; set; }
        public Step Step { get; set; }
        public string MapboxAccessToken { get; set; }

        public static async Task<IEnumerable<Journey>> GetAllAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Journey>("SELECT [JourneyName], SUM(Steps) As StepsTaken FROM [dbo].[Entries] GROUP BY [JourneyName]")
                .ConfigureAwait(false);
        }
    }
}
