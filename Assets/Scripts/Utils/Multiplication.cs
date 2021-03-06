﻿using UnityEngine;
using System;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public static class Multiplication  {

    public static float[,] FalkScheme(float[,] Matrix1, float[,] Matrix2, bool reverse1 = false, bool reverse2 = false)
    {
        if (UnityEngine.Object.ReferenceEquals(null, Matrix1))
            throw new System.ArgumentNullException("Matrix1");
        else if (UnityEngine.Object.ReferenceEquals(null, Matrix2))
            throw new System.ArgumentNullException("Matrix2");

        int r1 = Matrix1.GetLength(0);
        int c1 = Matrix1.GetLength(1);

        if (reverse1)
        {
            r1 = Matrix1.GetLength(1);
            c1 = Matrix1.GetLength(0);
        }


        int r2 = Matrix2.GetLength(0);
        int c2 = Matrix2.GetLength(1);

        if (reverse2)
        {
            r2 = Matrix1.GetLength(1);
            c2 = Matrix1.GetLength(0);
        }


        if (c1 != r2)
            throw new System.ArgumentOutOfRangeException("Matrix2", "Matrixes dimensions don't fit.");

        float[,] result = new float[r1, c2];

        // Naive matrix multiplication: O(n**3) 
        // Use Strassen algorithm O(n**2.81) in case of big matrices
        for (int r = 0; r < r1; ++r)
            for (int c = 0; c < c2; ++c)
            {
                float s = 0;

                for (int z = 0; z < c1; ++z)
                {
                    if (reverse1)
                    {
                        s += Matrix1[z, r] * Matrix2[z, c];
                    } else if (reverse2)
                    {
                        s += Matrix1[r, z] * Matrix2[c, z];
                    }
                    else
                    {
                        s += Matrix1[r, z] * Matrix2[z, c];
                    }   
                }
                result[r, c] = s;
            }

        return result;
    }
    /*int r1;
    int c1;
    int r2;
    int c2;*/
    public static void FalkSchemeM(Matrix<float> Matrix1, Matrix<float> Matrix2, Matrix<float> result, Func<float, float> f)
    {
        if (UnityEngine.Object.ReferenceEquals(null, Matrix1))
            throw new System.ArgumentNullException("Matrix1");
        else if (UnityEngine.Object.ReferenceEquals(null, Matrix2))
            throw new System.ArgumentNullException("Matrix2");

        int r1 = Matrix1.RowCount;
        int c1 = Matrix1.ColumnCount;

        int r2 = Matrix2.RowCount;
        int c2 = Matrix2.ColumnCount;
        //float s = 0;
        if (c1 != r2)
            throw new System.ArgumentOutOfRangeException("Matrix2", "Matrixes dimensions don't fit.");

        // Naive matrix multiplication: O(n**3) 
        for (int r = 0; r < r1; ++r)
            for (int c = 0; c < c2; ++c)
            {
                float s = 0;

                for (int z = 0; z < c1; ++z)
                {                  
                        s += Matrix1[r, z] * Matrix2[z, c];
                }
                result[r, c] = f(s);
            }
    }


    public static float[,] ElementWise(float[,] A, float[,] B)
    {
        for (int i = 0; i < A.GetLength(0); i++)
        {
            for (int j = 0; j < A.GetLength(1); j++)
            {
                A[i, j] *= B[i, j];
            }
        }
        return A;
    }
}
