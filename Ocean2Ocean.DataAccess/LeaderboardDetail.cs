using Dapper;

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class LeaderboardDetail
    {
        public IEnumerable<Leaderboard> DailyRankings { get; set; }
        public IEnumerable<Leaderboard> JourneyRankings { get; set; }
        public IEnumerable<Leaderboard> SumByDay { get; set; }
    }
}
