using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lexmou.Utils;
using System;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using Lexmou.MachineLearning.Evolutionary;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace Lexmou.MachineLearning
{
    public class DroneSession : Lexmou.MachineLearning.Session {


        public List<Matrix<float>> tmpBuildCustomWeights;
        public float initialValueWeights = 1.0f;
        public int populationSize = 100;
        public float mutationRate = 0.1f;
        public float randomIndividualsRate = 0.1f;
        public float bestIndividualsRate = 0.1f;
        public float emptyRate = 0.93f;
        public string task = "Stabilization";
        public DroneTask taskObject;

        private int individualSize;
        private Genetic gene;

        public GameObject[] dronePopulation;
        public MultiLayerMathsNet[] mlpPopulation;

        public override string GeneratePath(bool withSeed = false)
        {
            if (withSeed)
            {
                return "Save/GeneSession/" + "Task-" + task + "/Seed-" + seed + "/";
            }
            else
            {
                return "Save/GeneSession/" + "Task-" + task + "/";       
            }
        }

        public override void SetParametersFromCommandLine()
        {
            UIO.CheckBeforeReplaceCommandLineArguments(this, "task");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "seed");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "intervalSave");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "loadGeneration");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "timeScale");
        }

        public override void Build()
        {
            Debug.Log("Build DroneSession");
            tmpBuildCustomWeights = new List<Matrix<float>>();
            taskObject = (DroneTask)Activator.CreateInstance(Type.GetType("Lexmou.MachineLearning.Drone" + task));
            dronePopulation = new GameObject[populationSize];
            mlpPopulation = new MultiLayerMathsNet[populationSize];
            
            gene = new Genetic(seed, rndGenerator, populationSize, taskObject.individualSize, initialValueWeights, mutationRate, randomIndividualsRate, bestIndividualsRate, emptyRate, GeneratePath());
        }
        public override void RunEachIntervalUpdate()
        {
            throw new NotImplementedException();
        }

        public override void RunEachFixedUpdate()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            taskObject.Build(new object[5] { dronePopulation, mlpPopulation, rndGenerator, initialValueWeights, tmpBuildCustomWeights });
        }

        public override void OnDestroy()
        {
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override void CloseSessionWriter()
        {
            throw new NotImplementedException();
        }

        public override void BuildSessionWriter()
        {
            throw new NotImplementedException();
        }

        public override void BuildHUD()
        {
            throw new NotImplementedException();
        }
    }

}
