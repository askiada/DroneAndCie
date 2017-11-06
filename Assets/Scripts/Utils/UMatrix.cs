using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Lexmou.Utils
{
    public static class UMatrix
    {

        public static Matrix<float> Make2DMatrix(Vector<float> V, int height, int width)
        {
            Matrix<float> tmp = Matrix<float>.Build.Dense(height, width);
            for (int i = 0; i < width; i++)
            {
                tmp.SetColumn(i, V.SubVector(height * i, height));
            }
            return tmp;
        }
    }
}
