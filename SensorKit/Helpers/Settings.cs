using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorKit
{
    public class Settings
    {
        public int activityTypeId { get; set; }
        public bool IsExperimentOn { get; set; }
        public int Experiment { get; set; }
        public DateTime LastExperimentDate { get; set; }
        public Dictionary<string,string> Tags { get; set; }

        public bool IsOnline { get; set; }
        public bool IsTrackingOn { get; set; }
    }
}
