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
        public int EntryId { get; set; }
        public string Email { get; set; }
        public int Steps { get; set; }
        public DateTime Created { get; set; }
        public string JourneyName { get; set; }
        // This is a sum field for a specific query.
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
                .QueryAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries]")
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a specific entry by its Id.
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<Step> GetByIdAsync(int entryId, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryFirstOrDefaultAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries] WHERE [EntryId] = @entryId",
                new { entryId })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of entries related to a specific email with a LIKE query.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Step>> GetByEmailAsync(string email, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            // Make the query more general.
            email = $"%{email}%";

            return await connection
                .QueryAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries] WHERE [Email] LIKE @email ORDER BY [Created] DESC",
                new { email })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of entries related to an email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Step>> GetByExactEmailAsync(string email, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries] WHERE [Email] = @email ORDER BY [Created] DESC",
                new { email })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of entries related to a specific Journey.
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
                .QueryAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries] WHERE [JourneyName] LIKE @journeyName ORDER BY [Created] DESC",
                new { journeyName })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Check for duplicate entries.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> CheckForDuplicateAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var timeRange = DateTime.Now.AddHours(-1);

            var result = await connection
                .QueryFirstOrDefaultAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries] WHERE [Email] = @Email AND [JourneyName] = @JourneyName AND [Steps] = @Steps AND [Created] > @timeRange",
                new { Email, JourneyName, Steps, timeRange })
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
                .ExecuteAsync("INSERT INTO [dbo].[Entries] ([Email], [JourneyName], [Steps], [Created]) VALUES (@Email, @JourneyName, @Steps, @Created)",
                new { Email, JourneyName, Steps, Created = DateTime.Now })
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
                .ExecuteAsync("UPDATE [dbo].[Entries] SET [JourneyName] = @JourneyName, [Steps] = @Steps, [Created] = @Created WHERE [EntryId] = @EntryId",
                new { EntryId, JourneyName, Steps, Created = DateTime.Now })
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
                .ExecuteAsync("DELETE FROM [dbo].[Entries] WHERE [EntryId] = @EntryId",
                new { EntryId })
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
