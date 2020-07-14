using Dapper;

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ocean2Ocean.DataAccess
{
    public class Map
    {
        public Journey Journey { get; set; }
        public Team Team { get; set; }
        public IEnumerable<Team> Teams { get; set; }
        public Nickname Nickname { get; set; }
        public IEnumerable<Nickname> Nicknames { get; set; }
        public int StepsTaken { get; set; }
        public string ErrorMessage { get; set; }
        public GeoJson Route { get; set; }
    }
}
