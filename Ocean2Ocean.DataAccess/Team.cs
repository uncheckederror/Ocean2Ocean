using Dapper;

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class Team
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamWebsite { get; set; }
        public string Bio { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }


        public static async Task<IEnumerable<Team>> GetAllAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Team>("SELECT [TeamId], [TeamName], [TeamWebsite], [Bio], [Active], [Created] FROM [dbo].[Teams] ORDER BY [Created] DESC")
                .ConfigureAwait(false);
        }

        public static async Task<Team> GetByIdAsync(int teamId, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryFirstOrDefaultAsync<Team>("SELECT [TeamId], [TeamName], [TeamWebsite], [Bio], [Active], [Created] FROM [dbo].[Teams] WHERE [TeamId] = @teamId", new { teamId })
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Team>> GetByTeamNameAsync(string teamName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Team>("SELECT [TeamId], [TeamName], [TeamWebsite], [Bio], [Active], [Created] FROM [dbo].[Teams] WHERE [TeamName] = @teamName", new { teamName })
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Team>> SearchByTeamNameAsync(string teamName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            teamName = $"%{teamName}%";

            return await connection
                .QueryAsync<Team>("SELECT [TeamId], [TeamName], [TeamWebsite], [Bio], [Active], [Created] FROM [dbo].[Teams] WHERE [TeamName] LIKE @teamName", new { teamName })
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
                .ExecuteAsync("INSERT INTO [dbo].[Teams] ([TeamName], [TeamWebsite], [Bio], [Active], [Created]) VALUES (@TeamName, @TeamWebsite, @Bio, @Active, @Created)",
                new { TeamName, TeamWebsite, Bio, Active, Created = DateTime.Now })
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
                .ExecuteAsync("UPDATE [dbo].[Teams] SET [TeamName] = @TeamName, [TeamWebsite] = @TeamWebsite, [Bio] = @Bio, [Active] = @Active, [Created] = @Created WHERE [TeamId] = @TeamId",
                new { TeamName, TeamWebsite, Bio, Active, Created = DateTime.Now })
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
        /// Remove a journey.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var result = await connection
                .ExecuteAsync("DELETE FROM [dbo].[Teams] WHERE [TeamId] = @TeamId ",
                new { TeamId })
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
