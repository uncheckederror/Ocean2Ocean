using Dapper;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class Step
    {
        public int StepId { get; set; }
        public string Nickname { get; set; }
        public string JourneyName { get; set; }
        public string TeamName { get; set; }
        public int Steps { get; set; }
        public DateTime Created { get; set; }
        public int TotalSteps { get; set; }
        // These properties are for convenience and not present in the Db.
        public int StepsTaken { get; set; }
        public int StepsInRoute { get; set; }

        /// <summary>
        /// Get every entry in the database.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Step>> GetAllAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Step>("SELECT [StepId], [Nickname], [JourneyName], [TeamName], [Steps], [Created] FROM [dbo].[Steps]")
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a specific entry by its Id.
        /// </summary>
        /// <param name="StepId"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<Step> GetByIdAsync(int StepId, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryFirstOrDefaultAsync<Step>("SELECT [StepId], [Nickname], [JourneyName], [TeamName], [Steps], [Created] FROM [dbo].[Steps] WHERE [StepId] = @StepId",
                new { StepId })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of Steps related to a specific Nickname with a LIKE query.
        /// </summary>
        /// <param name="Nickname"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Step>> GetByNicknameAsync(string Nickname, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            // Make the query more general.
            Nickname = $"%{Nickname}%";

            return await connection
                .QueryAsync<Step>("SELECT [StepId], [Nickname], [JourneyName], [TeamName], [Steps], [Created] FROM [dbo].[Steps] WHERE [Nickname] LIKE @Nickname ORDER BY [Created] DESC",
                new { Nickname })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of Steps related to an Nickname.
        /// </summary>
        /// <param name="Nickname"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Step>> GetByExactNicknameAsync(string Nickname, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Step>("SELECT [StepId], [Nickname], [JourneyName], [TeamName], [Steps], [Created] FROM [dbo].[Steps] WHERE [Nickname] = @Nickname ORDER BY [Created] DESC",
                new { Nickname })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of Steps related to a specific Journey.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Step>> GetByJourneyAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            // Make the query more general.
            journeyName = $"%{journeyName}%";

            return await connection
                .QueryAsync<Step>("SELECT [StepId], [Nickname], [JourneyName], [TeamName], [Steps], [Created] FROM [dbo].[Steps] WHERE [JourneyName] LIKE @journeyName ORDER BY [Created] DESC",
                new { journeyName })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Check for duplicate Steps.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> CheckForDuplicateAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var timeRange = DateTime.Now.AddHours(-1);

            var result = await connection
                .QueryFirstOrDefaultAsync<Step>("SELECT [StepId], [Nickname], [JourneyName], [TeamName], [Steps], [Created] FROM [dbo].[Steps] WHERE [Nickname] = @Nickname AND [JourneyName] = @JourneyName AND [Steps] = @Steps AND [Created] > @timeRange",
                new { Nickname, JourneyName, Steps, timeRange })
                .ConfigureAwait(false);

            if (result != null && result.Steps > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
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
                .ExecuteAsync("INSERT INTO [dbo].[Steps] ([Nickname], [JourneyName], [TeamName], [Steps], [Created]) VALUES (@Nickname, @JourneyName, @TeamName, @Steps, @Created)",
                new { Nickname, JourneyName, TeamName, Steps, Created = DateTime.Now })
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
                .ExecuteAsync("UPDATE [dbo].[Steps] SET [JourneyName] = @JourneyName, [TeamName] = @TeamName, [Steps] = @Steps, [Created] = @Created WHERE [StepId] = @StepId",
                new { StepId, JourneyName, TeamName, Steps, Created = DateTime.Now })
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
        /// Delete a specific entry.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var result = await connection
                .ExecuteAsync("DELETE FROM [dbo].[Steps] WHERE [StepId] = @StepId",
                new { StepId })
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
