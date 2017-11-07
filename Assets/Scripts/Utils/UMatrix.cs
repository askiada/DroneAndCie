﻿using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Lexmou.Utils
{
    public static class UMatrix
    {

        public static Matrix<float> Make2DMatrix(Vector<float> V, int height, int width, bool colMode = true)
        {
            if(height * width < V.Count)
            {
                throw new System.ArgumentOutOfRangeException("The size of V is not compatible with the shape of the matrix !");
            }
            Matrix<float> tmp = Matrix<float>.Build.Dense(height, width);
            if (colMode)
            {
                for (int i = 0; i < width; i++)
                {
                    tmp.SetColumn(i, V.SubVector(height * i, height));
                }
            }
            else
            {
                for (int i = 0; i < height; i++)
                {
                    tmp.SetRow(i, V.SubVector(width * i, width));
                }
            }
            
            return tmp;
        }
    }
}
