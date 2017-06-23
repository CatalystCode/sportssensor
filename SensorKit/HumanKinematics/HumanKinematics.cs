using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorKit
{
    public class HumanKinematics
    {
        public HumanBodyBones SensorBodyPosition { get; set; }

        public IEnumerable<string> HumanBodyBones
        {
            get
            {
                return Enum.GetNames(typeof(HumanBodyBones)).OrderBy(s=>s);

            }
        }
    }
}
