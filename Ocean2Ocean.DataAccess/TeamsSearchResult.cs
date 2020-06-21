using System.Collections.Generic;

namespace Ocean2Ocean.DataAccess
{
    public class TeamsSearchResult
    {
        public string Query { get; set; }
        public IEnumerable<Team> Teams { get; set; }
    }
}
