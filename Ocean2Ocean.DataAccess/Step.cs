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
        public string Email { get; set; }
        public int Steps { get; set; }
        public int StepsTaken { get; set; }
        public int StepsInRoute { get; set; }
        public DateTime Created { get; set; }

        public static async Task<IEnumerable<Step>> GetAllAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var result = await connection
                .QueryAsync<Step>("SELECT [Email], [Steps] ,[Created], [EntryId] FROM [dbo].[Entries]")
                .ConfigureAwait(false);

            return result;
        }

        public static async Task<IEnumerable<Step>> GetByEmail(string email, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            // Make the query more general.
            email = $"%{email}%";

            var result = await connection
                .QueryAsync<Step>("SELECT [Email], [Steps] ,[Created], [EntryId] FROM [dbo].[Entries] WHERE [Email] LIKE @email ORDER BY [Created] DESC",
                new { email })
                .ConfigureAwait(false);

            return result;
        }

        public async Task<bool> PostAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var result = await connection
                .ExecuteAsync("INSERT INTO [dbo].[Entries] ([Email], [Steps], [Created]) VALUES (@Email, @Steps, @Created)",
                new { Email, Steps, Created = DateTime.Now })
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
