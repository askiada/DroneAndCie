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
namespace Lexmou.MachineLearning.Session.Quadcopter
{
    public class GeneSession : MonoBehaviour
    {

        public GameObject prefabDrone;
        private Genetic gene;
        public int seed;
        public int populationSize;
        private int individualSize;
        public float mutationRate = 0.1f;
        public float randomIndividualsRate = 0.1f;
        public float bestIndividualsRate = 0.1f;
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
        public float scoreActiveRandomRotation = 200.0f;
        public float angleRandomRotation = 60.0f;
        float theoricBestScore;
        public HUDManager hudManager;

        public float timeScale = 1.0f;
        SystemRandomSource rndGenerator;
        ContinuousUniform distribution;



        List<Matrix<float>> BuildCustomWeights(Vector<float> individual)
        {
            List<Matrix<float>> tmp = new List<Matrix<float>>();
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                if (i == 0)
                {
                    tmp.Add(UMatrix.Make2DMatrix(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]), shapes[i + 1], shapes[i] + 1));
                }
                else
                {
                    tmp.Add(UMatrix.Make2DMatrix(individual.SubVector((shapes[i - 1] + 1) * shapes[i], (shapes[i] + 1) * shapes[i + 1]), shapes[i + 1], shapes[i] + 1));
                }
            }
            return tmp;
        }

        void DestroyAll()
        {
            for (int i = 0; i < populationSize; i++)
            {
                Destroy(dronePopulation[i]);
                mlpPopulation[i] = null;
            }
        }


        void Awake()
        {
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
            gene = new Genetic(seed, rndGenerator, populationSize, individualSize, initialValueWeights, mutationRate, randomIndividualsRate, bestIndividualsRate);

            dronePopulation = new GameObject[populationSize];
            mlpPopulation = new MultiLayerMathsNet[populationSize];

            distribution = new ContinuousUniform(-angleRandomRotation, angleRandomRotation, rndGenerator);

            hudManager.AddTextLayout("Infos", "Seed : " + seed + "\r\n" + 
                                     "Pop Size : " + populationSize + "\r\n" +
                                     "Ind Size : " + individualSize + "\r\n" +
                                     "Mrate : " + mutationRate + "\r\n" +
                                     "Rrate : " + randomIndividualsRate + "\r\n" +
                                     "Brate : " + bestIndividualsRate + "\r\n" +
                                     "MLP : [" + string.Join("-", shapes.Select(x => x.ToString()).ToArray()) +  "] init " + initialValueWeights
                                     );

            ResetAll(rndGenerator, randomRotation);
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
                //Debug.Log(shapes[0]);
                dronePopulation[i].GetComponent<MainBoard>().inputSize = shapes[0];
                mlpPopulation[i] = new MultiLayerMathsNet(seed, rndGenerator, shapes, 1, initialValueWeights);
                dronePopulation[i].GetComponent<MainBoard>().mlp = mlpPopulation[i];

                if (firstReset)
                {
                    mlpPopulation[i].Reset(firstReset, null);
                }
                else
                {
                    mlpPopulation[i].Reset(firstReset, BuildCustomWeights(gene.GetIndividual(i)));
                }

            }
            externalEvaluations = Vector<float>.Build.Dense(populationSize);
            tmpVelo = Vector<float>.Build.Dense(populationSize);
        }


        float EvaluateIndividual(int i)
        {
            Rigidbody r = dronePopulation[i].GetComponentInChildren<Rigidbody>();

            float tmp2 = Mathf.Abs(r.velocity.y) + Mathf.Abs(UAngle.SteerAngle(r.transform.eulerAngles.x)) + Mathf.Abs(UAngle.SteerAngle(r.transform.eulerAngles.z)) + Mathf.Abs(UAngle.SteerAngle(r.transform.eulerAngles.y)) + Mathf.Abs(r.angularVelocity.x) + Mathf.Abs(r.angularVelocity.z) + (r.angularVelocity.y);
            float tmp = 1 / (1 + tmp2);

            return tmp;
        }


        void FixedUpdate()
        {
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

                generationInfos = gene.Run(externalEvaluations, theoricBestScore);

                if(gene.bestScore > scoreActiveRandomRotation)
                {
                    randomRotation = true;
                }
                //Debug.Log(generationInfos);

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
                    //droneBest.GetComponentInChildren<Rigidbody>().transform.eulerAngles = new Vector3(UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()));
                }
                

                droneBest.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(seed, rndGenerator, shapes, 1, initialValueWeights);

                droneBest.GetComponent<MainBoard>().inputSize = shapes[0];

                droneBest.GetComponent<MainBoard>().mlp.Reset(false, BuildCustomWeights(gene.GetBestIndividual()));

                //droneBest.FindGa
                //droneBest.GetComponentInChildren<Rigidbody>().tag = "FrameBest";
                foreach(Rigidbody rigid in droneBest.GetComponentsInChildren<Rigidbody>())
                {
                    if (rigid.CompareTag("DroneMotor"))
                    {
                        rigid.tag = "FrameBest";
                    }
                }
                //Debug.Log(droneBest.GetComponentInChildren<Rigidbody>().tag);

                DestroyAll();
                ResetAll(rndGenerator, randomRotation);

                theoricBestScore = 0;

            }
        }
    }
}
