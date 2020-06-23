using System.Collections.Generic;

namespace Ocean2Ocean.DataAccess
{
    public class NicknameResult
    {
        public Nickname Nickname { get; set; }
        public IEnumerable<Team> Teams { get; set; }
        public IEnumerable<Journey> Journeys { get; set; }
        public IEnumerable<Step> Steps { get; set; }

    }
}
