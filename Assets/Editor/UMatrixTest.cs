using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using Lexmou.Utils;
public class UMatrixTest : MonoBehaviour
{

    [TestFixtureTearDown]
    public void CleanUp()
    {

    }

    [SetUp]
    public void SetUp()
    {

    }

    [TearDown]
    public void TearDown()
    {

    }

    [Test]
    public void Make2DMatrixTest()
    {
        int vSize = 15;
        Vector<float> V = Vector<float>.Build.Random(vSize);
        int iter = 100000;
        Matrix<float> M;
        var watch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iter; i++)
        {

            M = UMatrix.Make2DMatrix(V, 5, 3);
        }

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Time Col mode: " + elapsedMs);


        watch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iter; i++)
        {

             M = UMatrix.Make2DMatrix(V, 5, 3, false);
        }

        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Time Row mode: " + elapsedMs);
        M = UMatrix.Make2DMatrix(V, 5, 3);
        Debug.Log(V);
        Debug.Log(M);

    }
}