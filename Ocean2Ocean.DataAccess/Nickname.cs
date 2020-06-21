using Dapper;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class Nickname
    {
        public int NicknameId { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }


        public static async Task<IEnumerable<Nickname>> GetAllAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Nickname>("SELECT [NicknameId], [Name], [Bio], [Active], [Created] FROM [dbo].[Nicknames] ORDER BY [Created] DESC")
                .ConfigureAwait(false);
        }

        public static async Task<Nickname> GetByIdAsync(int nicknameId, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryFirstOrDefaultAsync<Nickname>("SELECT [NicknameId], [Name], [Bio], [Active], [Created] FROM [dbo].[Nicknames] WHERE [NicknameId] = @nicknameId", new { nicknameId })
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Nickname>> GetByNicknameAsync(string name, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Nickname>("SELECT [NicknameId], [Name], [Bio], [Active], [Created] FROM [dbo].[Nicknames] WHERE [Name] = @name", new { name })
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Nickname>> SearchByNicknameAsync(string name, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            name = $"%{name}%";

            return await connection
                .QueryAsync<Nickname>("SELECT [NicknameId], [Name], [Bio], [Active], [Created] FROM [dbo].[Nicknames] WHERE [Name] LIKE @name", new { name })
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
                .ExecuteAsync("INSERT INTO [dbo].[Nicknames] ([Name], [Bio], [Active], [Created]) VALUES (@Name, @Bio, @Active, @Created)",
                new { Name, Bio, Active, Created = DateTime.Now })
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
                .ExecuteAsync("UPDATE [dbo].[Nicknames] SET [Name] = @Name, [Bio] = @Bio, [Active] = @Active, [Created] = @Created WHERE [NicknameId] = @NicknameId",
                new { NicknameId, Name, Bio, Active, Created = DateTime.Now })
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
                .ExecuteAsync("DELETE FROM [dbo].[Nicknames] WHERE [NicknameId] = @NicknameId ",
                new { NicknameId })
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
