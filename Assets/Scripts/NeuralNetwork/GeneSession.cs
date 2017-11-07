using UnityEngine;
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
namespace Lexmou.MachineLearning.Session.Quadcopter
{
    public class GeneSession : MonoBehaviour
    {
        public StreamWriter writer;
        public StreamWriter writerSession;
        public GameObject prefabDrone;
        private Genetic gene;
        public int seed;
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
        public float _windStrength = 0.3f;
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
        public int intervalSave = 10;

        public int loadGeneration = 0;

        public float timeScale = 1.0f;
        SystemRandomSource rndGenerator;
        ContinuousUniform distribution;


        List<Matrix<float>> tmpBuildCustomWeights;


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
                return "Save/GeneSession/";
            }
            else
            {
                return "Save/GeneSession/Seed-" + seed + "/";
            }
        }

        void Awake()
        {
            tmpBuildCustomWeights = new List<Matrix<float>>();
            hudManager = this.gameObject.AddComponent<HUDManager>() as HUDManager;
            
            hudManager.AddTextLayout("Best Drone Info", "Best Drone Info");
            hudManager.AddTextLayout("Position", "--------");
            hudManager.AddTextLayout("MLP Input", "--------");
            hudManager.AddTextLayout("Score Cond Active RR", "Score Cond Active RR " + scoreActiveRandomRotation.ToString());
            hudManager.AddTextLayout("Random rotation", "--------");
            hudManager.AddTextLayout("Wind", "--------");
            Time.timeScale = timeScale;
            hudManager.AddTextLayout("Time Scale", "Time Scale : " + Time.timeScale.ToString() + "x");
            hudManager.AddTextLayout("Generation : ", "--------");


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

            writer = UIO.CreateStreamWriter(GeneratePath(this.seed), "GeneSessionResults.csv", false);
            UIO.WriteLine(writer, "Generation;Angle Random Rotation;Wind");

            writerSession = UIO.CreateStreamWriter(GeneratePath(this.seed), "GeneSessionParams.csv", false);
            UIO.WriteLine(writerSession, "Seed : " + seed + ";" +
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
                
                dronePopulation[i].name = "Drone " + i;

                dronePopulation[i].GetComponent<MainBoard>().inputSize = shapes[0];
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


        float EvaluateIndividual(int i)
        {
            Rigidbody r = dronePopulation[i].GetComponentInChildren<Rigidbody>();

            float tmp2 = Mathf.Abs(r.velocity.y) + Mathf.Abs(UAngle.SteerAngle(r.transform.eulerAngles.x)) + Mathf.Abs(UAngle.SteerAngle(r.transform.eulerAngles.z)) + Mathf.Abs(UAngle.SteerAngle(r.transform.eulerAngles.y)) + Mathf.Abs(r.angularVelocity.x) + Mathf.Abs(r.angularVelocity.z) + Mathf.Abs(r.angularVelocity.y);
            float tmp = 1 / (1 + tmp2);

            return tmp;
        }


        void FixedUpdate()
        {
            CloseApp();

            theoricBestScore += 1;
            for (int i = 0; i < populationSize; i++)
            {
                tmpVelo[i] += EvaluateIndividual(i);
            }

            if (droneBest != null)
            {
                hudManager.UpdateTextLayout("Position", "Position : " + droneBest.GetComponentInChildren<Rigidbody>().transform.position.ToString());

                MultiLayerMathsNet mlp = droneBest.GetComponent<MainBoard>().mlp;
                hudManager.UpdateTextLayout("MLP Input", "MLP Output : " + mlp.layers[mlp.shapesSize - 1].ToString(4 , 1,"G2"));
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


                hudManager.UpdateTextLayout("Generation : ", generationInfos);
                if (randomRotation)
                {
                    droneBest = Instantiate(prefabDrone, new Vector3(-spacing - 10.0f, initialY, 0.0f), Quaternion.identity) as GameObject;
                    Vector3 rndRotation = new Vector3(UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()));
                    droneBest.GetComponentInChildren<Rigidbody>().transform.eulerAngles = rndRotation;
                    hudManager.UpdateTextLayout("Random rotation", "Random rotation : " + rndRotation.ToString());
                } else
                {
                    droneBest = Instantiate(prefabDrone, new Vector3(-spacing - 10.0f, initialY, 0.0f), Quaternion.identity) as GameObject;
                }
                

                droneBest.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(seed, rndGenerator, shapes, 1, initialValueWeights);

                droneBest.GetComponent<MainBoard>().inputSize = shapes[0];
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
