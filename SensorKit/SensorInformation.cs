using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorKit
{
    public class SensorInformation
    {
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Url { get; set; }
        public string ShortDescription { get; set; }
        
        public string Image { get {
                return $"/Assets/sensorkit/products/{Model.ToLower()}.png";
            }
        }

        public SensorCapabilities Capabilities { get; set; }
    }
}
