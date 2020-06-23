using System.Collections.Generic;

namespace Ocean2Ocean.DataAccess
{
    public class TeamResult
    {
        public Team Team { get; set; }
        public IEnumerable<Nickname> Nicknames { get; set; }
        public IEnumerable<Step> Steps { get; set; }
    }
}
