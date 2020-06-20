using Dapper;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class JourneysSearchResult
    {
        public string Query { get; set; }
        public IEnumerable<Journey> Journeys { get; set; }
    }
}
