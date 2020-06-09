using Dapper;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class Leaderboard
    {
        public string Email { get; set; }
        public string JourneyName { get; set; }
        public int TotalSteps { get; set; }

        public static async Task<IEnumerable<Leaderboard>> GetRankingsAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Leaderboard>("SELECT [Email], [JourneyName], SUM(Steps) As TotalSteps FROM [dbo].[Entries] WHERE [JourneyName] = @journeyName GROUP BY [Email], [JourneyName] ORDER BY TotalSteps DESC",
                new { journeyName })
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Leaderboard>> GetDailyRankingsAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var yesterday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var today = yesterday.AddDays(+1);

            return await connection
                .QueryAsync<Leaderboard>("SELECT [Email], [JourneyName], SUM(Steps) As TotalSteps FROM [dbo].[Entries] WHERE [JourneyName] = @journeyName AND [Created] >= @yesterday AND [Created] <= @today GROUP BY [Email], [JourneyName] ORDER BY TotalSteps DESC",
                new { journeyName, yesterday, today })
                .ConfigureAwait(false);
        }
    }
}
