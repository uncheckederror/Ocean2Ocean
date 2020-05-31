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

        // These properties are for convenience and not present in the Db.
        public int StepsTaken { get; set; }
        public int StepsInRoute { get; set; }

        public static async Task<IEnumerable<Step>> GetAllAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var result = await connection
                .QueryAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries]")
                .ConfigureAwait(false);

            return result;
        }

        public static async Task<IEnumerable<Step>> GetByEmailAsync(string email, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            // Make the query more general.
            email = $"%{email}%";

            var result = await connection
                .QueryAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries] WHERE [Email] LIKE @email ORDER BY [Created] DESC",
                new { email })
                .ConfigureAwait(false);

            return result;
        }

        public static async Task<IEnumerable<Step>> GetByJourneyAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            // Make the query more general.
            journeyName = $"%{journeyName}%";

            var result = await connection
                .QueryAsync<Step>("SELECT [EntryId], [Email], [JourneyName], [Steps], [Created] FROM [dbo].[Entries] WHERE [JourneyName] LIKE @journeyName ORDER BY [Created] DESC",
                new { journeyName })
                .ConfigureAwait(false);

            return result;
        }

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
