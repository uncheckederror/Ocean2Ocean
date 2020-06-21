using System.Collections.Generic;

namespace Ocean2Ocean.DataAccess
{
    public class NicknameSearchResult
    {
        public string Query { get; set; }
        public IEnumerable<Nickname> Nicknames { get; set; }
    }
}
