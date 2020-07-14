using System;
using System.Collections.Generic;
using System.Text;

namespace Ocean2Ocean.DataAccess
{

    public class GeoJson
    {
        public string type { get; set; }
        public Properties properties { get; set; }
        public Geometry geometry { get; set; }

        public class Properties
        {
        }

        public class Geometry
        {
            public string type { get; set; }
            public float[][] coordinates { get; set; }
        }
    }
}
