using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class GeneTest
{
    int seed;
    Gene geneSeed;
    Gene geneRndGenerator;

    [TestFixtureSetUp]
    public void Init()
    {
        
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
        seed = 65;
        geneSeed = new Gene(seed, null, 2, 10, 1.0f);
        //Debug.Log(geneSeed.ToString());
        //System.Threading.Thread.Sleep(10000);
        SystemRandomSource rndGenerator = new SystemRandomSource(seed);

        geneRndGenerator = new Gene(seed, rndGenerator, 2, 10);
    }

    [TearDown]
    public void TearDown()
    {
        //SetUp runs after all test cases
    }

    [Test]
    public void TypeRndGeneratorTest()
    {
        //Debug.Log(geneSeed.rndGenerator);
        //double seedDouble = geneSeed.rndGenerator.NextDouble();
        //System.Threading.Thread.Sleep(1000);
        //double rndGeneratorDouble = geneRndGenerator.rndGenerator.NextDouble();
        Assert.IsNotNull(geneSeed.rndGenerator);
        Assert.IsNotNull(geneRndGenerator.rndGenerator);
    }

    [Test]
    public void GeneratePopulationTest()
    {
        
        //double seedDouble = geneSeed.rndGenerator.NextDouble();
        //double rndGeneratorDouble = geneRndGenerator.rndGenerator.NextDouble();
        Assert.AreNotEqual(geneSeed.GetIndividual(0), geneSeed.GetIndividual(1));
        Assert.AreNotEqual(geneRndGenerator.GetIndividual(0), geneRndGenerator.GetIndividual(1));
    }

    [Test]
    public void EvaluationTest()
    {
        Vector<float> externalEvaluations = Vector<float>.Build.DenseOfArray(new float[2] { 1.0f, 0.0f });
        geneSeed.Evaluation(externalEvaluations, "max");
        Assert.AreEqual(0, geneSeed.GetBestIndividualIndex());
        geneSeed.Evaluation(externalEvaluations, "min");
    }

    [Test]
    public void SelectOneElementTest()
    {
        Vector<float> externalEvaluations = Vector<float>.Build.DenseOfArray(new float[2] { 1.0f, 0.0f });
        geneSeed.Evaluation(externalEvaluations, "max");
        Vector<float> selected = geneSeed.SelectionOneElement();

        Assert.AreEqual(geneSeed.GetIndividual(0), selected);

        externalEvaluations[0] = 0.0f;
        externalEvaluations[1] = 1.0f;
        geneSeed.Evaluation(externalEvaluations, "max");
        selected = geneSeed.SelectionOneElement();

        Assert.AreEqual(geneSeed.GetIndividual(1), selected);
    }

    [Test]
    public void CrossoverSliceTwoIndividualsTest()
    {
        Vector<float> first = geneSeed.GetIndividual(0);
        Vector<float> second = geneSeed.GetIndividual(1);


        geneSeed.CrossoverSliceTwoIndividuals(0, 1, 2, 5);


        Assert.AreEqual(first.SubVector(0, 2), geneSeed.GetIndividual(1).SubVector(0, 2));
        Assert.AreEqual(second.SubVector(0, 2), geneSeed.GetIndividual(0).SubVector(0, 2));

        Assert.AreEqual(first.SubVector(5, 5), geneSeed.GetIndividual(1).SubVector(5, 5));
        Assert.AreEqual(second.SubVector(5, 5), geneSeed.GetIndividual(0).SubVector(5, 5));


        /*Debug.Log(geneSeed.GetPopulation());
        var watch = System.Diagnostics.Stopwatch.StartNew();
        for(int i=0; i < 1000000;i++)
            geneSeed.CrossoverSliceTwoIndividualsColumns(0, 1, 2, 5);
        
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log("Time Matrix: " + elapsedMs);

        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
            geneSeed.CrossoverSliceTwoIndividuals(0, 1, 2, 5);
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;
        Debug.Log("Time Column: " + elapsedMs);*/

        //geneSeed.CrossoverSliceTwoIndividualsColumns(0, 1, 2, 5);

        //Assert.AreEqual();

        //Debug.Log(geneSeed.GetIndividual(0));// + " " + geneSeed.GetIndividual(1));
        //Debug.Log(geneSeed.GetIndividual(1));
    }

    [Test]

    public void MutationTest()
    {
        Matrix<float> tmp = Matrix<float>.Build.DenseOfMatrix(geneSeed.GetPopulation());
        geneSeed.GetPopulation().CopyTo(tmp);
        geneSeed.Mutation();
        Assert.AreNotEqual(tmp, geneSeed.GetPopulation());
    }


    [Test]

    public void SelectionTest()
    {
        int size = 3;
        float[] arr = new float[size];
        for (int i = 0; i < size; i++)
        {
            arr[i] = 1.0f;
        }
        arr[size - 1] = 2.0f;
        Vector<float> externalEvaluations = Vector<float>.Build.DenseOfArray(arr);

        Gene gene = new Gene(seed, null, size, 3, 0.1f, 0.34f, 0.34f);
        gene.Evaluation(externalEvaluations, "max");
        Debug.Log(gene.GetPopulation());
        gene.Selection();
        
    }


}
