using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using Lexmou.MachineLearning;
using Lexmou.MachineLearning.Evolutionary;

public class DroneSessionTest
{
    GameObject gObject;
    Session droneSession;

    [TestFixtureSetUp]
    public void Init()
    {

    }

    [TestFixtureTearDown]
    public void CleanUp()
    {

    }

    [SetUp]
    public void SetUp()
    {
        int seed = 65;
        gObject = new GameObject();
        gObject.AddComponent<DroneSession>();
        droneSession = gObject.GetComponent<DroneSession>();
        gObject.GetComponent<DroneSession>().seed = seed;
        Debug.Log(gObject.GetComponent<DroneSession>().seed);
    }

    [TearDown]
    public void TearDown()
    {
        //SetUp runs after all test cases
    }

    [Test]
    public void BuildTest()
    {
        gObject.GetComponent<DroneSession>().Build();
    }

    [Test]
    public void ResetTest()
    {
        gObject.GetComponent<DroneSession>().Reset();
        Debug.Log(gObject.GetComponent<DroneSession>().mlpPopulation[2].weights[0]);
    }
}
