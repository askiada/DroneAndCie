using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using Lexmou.MachineLearning.Evolutionary;
using Lexmou.Utils;
public class GeneSession : MonoBehaviour {
 
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
    //bool nextGen = false;
    private GameObject[] dronePopulation;
    private MultiLayerMathsNet[] mlpPopulation;
    GameObject droneBest;

    private float nextUpdate;
    public float initialValueWeights;
    public float initialY = 3.0f;
    public float intervalUpdate = 4.0f;
    float theoricBestScore;
    HUDManager hudManager;

    public float timeScale = 1.0f;
    SystemRandomSource rndGenerator;

    /**
     * # Youhou je suis du markdown
     *
     * 
     */
    private static T[,] Make2DArray<T>(T[] input, int height, int width)
    {
        T[,] output = new T[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                output[i, j] = input[i * width + j];
            }
        }
        return output;
    }

    /*List<float[,]> BuildCustomWeights(Vector<float> individual)
    {
        List<float[,]> tmp = new List<float[,]>();
        for (int i=0; i < shapes.Count - 1; i++)
        {
            if(i == 0)
            {
                tmp.Add(Make2DArray(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]).AsArray(), shapes[i] + 1, shapes[i + 1]));
            }
            else
            {
                tmp.Add(Make2DArray(individual.SubVector((shapes[i-1] + 1) * shapes[i], shapes[i] * shapes[i + 1]).AsArray(), shapes[i], shapes[i + 1]));
            }

        }
        return tmp;
    }*/


    Matrix<float> Make2DMatrix(Vector<float> V, int height, int width)
    {
        Matrix<float> tmp = Matrix<float>.Build.Dense(height, width);
        for(int i =0; i < width; i++)
        {
            //Debug.Log(tmp + "  " + V.SubVector(height * i, height));
            tmp.SetColumn(i, V.SubVector(height * i, height));
        }
        return tmp;
    }


    List<Matrix<float>> BuildCustomWeights(Vector<float> individual)
    {
        List<Matrix<float>> tmp = new List<Matrix<float>>();
        for (int i = 0; i < shapes.Count - 1; i++)
        {
            /*if (i == 0)
            {
                tmp.Add(Make2DArray(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]).AsArray(), shapes[i] + 1, shapes[i + 1]));
            }
            else
            {
                tmp.Add(Make2DArray(individual.SubVector((shapes[i - 1] + 1) * shapes[i], shapes[i] * shapes[i + 1]).AsArray(), shapes[i], shapes[i + 1]));
            }*/
            if (i == 0)
            {
                //Debug.Log(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]));
                tmp.Add(Make2DMatrix(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]),  shapes[i + 1], shapes[i] + 1));
            }
            else
            {
                tmp.Add(Make2DMatrix(individual.SubVector((shapes[i - 1] + 1) * shapes[i], (shapes[i] + 1) * shapes[i + 1]), shapes[i + 1], shapes[i] + 1));
            }
            

        }
        return tmp;
    }

    // Use this for initialization


    void DestroyAll()
    {
        for (int i = 0; i < populationSize; i++)
        {
            Destroy(dronePopulation[i]);
            mlpPopulation[i] = null;
        }
    }


    void Awake () {
        hudManager = this.gameObject.AddComponent<HUDManager>() as HUDManager;
        hudManager.AddTextLayout("Best Drone Info", "Best Drone Info");
        hudManager.AddTextLayout("Position", "--------");
        hudManager.AddTextLayout("MLP Input", "--------");
        Time.timeScale = timeScale;
        nextUpdate = intervalUpdate;
        rndGenerator = new SystemRandomSource(seed);
        individualSize = 0;
        for(int i = 0; i< shapes.Count - 1; i++)
        {
            individualSize += (shapes[i]+1) * shapes[i + 1];
        }
        Debug.Log("Init Random Generator : " + rndGenerator);
        gene = new Genetic(seed, rndGenerator, populationSize, individualSize, initialValueWeights, mutationRate, randomIndividualsRate, bestIndividualsRate);

        
        //mlp = new MultiLayer(shape, seed, 1);


        dronePopulation = new GameObject[populationSize];
        mlpPopulation = new MultiLayerMathsNet[populationSize];

        ResetAll(rndGenerator);
    }

    void ResetAll(SystemRandomSource rndGenerator, bool firstReset = false)
    {
        for (int i = 0; i < populationSize; i++)
        {
            dronePopulation[i] = Instantiate(prefabDrone, new Vector3(i * spacing, initialY, 0.0f), Quaternion.identity) as GameObject;
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
            /*Debug.Log("dronePopulation[i].GetComponent<MainBoard>().mlp " + dronePopulation[i].GetComponent<MainBoard>().mlp.weights[0][0, 5]);
            Debug.Log("mlpPopulation[i] " + mlpPopulation[i].weights[0][0, 5]);*/
        }
        externalEvaluations = Vector<float>.Build.Dense(populationSize);
        tmpVelo = Vector<float>.Build.Dense(populationSize);
    }

    // Update is called once per frame

    /**
     * # Youhou je suis du markdown
     *
     */
    
    float EvaluateIndividual(int i)
    {

        /*mlpPopulation[i].layers[0].MapInplace(Mathf.Abs);
        float tmp =  mlpPopulation[i].layers[0].RowSums()[0];*/


        //Transform obj dronePopulation[i].GetComponent<MainBoard>.gameObject.transform.GetChild(0).GetChild(0);
        //Transform t = dronePopulation[i].GetComponent<MainBoard>().gameObject.transform.GetChild(0).GetChild(0);
        Rigidbody r = dronePopulation[i].GetComponentInChildren<Rigidbody>();
        //rotate = t.localEulerAngles;

        //float[,] final = new float[1, 6] { { obj.position.x, obj.position.y, obj.position.z, rotate.x, rotate.y, rotate.z } };
        //inputMLP = gyro.complete3(rotate);

        //inputMLP = new float[1, 6] { { obj.position.x, obj.position.y, obj.position.z, rotate.x, rotate.y, rotate.z } };

        //Debug.Log(r.position.y);
        float tmp2 = Mathf.Abs(r.velocity.y) + Mathf.Abs(Angle.SteerAngle(r.transform.eulerAngles.x)) + Mathf.Abs(Angle.SteerAngle(r.transform.eulerAngles.z)) + Mathf.Abs(Angle.SteerAngle(r.transform.eulerAngles.y)) + Mathf.Abs(r.angularVelocity.x) + Mathf.Abs(r.angularVelocity.z) + (r.angularVelocity.y);
        float tmp = 1/ (1 + tmp2);

        //float final = (tmp < 0) ? 0 : tmp;

        return tmp;
    }


    void FixedUpdate () {
        theoricBestScore += 1;
        for (int i = 0; i < populationSize; i++)
        {
            //Rigidbody r = dronePopulation[i].GetComponentInChildren<Rigidbody>();
            tmpVelo[i] += EvaluateIndividual(i);
        }
        //Debug.Log(tmpVelo);

        if(droneBest != null)
        {
            hudManager.UpdateTextLayout("Position", "Position : " + droneBest.GetComponentInChildren<Rigidbody>().transform.position.ToString());

            MultiLayerMathsNet mlp = droneBest.GetComponent<MainBoard>().mlp;
            //Debug.Log(mlp.shapesSize);
            hudManager.UpdateTextLayout("MLP Input", "MLP Input : " + mlp.layers[0] + " - " + mlp.layers[mlp.shapesSize - 1]);
        }

        if (Time.time >= nextUpdate)
        {
            string generationInfos;
            Destroy(droneBest);
            //Debug.Log(Time.time + ">=" + nextUpdate);
            // Change the next update (current second+1)
            nextUpdate = Mathf.FloorToInt(Time.time) + intervalUpdate;

            /*for (int i = 0; i < populationSize; i++)
            {
                externalEvaluations[i] = EvaluateIndividual(i);
            }*/
            externalEvaluations = tmpVelo ;

            generationInfos = gene.Run(externalEvaluations, theoricBestScore);
            Debug.Log(generationInfos);
            droneBest = Instantiate(prefabDrone,new Vector3(-spacing - 2.0f, initialY, 0.0f),Quaternion.identity) as GameObject;

            //droneBest.GetComponent<MainBoard>().gameObject.transform.GetChild(0).GetChild(0).position = new Vector3(-spacing - 2.0f, 0.0f, 0.0f);

            droneBest.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(seed, rndGenerator, shapes, 1, initialValueWeights);

            droneBest.GetComponent<MainBoard>().inputSize = shapes[0];

            droneBest.GetComponent<MainBoard>().mlp.Reset(false, BuildCustomWeights(gene.GetBestIndividual()));
            

            DestroyAll();

            //nextUpdate = Mathf.FloorToInt(Time.time) + intervalUpdate;

            ResetAll(rndGenerator);
            //externalEvaluations = Vector<float>.Build.Dense(populationSize);
            /*for (int i = 0; i < populationSize; i++)
            {
                mlpPopulation[i].Reset(0.0f, false, BuildCustomWeights(gene.GetIndividual(i)));
            }*/
            theoricBestScore = 0;

        }
	}
}
