using UnityEngine;
using System.Collections;


namespace Lexmou.Utils
{
    public static class UAngle {

        public static float SteerAngle(float value)
        {
            if (value > 180)
                return value -= 360;
            else return value;
        }


        public static float ReverseSteerAngle(float value)
        {
            if (value < 0)
                return value += 360;
            else return value;
        }
    }
}