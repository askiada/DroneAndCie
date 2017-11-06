using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.Linq;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;

public class MLPTest
{
    int seed;
    List<int> shapes;
    MultiLayer mlp;
    MultiLayerMathsNet mlpMN;
    float initialValueWeights;
    [TestFixtureSetUp]
    public void Init()
    {
        this.seed = 42;
        this.shapes = new List<int>() { 9, 20, 10, 5, 4};
        this.initialValueWeights = 1.0f;
    }

    [TestFixtureTearDown]
    public void CleanUp()
    {
        //geneSeed = null;
        //geneRndGenerator = null;
        //CleanUp runs once after all test cases are finished.
    }

    [SetUp]
    public void SetUp()
    {
        mlpMN = new MultiLayerMathsNet(seed, null, shapes, 1, initialValueWeights);
        mlp = new MultiLayer(shapes, seed, 1, null);
    }

    [TearDown]
    public void TearDown()
    {
        //SetUp runs after all test cases
    }

    [Test]
    public void InstanceSpeedTest()
    {
        int iter = 1000;
        mlp = null;
        mlpMN = null;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {
            mlpMN = new MultiLayerMathsNet(seed, null, shapes, 1, initialValueWeights);
            mlpMN = null;
        }
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Time MathNet: " + elapsedMs);


        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {
            mlp = new MultiLayer(shapes, seed, 1, null);
            mlp = null;
        }
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Time Array: " + elapsedMs);
    }

    [Test]
    public void ResetSpeedTest()
    {
        int iter = 1000;
        mlpMN = new MultiLayerMathsNet(seed, null, shapes, 1, initialValueWeights);
        mlp = new MultiLayer(shapes, seed, 1, null);


        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {
            
            mlpMN.Reset(true);
        }
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Reset Time MathNet: " + elapsedMs);


        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {
            
            mlp.Reset(1.0f, true);
        }
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Reset Time Array: " + elapsedMs);
    }

    [Test]
    public void CustomResetSpeedTest()
    {
        int iter = 10000;
        float initialValueWeights = 1.0f;

        mlp.Reset(initialValueWeights, true);
        mlpMN.Reset(true);

        SystemRandomSource rndGenerator = new SystemRandomSource(seed);
        List<Matrix<float>> weightsMN = new List<Matrix<float>>();
        List<float[,]> weights = new List<float[,]>();
        for (int i = 0; i < shapes.Count - 1; i++)
        {
            weightsMN.Add(Matrix<float>.Build.Random(mlpMN.layers[i + 1].RowCount, mlpMN.layers[i].RowCount, new ContinuousUniform(-initialValueWeights, initialValueWeights, rndGenerator)));
        }

        for (int i = 0; i < shapes.Count - 1; i++)
        {
            weights.Add(new float[mlp.layers[i].GetLength(1), mlp.layers[i + 1].GetLength(1)]);
            
        }
        for (int i = 0; i < shapes.Count - 1; i++)
        {
            for (int j = 0; j < weights[i].GetLength(0); j++)
            {
                for (int k = 0; k < weights[i].GetLength(1); k++)
                {

                    weights[i][j, k] = (float)ContinuousUniform.Sample(rndGenerator, -initialValueWeights, initialValueWeights);
                }
            }
        }

        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {

            mlpMN.Reset(false, weightsMN);
        }
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Reset Time MathNet: " + elapsedMs);


        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {

            mlp.Reset(initialValueWeights, false, weights);
        }
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Reset Time Array: " + elapsedMs);

        Assert.AreEqual(mlp.weights, weights);
        Assert.AreEqual(mlpMN.weights, weightsMN);
    }

    [Test]

    public void PropagateForwardTest()
    {
        /*SystemRandomSource rndGenerator = new SystemRandomSource(seed);
        ContinuousUniform cu = new ContinuousUniform(-initialValueWeights, initialValueWeights, rndGenerator);
        */
        Vector<float> data = Vector<float>.Build.DenseOfArray(new float[] { 1,2,3,4,5,6,7,8,9}); ;
        int iter = 10000;


        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {
            //data = Vector<float>.Build.DenseOfArray(new float[]  { i, i + 1, i + 2, i + 3, i + 4, i + 5  });
            mlpMN.PropagateForward2(data);
        }
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("PropagateForward Array Time MathNet: " + elapsedMs);



        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {
            mlpMN.PropagateForward(data);
        }
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("PropagateForward2 Multiply Time MathNet: " + elapsedMs);

        float[,] dataArr  = new float[,] { { 1,2,3,4,5,6,7,8,9 } };
        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iter; i++)
        {
            mlp.PropagateForward(dataArr);
        }
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("PropagateForward Time Array: " + elapsedMs);
        //mlpMN.PropagateForward2(data);
    }

    /*[Test]
    public void MatrixMultiplyTest()
    {
        var ns = new List<int> { 5000, 10000, 20000, 40000 };
        var d = 10;
        var small = DenseMatrix.CreateRandom(d, d, new ContinuousUniform());
        foreach (var n in ns)
        {
            var tall = DenseMatrix.CreateRandom(n, d, new ContinuousUniform());
            var sw1 = new System.Diagnostics.Stopwatch();
            sw1.Start();
            var prod1 = tall.Multiply(small);
            sw1.Stop();
            var sw2 = new System.Diagnostics.Stopwatch();
            sw2.Start();
            var prod2 = DenseMatrix.OfColumnVectors(tall.EnumerateRows().Select(row => small.Transpose().Multiply(row))).Transpose();
            sw2.Stop();
            Debug.Log("n = " + n + ", MathNet: " + sw1.ElapsedMilliseconds / 1000.0 + "s, Naive: " + sw2.ElapsedMilliseconds / 1000.0 + "s.");
            Assert.That(prod1.ToArray(), Is.EquivalentTo(prod2.ToArray()));
        }
    }*/

}
