using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Utils
{
    public static class MathUtility
    {
        public static Vector2 RotateVector(Vector2 v, float degree)
        {
            float delta = Mathf.Deg2Rad * degree;
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        public static float NormalRNG(float mean, float stdDev)
        {
            Random rand = new Random();
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (float)(mean + stdDev * randStdNormal);
        }

        public static float FullAngle(Vector2 first, Vector2 second)
        {
            var dot = Vector2.Dot(first, second);
            var deter = first.x * second.y - first.y * second.x;
            var angle = (float)ConvertRadiansToDegrees(Math.Atan2(deter, dot));
            if (angle < 0)
                angle += 360;
            return angle;
        }

        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        public static float NormalizeAngle(float angle)
        {
            angle = angle % 360;

            if (angle < 0)
            {
                angle += 360;
            }
            return angle;
        }

        public static float AngleDifference(float firstAngle, float secondAngle)
        {
            float difference = secondAngle - firstAngle;
            while (difference < -180f) difference += 360f;
            while (difference > 180f) difference -= 360f;
            return difference;
        }

        public static float BringAnglesCloser(float source, float destination, float change)
        {
            var up = AngleDifference(source + change, destination);
            var down = AngleDifference(source - change, destination);

            if (Math.Abs(up) < Math.Abs(down))
            {
                return source + change;
            }

            return source - change;
        }
    }
}
