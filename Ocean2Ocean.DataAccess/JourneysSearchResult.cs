﻿using System.Collections.Generic;

namespace Ocean2Ocean.DataAccess
{
    public class JourneysSearchResult
    {
        public string Query { get; set; }
        public IEnumerable<Journey> Journeys { get; set; }
    }
}
