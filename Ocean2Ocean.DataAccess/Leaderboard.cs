using System;
using System.Collections.Generic;
using System.Text;

namespace Ocean2Ocean.DataAccess
{
    public class Leaderboard
    {
        public IEnumerable<Step> JourneyRankings { get; set; }
        public IEnumerable<Step> DailyRankings { get; set; }
    }
}
