using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using MathNet.Numerics;

namespace Lexmou.Utils
{
    /**
     * \class UVector
     * \brief Util class for MathNet.Numerics.Vector
     * \details This UVector static class provides a set of utility static methods that can be used to perform operations on MathNet.Numerics.Vector
     */

    public static class UVector
    {

        /**
        * \brief Mean of a Vector
        * \details Build the mean of a Vector and convert a \e MathNet.Numerics.Tuple<double, double> to \e MathNet.Numerics.Tuple<float, float>
        * \param[in]   V  Vector used
        * \return A \e float which is the mean
        */
        public static float BuildMean(Vector<float> V)
        {
            return (float)Statistics.Mean(V);
        }

        /**
        * \brief Standard deviation of a Vector
        * \details Build the standard deviation of a Vector and convert a \e MathNet.Numerics.Tuple<double, double> to \e MathNet.Numerics.Tuple<float, float>
        * \param[in]   V  Vector used
        * \return A \e float which is the standard deviation
        */
        public static float BuildStdDeviation(Vector<float> V)
        {
            return (float)Statistics.StandardDeviation(V);
        }

        /**
        * \brief Mean and standard deviation of a Vector
        * \details Build the mean and the standard deviation of a Vector and convert a \e MathNet.Numerics.Tuple<double, double> to \e MathNet.Numerics.Tuple<float, float>
        * \param[in]   V  Vector used
        * \return A \e MathNet.Numerics.Tuple<float, float> where item1 is the mean of V and item2 is the standard deviation
        */
        public static Tuple<float,float> BuildMeanStdVariation(Vector<float> V)
        {
            Tuple<double, double> tmp = Statistics.MeanStandardDeviation(V);
            return new Tuple<float, float>((float)tmp.Item1, (float)tmp.Item2);
        }

        /**
        * \brief Median of a Vector
        * \details Build the median of a Vector
        * \param[in]   V  Vector used
        * \return  A \e float which is the median
        */

        public static float BuildMedian(Vector<float> V)
        {
            return Statistics.Median(V);
        }
    }
}
