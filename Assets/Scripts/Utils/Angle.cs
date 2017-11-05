using UnityEngine;
using System.Collections;


namespace Lexmou.Utils
{
    public static class Angle {

        public static float SteerAngle(float value)
        {
            if (value > 180)
                return value -= 360;
            else return value;
        }
    }
}