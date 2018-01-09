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

using System.Reflection;

/*! \mainpage Drone et Compagnie


##Présentation

La robotique est un enjeu majeur des années à venir. Que cela soit en matière de développement technologique ou d'éthique, il est clair que la période actuelle fera office de charnière pour les années à venir. Actuellement, les robots les plus pointus sont situés aux extrêmes des domaines d'applications. En effet, les avancées du robot Rosa ont fait des miracles en chirurgie tandis que les poupées sexy Japonaise relève de l'exploit niveau reproduction humaine. Pour autant, le champ est encore large et il reste probablement plus à découvrir que tout ce qui a été fait jusqu'à maintenant.

Après cette introduction un peu pompeuse, il est temps de rentrer dans le vif du sujet. Dans le cadre d'un GROS projet personnel, il m'est venu à l'esprit qu'il serait utile d'avoir à ma disposition un quadcopter autonome fiable et peu cher. Et donc c'est ainsi qu'est né ce projet sans informations sur sa date de décès. En effet, l'enjeu est de taille et il est tout à fait possible que l'idée soit mise de côté à plus ou moins long terme. Trêve de discussion, il est grand temps d'exposer le plan de jeu.

##Objectifs

1. Modéliser un Quadcopter dans Unity3D. La simulation physique doit être "réaliste".
2. Créer un système de pilotage manuel (utilisation d'un gamepad)
3. Mettre en place une aide :
     - Stabilisation
     - Détection des obstacles et correction		
4. Autonomie en milieu connu
5. Autonomie en milieu inconnu

##Environnement de travail

Les goûts et les couleurs...
Il est très probable que le développement du projet révèle un grands nombres de points faibles à l'environnement de travail choisi. Mais, pour des raisons de simplicité de mise en place du début de projet, Unity3D a semblé la solution la plus aisée (déjà réalisé un systême multi-agents dessus).
Au moment de l'écriture de cette article, tout a été réalisé sur Windows 10 avec Unity 5.6.4.

A priori, il faudra mettre en place un serveur sur linux afin de faire tourner les longues simulations sans bloquer le pc de travail. Il est possible de facilement désactiver l'affichage sous Ubuntu.
*/

namespace Lexmou.MachineLearning
{
    public class DroneSession : Lexmou.MachineLearning.Session
    {
        public bool directSignal = false;

        public bool _save = false;
        public bool save { get { return _save; } set { _save = value; } }
        public string _fromTask = "Stabilization";
        public string fromTask { get { return _fromTask; } set { _fromTask = value; } }
        public HUDManager hudManager;
        public StreamWriter writerSession;
        public StreamWriter writerEnv;
        public List<Matrix<float>> tmpBuildCustomWeights;
        float theoricBestScore;
        public float initialValueWeights = 1.0f;
        public int populationSize = 100;
        public float mutationRate = 0.1f;
        public float randomIndividualsRate = 0.1f;
        public float bestIndividualsRate = 0.1f;
        public float emptyRate = 0.93f;
        public string _task = "Stabilization";
        public string task { get { return _task; } set { _task = value; } }
        public DroneTask taskObject;
        Vector<float> externalEvaluations;
        //private int individualSize;
        private Genetic gene;
        string generationInfos;
        public int _intervalSave;
        public int intervalSave { get { return _intervalSave; } set { _intervalSave = value; } }
        public int _loadGeneration;
        public int loadGeneration { get { return _loadGeneration; } set { _loadGeneration = value; } }

        public Rigidbody[] droneRigid;
        public GameObject[] dronePopulation;
        public MultiLayerMathsNet[] mlpPopulation;
        public Vector3[] targetPosition;

        private ControlSignal signal;
        private ThrustSignal tsignal;


        public override string GeneratePath(string task, bool withSeed = false)
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
            UIO.CheckBeforeReplaceCommandLineArguments(this, "save");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "task");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "fromTask");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "seed");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "intervalSave");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "loadGeneration");
            UIO.CheckBeforeReplaceCommandLineArguments(this, "timeScale");
        }

        public override void Build()
        {
            if (save)
            {
                writerEnv = UIO.CreateStreamWriter(GeneratePath(task, true), "GeneSessionResults.csv", false);
                UIO.WriteLine(writerEnv, "Generation;Angle Random Rotation;Wind");
            }
            Debug.Log("Build DroneSession");
            signal = new ControlSignal();
            tsignal = new ThrustSignal();
            tmpBuildCustomWeights = new List<Matrix<float>>();
            externalEvaluations = Vector<float>.Build.Dense(populationSize);
            //Debug.Log(fromTask);
            taskObject = (DroneTask)Activator.CreateInstance(Type.GetType("Lexmou.MachineLearning.Drone" + task), rndGenerator, fromTask);
            //Debug.Log(taskObject.fromTask);
            dronePopulation = new GameObject[populationSize];
            droneRigid = new Rigidbody[populationSize];
            targetPosition = new Vector3[populationSize];
            mlpPopulation = new MultiLayerMathsNet[populationSize];

            gene = new Genetic(seed, rndGenerator, populationSize, taskObject.individualSize, initialValueWeights, mutationRate, randomIndividualsRate, bestIndividualsRate, emptyRate, GeneratePath(task, false), save);
            if (loadGeneration != 0)
            {
                float[,] floatArr = new float[taskObject.individualSize - taskObject.rowIndex, populationSize];
                Debug.Log(taskObject.fromTask);
                //Debug.Log(taskObject.individualSize);
                gene.LoadGeneration(GeneratePath(taskObject.fromTask, true), loadGeneration, floatArr, taskObject.rowIndex);
                gene.generation = loadGeneration;
            }

        }
        public override void RunEachIntervalUpdate()
        {
            generationInfos = gene.Run(externalEvaluations, theoricBestScore, intervalSave);
            hudManager.UpdateTextLayout("Generation", generationInfos);
            theoricBestScore = 0;
            if (save)
                UIO.WriteLine(writerEnv, gene.generation + ";" + taskObject.AngleRandomRotation + ";" + taskObject.WindStrength);
        }

        private float MapEvaluate(int index, float value)
        {
            return value + taskObject.EvaluateIndividual(index, droneRigid[index], targetPosition[index]);
        }

        public override void RunEachFixedUpdate()
        {
            theoricBestScore += 1;
            for (int i = 0; i < populationSize; i++)
            {
                taskObject.UCSignal(droneRigid[i], targetPosition[i]);
                //Ici se trouve la plus grosse allocation à chaque frame !!! Si falk est vrai un poil plus lent mais moins de garbage (13.5kB contre 55kB)
                mlpPopulation[i].PropagateForward(taskObject.signal.input, true);


                if (directSignal)
                {
                    tsignal.FRThrust = mlpPopulation[i].layers[taskObject.shapes.Count - 1][0, 0];
                    tsignal.FLThrust = mlpPopulation[i].layers[taskObject.shapes.Count - 1][1, 0];
                    tsignal.RRThrust = mlpPopulation[i].layers[taskObject.shapes.Count - 1][2, 0];
                    tsignal.RLThrust = mlpPopulation[i].layers[taskObject.shapes.Count - 1][3, 0];

                    dronePopulation[i].GetComponent<MainBoard>().SendThrustSignal(tsignal);
                }
                else {

                    signal.Throttle = mlpPopulation[i].layers[taskObject.shapes.Count - 1][3, 0];
                    signal.Rudder = mlpPopulation[i].layers[taskObject.shapes.Count - 1][0, 0];
                    signal.Elevator = mlpPopulation[i].layers[taskObject.shapes.Count - 1][1, 0];
                    signal.Aileron = mlpPopulation[i].layers[taskObject.shapes.Count - 1][2, 0];

                    dronePopulation[i].GetComponent<MainBoard>().SendControlSignal(signal);
                }

            }
            externalEvaluations.MapIndexedInplace(MapEvaluate);
        }

        void BuildCustomWeights(List<Matrix<float>> newWeights, List<int> shapes, Vector<float> individual)
        {
            int bias = 1;
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                if (i == shapes.Count - 2)
                    bias = 0;
                if (i == 0)
                {
                    // Debug.Log(newWeights[i]);
                    // Debug.Log(individual.SubVector(0, (shapes[i] + 1) * (shapes[i + 1] + bias)));

                    UMatrix.Make2DMatrix(newWeights[i], individual.SubVector(0, (shapes[i] + 1) * (shapes[i + 1] + bias)), (shapes[i + 1] + bias), shapes[i] + 1);
                }
                else
                {
                    UMatrix.Make2DMatrix(newWeights[i], individual.SubVector((shapes[i - 1] + 1) * shapes[i], (shapes[i] + 1) * (shapes[i + 1] + bias)), (shapes[i + 1] + bias), shapes[i] + 1);
                }
            }
        }

        public override void Reset()
        {
            for (int i = 0; i < populationSize; i++)
            {

                dronePopulation[i] = MonoBehaviour.Instantiate((GameObject)Resources.Load("DroneGene"), taskObject.GetInitialposition(i), Quaternion.identity) as GameObject;
                droneRigid[i] = dronePopulation[i].GetComponentInChildren<Rigidbody>();
                taskObject.GetTargetPosition(i, targetPosition);
                //Debug.Log(targetPosition[i].ToString());
                //targetPosition[i] = 
                if (!taskObject.ResetOrientation(droneRigid[i], gene.bestScore, gene.median, i, populationSize))
                {
                    OnDestroy();
                    Application.Quit();
                    Time.timeScale = 0f;
                }
                if ((gene.generation == 1) || (gene.generation == loadGeneration))
                    mlpPopulation[i] = new MultiLayerMathsNet(-1, rndGenerator, taskObject.shapes, 1, initialValueWeights);
                BuildCustomWeights(mlpPopulation[i].weights, taskObject.shapes, gene.GetIndividual(i));
            }
            externalEvaluations.Clear();
        }

        public override void OnDestroy()
        {
            if (save)
            {
                Debug.Log("Destroy Genetic Writer");
                UIO.CloseStreamWriter(gene.writer);
                UIO.CloseStreamWriter(writerEnv);
            }
        }

        public override void Destroy()
        {
            for (int i = 0; i < populationSize; i++)
            {
                Destroy(dronePopulation[i]);
                //mlpPopulation[i] = null;
                droneRigid[i] = null;
                targetPosition[i] = Vector3.zero;
            }
        }

        public override void CloseSessionWriter()
        {
            if (save)
                UIO.CloseStreamWriter(writerSession);
        }

        public override void BuildSessionWriter()
        {
            if (save)
            {
                writerSession = UIO.CreateStreamWriter(GeneratePath(task, true), "DroneSessionParams.csv", false);
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
        }

        public override void BuildHUD()
        {
            hudManager = this.gameObject.AddComponent<HUDManager>() as HUDManager;
            hudManager.AddTextLayout("Task", task);
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
