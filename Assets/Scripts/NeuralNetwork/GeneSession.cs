using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using Lexmou.MachineLearning.Evolutionary;
using Lexmou.Utils;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using System.Linq;
using System.IO;
using System.Reflection;
using Lexmou.Environment.Wind;
using Lexmou.Manager;

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


namespace Lexmou.MachineLearning.DroneSession2 { 
    public class GeneSession : MonoBehaviour
    {
        Rigidbody rigid;
        public StreamWriter writer;
        public StreamWriter writerSession;
        public string _task;
        public string task { get { return _task; } set { _task = value; } }

        public GameObject prefabDrone;
        private Genetic gene;
        public int _seed;
        public int seed { get { return _seed; } set { _seed = value; } }
        public int populationSize;
        private int individualSize;
        public float mutationRate = 0.1f;
        public float randomIndividualsRate = 0.1f;
        public float bestIndividualsRate = 0.1f;
        public float emptyRate = 0.93f;
        public float spacing = 5.0f;
        public List<int> shapes;
        Vector<float> externalEvaluations;
        Vector<float> tmpVelo;

        private GameObject[] dronePopulation;
        private MultiLayerMathsNet[] mlpPopulation;
        GameObject droneBest;

        private float nextUpdate;
        public float initialValueWeights;
        public float initialY = 3.0f;
        public float intervalUpdate = 4.0f;
        public bool randomRotation = false;
        public float scoreActiveWind = 120.0f;
        public float _windStrength = 0.0f;
        public float scoreActiveRandomRotation = 200.0f;
        public float _angleRandomRotation = 60.0f;
        float[] angleRandomRotationArr = new float[7] {2, 10, 20, 30, 40, 50, 60};
        int indexRRArr = 0;
        float medianLimit = 190.0f;

        public float AngleRandomRotation
        {
            get { return _angleRandomRotation; }
            set
            {
                _angleRandomRotation = value;
                this.distribution = new ContinuousUniform(-AngleRandomRotation, AngleRandomRotation, rndGenerator);
            }
        }

        public float WindStrength
        {
            get { return _windStrength; }
            set
            {
                _windStrength = value;
                GameObject.Find("WindArea").GetComponent<WindArea>().windStrength = _windStrength;
            }
        }


        

        float theoricBestScore;
        public HUDManager hudManager;
        public int _intervalSave;
        public int intervalSave { get { return _intervalSave; } set { _intervalSave = value; } }
        public int _loadGeneration;
        public int loadGeneration { get { return _loadGeneration; } set { _loadGeneration = value; } }

        public float _timeScale;
        public float timeScale { get { return _timeScale; } set { _timeScale = value; } }
        SystemRandomSource rndGenerator;
        ContinuousUniform distribution;
        ContinuousUniform deltaDistribution;


        List<Matrix<float>> tmpBuildCustomWeights;


        public object this[string propertyName]
        {
            get
            {
                Type myType = typeof(GeneSession);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(GeneSession);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                Debug.Log(value);
                myPropInfo.SetValue(this, value, null);

            }

        }


        void CheckNullOrReplace(string propertyName)
        {
            string valueCL = UIO.GetCommandLineArguments("-" + propertyName);

            if (valueCL != null)
            {
                if (this[propertyName] is int)
                {
                    this[propertyName] = int.Parse(valueCL);
                }
                else if (this[propertyName] is float)
                {
                    this[propertyName] = float.Parse(valueCL);
                }
                else
                {
                    this[propertyName] = valueCL;
                }

            }
        }


        void SetParametersFromCommandLine()
        {
            CheckNullOrReplace("task");
            CheckNullOrReplace("seed");
            CheckNullOrReplace("intervalSave");
            CheckNullOrReplace("loadGeneration");
            CheckNullOrReplace("timeScale");
        }


        void CloseApp()
        {
            if (Directory.Exists("Quit/"))
            {
                OnDestroy();
                Application.Quit();
            }
            
        }

        void OnDestroy()
        {
            Debug.Log("Destroy Genetic Writer");
            UIO.CloseStreamWriter(gene.writer);
            UIO.CloseStreamWriter(writer);
        }

        void BuildCustomWeights(Vector<float> individual)
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

        void DestroyAll()
        {
            for (int i = 0; i < populationSize; i++)
            {
                Destroy(dronePopulation[i]);
                mlpPopulation[i] = null;
            }
        }


        string GeneratePath(int seed = -1)
        {
            if(seed == -1)
            {
                return "Save/GeneSession/" + "Task-" + task + "/";
            }
            else
            {
                return "Save/GeneSession/" + "Task-" + task + "/Seed-" + seed + "/";
            }
        }



        void Awake()
        {
            /*timeScale = 2;
            loadGeneration = 0;
            intervalSave = 0;*/
            SetParametersFromCommandLine();

            if (task == "stabilization")
            {
                shapes = new List<int> {9, 4};
                Debug.Log("Stabilization Task with MLP [" + string.Join(" - ", shapes.Select(x => x.ToString()).ToArray()) +  "]");
            } else if(task == "move")
            {
                shapes = new List<int> { 12, 4 };
                Debug.Log("Move Task with MLP [" + string.Join(" - ", shapes.Select(x => x.ToString()).ToArray()) + "]");
            }
            else
            {
                throw new System.ArgumentException("Task is not valid !");
            }

            tmpBuildCustomWeights = new List<Matrix<float>>();
            hudManager = this.gameObject.AddComponent<HUDManager>() as HUDManager;

            hudManager.AddTextLayout("Task", task);
            hudManager.AddTextLayout("Best Drone Info", "");
            hudManager.AddTextLayout("Position", "--------");
            hudManager.AddTextLayout("Position with delta", "--------");
            hudManager.AddTextLayout("Commands", "--------");
            hudManager.AddTextLayout("Score Cond Active RR", scoreActiveRandomRotation.ToString());
            hudManager.AddTextLayout("Random rotation", "--------");
            hudManager.AddTextLayout("Wind", "--------");
            Time.timeScale = timeScale;
            hudManager.AddTextLayout("Time Scale", Time.timeScale.ToString() + "x");
            hudManager.AddTextLayout("Generation", "--------");


            nextUpdate = intervalUpdate;

            rndGenerator = new SystemRandomSource(seed);
            individualSize = 0;
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                individualSize += (shapes[i] + 1) * shapes[i + 1];
            }
            Debug.Log("Init Random Generator : " + rndGenerator);
            gene = new Genetic(seed, rndGenerator, populationSize, individualSize, initialValueWeights, mutationRate, randomIndividualsRate, bestIndividualsRate, emptyRate, GeneratePath());

            if(loadGeneration != 0)
            {
                float[,] floatArr = new float[individualSize, populationSize];
                gene.LoadGeneration(loadGeneration, floatArr);
                gene.generation = loadGeneration;
            }

            dronePopulation = new GameObject[populationSize];
            mlpPopulation = new MultiLayerMathsNet[populationSize];

            distribution = new ContinuousUniform(-AngleRandomRotation, AngleRandomRotation, rndGenerator);
            deltaDistribution = new ContinuousUniform(-2, 2, rndGenerator);


            hudManager.AddTextLayout("Infos", "Seed : " + seed + "\r\n" + 
                                     "Pop Size : " + populationSize + "\r\n" +
                                     "Ind Size : " + individualSize + "\r\n" +
                                     "Mrate : " + mutationRate + "\r\n" +
                                     "Rrate : " + randomIndividualsRate + "\r\n" +
                                     "Brate : " + bestIndividualsRate + "\r\n" +
                                     "ERate : " + emptyRate + " (" + Mathf.RoundToInt(emptyRate * individualSize) + ")\r\n" +
                                     "MLP : [" + string.Join("-", shapes.Select(x => x.ToString()).ToArray()) +  "] init " + initialValueWeights
                                     );

            ResetAll(rndGenerator, randomRotation);

            writer = UIO.CreateStreamWriter(GeneratePath(seed), "GeneSessionResults.csv", false);
            UIO.WriteLine(writer, "Generation;Angle Random Rotation;Wind");

            writerSession = UIO.CreateStreamWriter(GeneratePath(seed), "GeneSessionParams.csv", false);
            UIO.WriteLine(writerSession, "Task : " + task + ";" +
                                     "Seed : " + seed + ";" +
                                     "Population Size : " + populationSize + ";" +
                                     "Individual Size : " + individualSize + ";" +
                                     "Mutation Rate : " + mutationRate + ";" +
                                     "Random Rate : " + randomIndividualsRate + ";" +
                                     "Best Individual Rate : " + bestIndividualsRate + ";" +
                                     "Empty Coeff Individual Rate : " + emptyRate + " (" + Mathf.RoundToInt(emptyRate * individualSize)+ ");" +
                                     "MLP layers : [" + string.Join("-", shapes.Select(x => x.ToString()).ToArray()) + "] ; Intitial Value Weights :  [" + -initialValueWeights + "," + initialValueWeights + "]");
            UIO.CloseStreamWriter(writerSession);
        }
       

        void ResetAll(SystemRandomSource rndGenerator, bool randomRotation = false, bool firstReset = false)
        {
            for (int i = 0; i < populationSize; i++)
            {
                if (randomRotation)
                {
                    dronePopulation[i] = Instantiate(prefabDrone, new Vector3(i * spacing, initialY, 0.0f), Quaternion.identity) as GameObject;
                    dronePopulation[i].GetComponentInChildren<Rigidbody>().transform.eulerAngles =new Vector3(UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()));
                }
                else
                {
                    dronePopulation[i] = Instantiate(prefabDrone, new Vector3(i * spacing, initialY, 0.0f), Quaternion.identity) as GameObject;
                }

                //dronePopulation[i].GetComponent<MainBoard>().initPosition = new Vector3(i * spacing, initialY, 0.0f);
                if (task == "move")
                {
                    dronePopulation[i].GetComponent<MainBoard>().initDeltaPosition = new Vector3((float)deltaDistribution.Sample(), (float)deltaDistribution.Sample(), (float)deltaDistribution.Sample());
                }
                dronePopulation[i].name = "Drone " + i;

                //dronePopulation[i].GetComponent<MainBoard>().inputSize = shapes[0];
                mlpPopulation[i] = new MultiLayerMathsNet(seed, rndGenerator, shapes, 1, initialValueWeights);
                dronePopulation[i].GetComponent<MainBoard>().mlp = mlpPopulation[i];

                if (firstReset)
                {
                    mlpPopulation[i].Reset(firstReset, null);
                }
                else
                {
                    BuildCustomWeights(gene.GetIndividual(i));
                    mlpPopulation[i].Reset(firstReset, tmpBuildCustomWeights);
                }

            }
            externalEvaluations = Vector<float>.Build.Dense(populationSize);
            tmpVelo = Vector<float>.Build.Dense(populationSize);
        }


        float EvaluateIndividualStabilization(int i)
        {
            /*Debug.Log("Velo " + rigid.velocity.ToString());
            Debug.Log("Euler " + rigid.transform.eulerAngles.ToString());
            Debug.Log("Angular Velo " + rigid.angularVelocity);*/
            return 1 / (1 + Mathf.Abs(rigid.velocity.y) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.x)) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.y)) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.z)) + Mathf.Abs(rigid.angularVelocity.x) + Mathf.Abs(rigid.angularVelocity.z) + Mathf.Abs(rigid.angularVelocity.y));
        }

        float EvaluateIndividualMove(int i)
        {
            

            return 1 / (1 + Mathf.Abs(dronePopulation[i].GetComponent<MainBoard>().deltaPosition.x) + Mathf.Abs(dronePopulation[i].GetComponent<MainBoard>().deltaPosition.y) + Mathf.Abs(dronePopulation[i].GetComponent<MainBoard>().deltaPosition.z));
        }


        void ConfigureDrone(GameObject drone, Vector<float> input)
        {
            drone.GetComponent<InputControl>().SendSignalWithMLP(input);
        }


        void FixedUpdate()
        {
            //Debug.Log("FixedUpdate GeneSession");
            CloseApp();

            theoricBestScore += 1;
            for (int i = 0; i < populationSize; i++)
            {
                rigid = dronePopulation[i].GetComponentInChildren<Rigidbody>();
                if (task == "stabilization")
                {
                    ConfigureDrone(dronePopulation[i], Vector<float>.Build.DenseOfArray(new float[] { UAngle.SteerAngle(rigid.transform.eulerAngles.x), UAngle.SteerAngle(rigid.transform.eulerAngles.y), UAngle.SteerAngle(rigid.transform.eulerAngles.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z }));
                    tmpVelo[i] += EvaluateIndividualStabilization(i);
                } else if(task == "move")
                {
                    //Debug.Log(rigid.transform.localPosition.x - dronePopulation[i].GetComponent<MainBoard>().initDeltaPosition.x);
                    dronePopulation[i].GetComponent<MainBoard>().deltaPosition = new Vector3(rigid.transform.localPosition.x - dronePopulation[i].GetComponent<MainBoard>().initDeltaPosition.x, rigid.transform.localPosition.y - dronePopulation[i].GetComponent<MainBoard>().initDeltaPosition.y, rigid.transform.localPosition.z - dronePopulation[i].GetComponent<MainBoard>().initDeltaPosition.z);
                    ConfigureDrone(dronePopulation[i], Vector<float>.Build.DenseOfArray(new float[] { dronePopulation[i].GetComponent<MainBoard>().deltaPosition.x, dronePopulation[i].GetComponent<MainBoard>().deltaPosition.y, dronePopulation[i].GetComponent<MainBoard>().deltaPosition.z, UAngle.SteerAngle(rigid.transform.eulerAngles.x), UAngle.SteerAngle(rigid.transform.eulerAngles.y), UAngle.SteerAngle(rigid.transform.eulerAngles.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z }));
                    tmpVelo[i] += EvaluateIndividualMove(i);
                }

               

            }

            if (droneBest != null)
            {
                rigid = droneBest.GetComponentInChildren<Rigidbody>();
                hudManager.UpdateTextLayout("Position", droneBest.GetComponentInChildren<Rigidbody>().transform.position.ToString());
                if (task == "stabilization")
                {
                    ConfigureDrone(droneBest, Vector<float>.Build.DenseOfArray(new float[] { UAngle.SteerAngle(rigid.transform.eulerAngles.x), UAngle.SteerAngle(rigid.transform.eulerAngles.y), UAngle.SteerAngle(rigid.transform.eulerAngles.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z }));
                }
                else if (task == "move")
                {
                    droneBest.GetComponent<MainBoard>().deltaPosition = new Vector3(rigid.transform.localPosition.x - droneBest.GetComponent<MainBoard>().initDeltaPosition.x, rigid.transform.localPosition.y - droneBest.GetComponent<MainBoard>().initDeltaPosition.y, rigid.transform.localPosition.z - droneBest.GetComponent<MainBoard>().initDeltaPosition.z);
                    ConfigureDrone(droneBest, Vector<float>.Build.DenseOfArray(new float[] { droneBest.GetComponent<MainBoard>().deltaPosition.x, droneBest.GetComponent<MainBoard>().deltaPosition.y, droneBest.GetComponent<MainBoard>().deltaPosition.z, UAngle.SteerAngle(rigid.transform.eulerAngles.x), UAngle.SteerAngle(rigid.transform.eulerAngles.y), UAngle.SteerAngle(rigid.transform.eulerAngles.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z }));
                    hudManager.UpdateTextLayout("Position with delta", droneBest.GetComponent<MainBoard>().deltaPosition.ToString());
                }
                MultiLayerMathsNet mlp = droneBest.GetComponent<MainBoard>().mlp;
                hudManager.UpdateTextLayout("Commands", mlp.layers[mlp.shapesSize - 1].ToString(4 , 1,"G2"));
            }


            if (Time.time >= nextUpdate)
            {
                string generationInfos;
                Destroy(droneBest);

                nextUpdate = Mathf.FloorToInt(Time.time) + intervalUpdate;

                externalEvaluations = tmpVelo;

                generationInfos = gene.Run(externalEvaluations, theoricBestScore, intervalSave);

                if(gene.bestScore > scoreActiveRandomRotation && (gene.median) > medianLimit && indexRRArr < angleRandomRotationArr.Length)
                {
                    if(!randomRotation)
                        randomRotation = true;

                    if(medianLimit == 190.0f)
                    {
                        medianLimit = 150.0f;
                    }

                    AngleRandomRotation = angleRandomRotationArr[indexRRArr++];
                }


                hudManager.UpdateTextLayout("Generation", generationInfos);

                if (randomRotation)
                {
                    droneBest = Instantiate(prefabDrone, new Vector3(-spacing - 10.0f, initialY, 0.0f), Quaternion.identity) as GameObject;
                    Vector3 rndRotation = new Vector3(UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()));
                    droneBest.GetComponentInChildren<Rigidbody>().transform.eulerAngles = rndRotation;
                    hudManager.UpdateTextLayout("Random rotation", rndRotation.ToString());

                } else
                {
                    droneBest = Instantiate(prefabDrone, new Vector3(-spacing - 10.0f, initialY, 0.0f), Quaternion.identity) as GameObject;
                    //droneBest.GetComponent<MainBoard>().initPosition = new Vector3(-spacing - 10.0f, initialY, 0.0f);

                }

                if (task == "move")
                {
                    droneBest.GetComponent<MainBoard>().initDeltaPosition = new Vector3((float)deltaDistribution.Sample(), (float)deltaDistribution.Sample(), (float)deltaDistribution.Sample());
                }

                droneBest.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(seed, rndGenerator, shapes, 1, initialValueWeights);

                //droneBest.GetComponent<MainBoard>().inputSize = shapes[0];
                BuildCustomWeights(gene.GetBestIndividual());
                droneBest.GetComponent<MainBoard>().mlp.Reset(false, tmpBuildCustomWeights);

                foreach(Rigidbody rigid in droneBest.GetComponentsInChildren<Rigidbody>())
                {
                    if (rigid.CompareTag("DroneMotor"))
                    {
                        rigid.tag = "FrameBest";
                    }
                }

                DestroyAll();
                ResetAll(rndGenerator, randomRotation);

                theoricBestScore = 0;
                UIO.WriteLine(writer, gene.generation + ";" + AngleRandomRotation + ";" + WindStrength);


            }
        }
    }
}
