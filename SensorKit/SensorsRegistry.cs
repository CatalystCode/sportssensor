using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorKit
{
    public class SensorsRegistry
    {
        public List<SensorInformation> _registry = new List<SensorInformation>()
        {
            new SensorInformation{ Model=SensorKit.HUB_SENSOR_ID, ShortDescription = "Sensor Hub Device", Capabilities = SensorCapabilities.Hub },
            // illumiSens
            new SensorInformation{ Model="illumiSens", Manufacturer="illumiSens", Url="https://illumisens.com/products/illumisens-body-sensors", ShortDescription = "Body sensors", Capabilities = SensorCapabilities.IMU },
            new SensorInformation{ Model="illumiSki", Manufacturer="illumiSens", Url = "https://illumisens.com/products/illumiski-ski-sensor", ShortDescription = "Sensors for ski and snowboard", Capabilities = SensorCapabilities.IMU | SensorCapabilities.ChangeColor },
            new SensorInformation{ Model="illumiBand", Manufacturer="illumiSens", Url="https://illumisens.com/products/illumiband-wearable-sensors", ShortDescription = "Sensor enabled wearable band", Capabilities = SensorCapabilities.IMU | SensorCapabilities.Button }
            // add your sensors here...           
        };

        public IEnumerable<SensorInformation> Public
        {
            get
            {
                return _registry.Where(s=>s.Model != SensorKit.HUB_SENSOR_ID);
            }
        }
        
    }

    
}
