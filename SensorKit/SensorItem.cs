using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SensorKit
{
    [DataContract]
    public class SensorItem
    {
        public DateTimeOffset timestamp { get; set; }

        public int activityTypeId { get; set; }

        public double aX { get; set; }
        public double aY { get; set; }
        public double aZ { get; set; }
        public double avX { get; set; }
        public double avY { get; set; }
        public double avZ { get; set; }
        public double qW { get; set; }
        public double qX { get; set; }
        public double qY { get; set; }
        public double qZ { get; set; }

        public double lat { get; set; }
        public double lon { get; set; }
        public double speed { get; set; }
        public double alt { get; set; }
        public double incl { get; set; }

        public Vector3 ToEulerAngles()
        {
            // Derivation from http://www.geometrictools.com/Documentation/EulerAngles.pdf
            // Order of rotations: Z first, then X, then Y
            float check = 2.0f * (float)(-qY * qZ + qW * qX);
            const float radToDeg = 180f / (float)Math.PI;

            if (check < -0.995f)
            {
                return new Vector3(-90f, 0f, -(float)Math.Atan2(2.0f * (qX * qZ - qW * qY), 1.0f - 2.0f * (qY * qY + qZ * qZ)) * radToDeg);
            }
            else if (check > 0.995f)
            {
                return new Vector3(90f, 0f, (float)Math.Atan2(2.0f * (qX * qZ - qW * qY), 1.0f - 2.0f * (qY * qY + qZ * qZ)) * radToDeg);
            }
            else
            {
                return new Vector3(
                    (float)Math.Asin(check) * radToDeg,
                    (float)Math.Atan2(2.0f * (qX * qZ - qW * qY), 1.0f - 2.0f * (qX * qX + qY * qY)) * radToDeg,
                    (float)Math.Atan2(2.0f * (qX * qY - qW * qZ), 1.0f - 2.0f * (qX * qX + qZ * qZ)) * radToDeg);
            }
        }

        public float YawAngle => ToEulerAngles().Y;

        public float PitchAngle => ToEulerAngles().X;

        public float RollAngle => ToEulerAngles().Z;
    }
}
