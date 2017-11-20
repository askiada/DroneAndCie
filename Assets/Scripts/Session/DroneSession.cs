using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lexmou.Manager;
using Lexmou.Utils;
using System;
using System.IO;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using Lexmou.MachineLearning.Evolutionary;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Drone.Hardware;
using System.Linq;


namespace Lexmou.MachineLearning
{
    public class DroneSession : Lexmou.MachineLearning.Session {

        public HUDManager hudManager;
        public StreamWriter writerSession;
        public List<Matrix<float>> tmpBuildCustomWeights;
        float theoricBestScore;
        public float initialValueWeights = 1.0f;
        public int populationSize = 100;
        public float mutationRate = 0.1f;
        public float randomIndividualsRate = 0.1f;
        public float bestIndividualsRate = 0.1f;
        public float emptyRate = 0.93f;
        public string task = "Stabilization";
        public DroneTask taskObject;
        Vector<float> externalEvaluations;
        //private int individualSize;
        private Genetic gene;
        public int _intervalSave;
        public int intervalSave { get { return _intervalSave; } set { _intervalSave = value; } }
        public int _loadGeneration;
        public int loadGeneration { get { return _loadGeneration; } set { _loadGeneration = value; } }

        public Rigidbody[] droneRigid;
        public GameObject[] dronePopulation;
        public MultiLayerMathsNet[] mlpPopulation;

        private ControlSignal signal;

        public override string GeneratePath(bool withSeed = false)
        {
            if (withSeed)
            {
                return "Save/DroneSession/" + "Task-" + task + "/Seed-" + seed + "/";
            }
            else
            {
                return "Save/DroneSession/" + "Task-" + task + "/";       
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
            signal = new ControlSignal();
            tmpBuildCustomWeights = new List<Matrix<float>>();
            externalEvaluations = Vector<float>.Build.Dense(populationSize);
            taskObject = (DroneTask)Activator.CreateInstance(Type.GetType("Lexmou.MachineLearning.Drone" + task));
            dronePopulation = new GameObject[populationSize];
            droneRigid = new Rigidbody[populationSize];
            mlpPopulation = new MultiLayerMathsNet[populationSize];
            
            gene = new Genetic(seed, rndGenerator, populationSize, taskObject.individualSize, initialValueWeights, mutationRate, randomIndividualsRate, bestIndividualsRate, emptyRate, GeneratePath(false));
            if (loadGeneration != 0)
            {
                float[,] floatArr = new float[taskObject.individualSize, populationSize];
                gene.LoadGeneration(loadGeneration, floatArr);
                gene.generation = loadGeneration;
            }

        }
        public override void RunEachIntervalUpdate()
        {
            string generationInfos;
            generationInfos = gene.Run(externalEvaluations, theoricBestScore, intervalSave);
            hudManager.UpdateTextLayout("Generation", generationInfos);
            theoricBestScore = 0;
        }

        public override void RunEachFixedUpdate()
        {
            theoricBestScore += 1;
            for (int i = 0; i < populationSize; i++)
            {
                mlpPopulation[i].PropagateForward(taskObject.UCSignal(droneRigid[i]).input);
                
                signal.Throttle = mlpPopulation[i].layers[taskObject.shapes.Count - 1][3, 0];
                signal.Rudder = mlpPopulation[i].layers[taskObject.shapes.Count - 1][0, 0];
                signal.Elevator = mlpPopulation[i].layers[taskObject.shapes.Count - 1][1, 0];
                signal.Aileron = mlpPopulation[i].layers[taskObject.shapes.Count - 1][2, 0];

                dronePopulation[i].GetComponent<MainBoard>().SendControlSignal(signal);
                externalEvaluations[i] += taskObject.EvaluateIndividual(i, droneRigid[i]);
            }
        }

        void BuildCustomWeights(List<int> shapes, Vector<float> individual)
        {
            tmpBuildCustomWeights.Clear();
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                if (i == 0)
                {
                    tmpBuildCustomWeights.Add(UMatrix.Make2DMatrix(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]), shapes[i + 1], shapes[i] + 1));
                }
                else
                {
                    tmpBuildCustomWeights.Add(UMatrix.Make2DMatrix(individual.SubVector((shapes[i - 1] + 1) * shapes[i], (shapes[i] + 1) * shapes[i + 1]), shapes[i + 1], shapes[i] + 1));
                }
            }
        }

        public override void Reset()
        {
            //Debug.Log("Reset");
            for (int i = 0; i < populationSize; i++)
            {
                dronePopulation[i] = MonoBehaviour.Instantiate((GameObject)Resources.Load("DroneGene"), taskObject.GetInitialposition(i), Quaternion.identity) as GameObject;
                droneRigid[i] = dronePopulation[i].GetComponentInChildren<Rigidbody>();
                mlpPopulation[i] = new MultiLayerMathsNet(-1, rndGenerator, taskObject.shapes, 1, initialValueWeights);
                //if(gene.generation == 1)
                //Suspect, il doit y avoir une fuite mémoire ici je pense
                BuildCustomWeights(taskObject.shapes, gene.GetIndividual(i));
                mlpPopulation[i].Reset(false, tmpBuildCustomWeights);
            }
            externalEvaluations = Vector<float>.Build.Dense(populationSize);
        }

        public override void OnDestroy()
        {
            Debug.Log("Destroy Genetic Writer");
            UIO.CloseStreamWriter(gene.writer);
            //UIO.CloseStreamWriter(writer);
        }

        public override void Destroy()
        {
            for (int i = 0; i < populationSize; i++)
            {
                Destroy(dronePopulation[i]);
                mlpPopulation[i] = null;
                droneRigid[i] = null;
            }
        }

        public override void CloseSessionWriter()
        {
            UIO.CloseStreamWriter(writerSession);
        }

        public override void BuildSessionWriter()
        {
            writerSession = UIO.CreateStreamWriter(GeneratePath(true), "DroneSessionParams.csv", false);
            UIO.WriteLine(writerSession, "Task : " + task + ";" +
                                     "Seed : " + seed + ";" +
                                     "Population Size : " + populationSize + ";" +
                                     "Individual Size : " + taskObject.individualSize + ";" +
                                     "Mutation Rate : " + mutationRate + ";" +
                                     "Random Rate : " + randomIndividualsRate + ";" +
                                     "Best Individual Rate : " + bestIndividualsRate + ";" +
                                     "Empty Coeff Individual Rate : " + emptyRate + " (" + Mathf.RoundToInt(emptyRate * taskObject.individualSize) + ");" +
                                     "MLP layers : [" + string.Join("-", taskObject.shapes.Select(x => x.ToString()).ToArray()) + "] ; Intitial Value Weights :  [" + -initialValueWeights + "," + initialValueWeights + "]");
        }

        public override void BuildHUD()
        {
            hudManager = this.gameObject.AddComponent<HUDManager>() as HUDManager;
            hudManager.AddTextLayout("Task", task);
            hudManager.AddTextLayout("Best Drone Info", "");
            hudManager.AddTextLayout("Position", "--------");
            hudManager.AddTextLayout("Commands", "--------");
            hudManager.AddTextLayout("Time Scale", Time.timeScale.ToString() + "x");
            hudManager.AddTextLayout("Generation", "--------");
            hudManager.AddTextLayout("Infos", "Seed : " + seed + "\r\n" +
                         "Pop Size : " + populationSize + "\r\n" +
                         "Ind Size : " + taskObject.individualSize + "\r\n" +
                         "Mrate : " + mutationRate + "\r\n" +
                         "Rrate : " + randomIndividualsRate + "\r\n" +
                         "Brate : " + bestIndividualsRate + "\r\n" +
                         "ERate : " + emptyRate + " (" + Mathf.RoundToInt(emptyRate * taskObject.individualSize) + ")\r\n" +
                         "MLP : [" + string.Join("-", taskObject.shapes.Select(x => x.ToString()).ToArray()) + "] init " + initialValueWeights
                         );
        }
    }

}
