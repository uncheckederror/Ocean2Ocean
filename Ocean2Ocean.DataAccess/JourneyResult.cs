using System.Collections.Generic;

namespace Ocean2Ocean.DataAccess
{
    public class JourneyResult
    {
        public Journey Journey { get; set; }
        public IEnumerable<Step> Steps { get; set; }
        public IEnumerable<Team> Teams { get; set; }
        public IEnumerable<Nickname> Nicknames { get; set; }
    }
}
