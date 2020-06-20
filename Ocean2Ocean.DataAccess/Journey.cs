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
        public int JourneyId { get; set; }
        public string JourneyName { get; set; }
        public int StepsTaken { get; set; }
        public int Participants { get; set; }
        public DateTime Date { get; set; }
        public Step Step { get; set; }
        public string ErrorMessage { get; set; }
        public string GeometryFileName { get; set; }
        public string Bio { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }


        /// <summary>
        /// Get a list of all the Journeys.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Journey>> GetAllAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Journey>("SELECT [JourneyName], SUM(Steps) As StepsTaken FROM [dbo].[Steps] GROUP BY [JourneyName]")
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Journey>> GetAllTempAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Journey>("SELECT [JourneyId], [JourneyName], [GeometryFileName], [Bio], [Active], [Created] FROM [dbo].[Journeys]")
                .ConfigureAwait(false);
        }

        public static async Task<Journey> GetByIdAsync(int journeyId, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryFirstOrDefaultAsync<Journey>("SELECT [JourneyId], [JourneyName], [GeometryFileName], [Bio], [Active], [Created] FROM [dbo].[Journeys] WHERE [JourneyId] = @journeyId", new { journeyId })
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Journey>> GetByJourneyNameAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Journey>("SELECT [JourneyId], [JourneyName], [GeometryFileName], [Bio], [Active], [Created] FROM [dbo].[Journeys] WHERE [JourneyName] = @JourneyName", new { journeyName })
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Journey>> SearchByJourneyNameAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            journeyName = $"%{journeyName}%";

            return await connection
                .QueryAsync<Journey>("SELECT [JourneyId], [JourneyName], [GeometryFileName], [Bio], [Active], [Created] FROM [dbo].[Journeys] WHERE [JourneyName] LIKE @JourneyName", new { journeyName })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Add an entry to the database.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> PostAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var result = await connection
                .ExecuteAsync("INSERT INTO [dbo].[Journeys] ([JourneyName], [GeometryFileName], [Bio], [Active], [Created]) VALUES ( @JourneyName, @GeometryFileName, @Bio, @Active, @Created )",
                new { JourneyName, GeometryFileName, Bio, Active, Created = DateTime.Now })
                .ConfigureAwait(false);

            if (result == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Update a specific entry.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> PutAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var result = await connection
                .ExecuteAsync("UPDATE [dbo].[Journeys] SET [JourneyName] = @JourneyName, [GeometryFileName] = @GeometryFileName, [Bio] = @Bio, [Active] = @Active, [Created] = @Created WHERE [JourneyId] = @JourneyId ",
                new { JourneyId, JourneyName, GeometryFileName, Bio, Active, Created = DateTime.Now })
                .ConfigureAwait(false);

            if (result == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
