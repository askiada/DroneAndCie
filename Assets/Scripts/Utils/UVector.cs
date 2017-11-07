using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using MathNet.Numerics;

namespace Lexmou.Utils
{
    public static class UVector
    {
        public static Tuple<float,float> BuildMeanStdVariation(Vector<float> V)
        {
            Tuple<double, double> tmp = Statistics.MeanStandardDeviation(V);
            return new Tuple<float,float>((float)tmp.Item1, (float)tmp.Item2);
        }

        public static float BuildMedian(Vector<float> V)
        {
            return Statistics.Median(V);
        }
    }
}
