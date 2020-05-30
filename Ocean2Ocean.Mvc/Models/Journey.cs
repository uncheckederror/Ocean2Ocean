using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ocean2Ocean.Models
{
    public class Journey
    {
        public int StepsTaken { get; set; }
        public int Participants { get; set; }
        public DateTime Date { get; set; }
        public Step Step { get; set; }
        public string MapboxAccessToken { get; set; }
    }
}
