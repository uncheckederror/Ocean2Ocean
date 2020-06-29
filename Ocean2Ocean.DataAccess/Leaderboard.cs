﻿using Dapper;

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class Leaderboard
    {
        public string Nickname { get; set; }
        public string JourneyName { get; set; }
        public string TeamName { get; set; }
        public int TotalSteps { get; set; }
        private int SumYear { get; set; }
        private int SumMonth { get; set; }
        private int SumDay { get; set; }
        public DateTime SummedDate { get; set; }

        /// <summary>
        /// Get a list of objects ordered by the steps they've contributed to the Journey.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Leaderboard>> GetRankingsAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Leaderboard>("SELECT [Nickname], [JourneyName], [TeamName], SUM(Steps) As TotalSteps FROM [dbo].[Steps] WHERE [JourneyName] = @journeyName GROUP BY [Nickname], [JourneyName], [TeamName] ORDER BY TotalSteps DESC",
                new { journeyName })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of teams ordered by the steps they've contributed to the Journey.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Leaderboard>> GetTeamRankingsAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Leaderboard>("SELECT [JourneyName], [TeamName], SUM(Steps) As TotalSteps FROM [dbo].[Steps] WHERE [JourneyName] = @journeyName GROUP BY [JourneyName], [TeamName] ORDER BY TotalSteps DESC",
                new { journeyName })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of objects ordered by the steps they've contributed today.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Leaderboard>> GetDailyRankingsAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection
                .QueryAsync<Leaderboard>("SELECT [Nickname], [JourneyName], [TeamName], SUM(Steps) As TotalSteps FROM [dbo].[Steps] WHERE [JourneyName] = @journeyName AND [DateStepped] = @date GROUP BY [Nickname], [JourneyName], [TeamName] ORDER BY TotalSteps DESC",
                new { journeyName, date = DateTime.Now.ToShortDateString() })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of teams ordered by the steps they've contributed today.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Leaderboard>> GetDailyTeamRankingsAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);


            return await connection
                .QueryAsync<Leaderboard>("SELECT [JourneyName], [TeamName], SUM(Steps) As TotalSteps FROM [dbo].[Steps] WHERE [JourneyName] = @journeyName AND [DateStepped] = @date GROUP BY [JourneyName], [TeamName] ORDER BY TotalSteps DESC",
                new { journeyName, date = DateTime.Now.ToShortDateString() })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of objects where each contains the sum of the steps contributed for a specific day.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Leaderboard>> GetTotalStepsByDayAsync(string journeyName, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var yesterday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var today = yesterday.AddDays(+1);

            var results = await connection
                .QueryAsync<Leaderboard>("SELECT [JourneyName], Year(Created) As SumYear, Month(Created) As SumMonth, Day(Created) As SumDay, SUM(Steps) As TotalSteps FROM[dbo].[Steps] WHERE [JourneyName] = @journeyName GROUP BY [JourneyName], Year(Created), Month(Created), Day(Created) ORDER BY SumDay DESC",
                new { journeyName })
                .ConfigureAwait(false);

            foreach (var result in results)
            {
                result.SummedDate = new DateTime(result.SumYear, result.SumMonth, result.SumDay);
            }

            return results;
        }
    }
}
