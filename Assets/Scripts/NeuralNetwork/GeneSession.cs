using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class GeneSession : MonoBehaviour {


    public GameObject prefabDrone;
    private Gene gene;
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
    private float intervalUpdate = 4.0f;
    SystemRandomSource rndGenerator;

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
        for(int i =0; i < width - 1; i++)
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
                tmp.Add(Make2DMatrix(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]),  shapes[i + 1], shapes[i] + 1));
            }
            else
            {
                tmp.Add(Make2DMatrix(individual.SubVector((shapes[i - 1] + 1) * shapes[i], shapes[i] * shapes[i + 1]), shapes[i + 1], shapes[i]));
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
        nextUpdate = intervalUpdate;
        rndGenerator = new SystemRandomSource(seed);
        individualSize = (shapes[0] + 1) * shapes[1];
        for(int i = 1; i< shapes.Count - 1; i++)
        {
            individualSize += shapes[i] * shapes[i + 1];
        }
        //Debug.Log(individualSize);
        gene = new Gene(seed, rndGenerator, populationSize, individualSize, mutationRate, randomIndividualsRate, bestIndividualsRate);

        
        //mlp = new MultiLayer(shape, seed, 1);


        dronePopulation = new GameObject[populationSize];
        mlpPopulation = new MultiLayerMathsNet[populationSize];

        ResetAll(rndGenerator);
    }

    void ResetAll(SystemRandomSource rndGenerator, bool firstReset = false)
    {
        for (int i = 0; i < populationSize; i++)
        {
            dronePopulation[i] = Instantiate(prefabDrone, new Vector3(i * spacing, 0.1f, 0.0f), Quaternion.identity) as GameObject;
            dronePopulation[i].name = "Drone " + i;
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

    float EvaluateIndividual(int i)
    {

        

        //Transform obj dronePopulation[i].GetComponent<MainBoard>.gameObject.transform.GetChild(0).GetChild(0);
        Transform t = dronePopulation[i].GetComponent<MainBoard>().gameObject.transform.GetChild(0).GetChild(0);
        Rigidbody r = dronePopulation[i].GetComponentInChildren<Rigidbody>();
        //rotate = t.localEulerAngles;

        /*float[,] final = new float[1, 6] { { obj.position.x, obj.position.y, obj.position.z, rotate.x, rotate.y, rotate.z } };
        //inputMLP = gyro.complete3(rotate);

        inputMLP = new float[1, 6] { { obj.position.x, obj.position.y, obj.position.z, rotate.x, rotate.y, rotate.z } };*/

        float tmp = (t.position.z*t.position.z) - (r.transform.eulerAngles.x * r.transform.eulerAngles.x) / 129600 - (r.transform.eulerAngles.z * r.transform.eulerAngles.z) / 129600 - (r.transform.eulerAngles.y * r.transform.eulerAngles.y) / 129600;

        //float final = (tmp < 0) ? 0 : tmp;

        return tmp;
    }

	void Update () {
        /*for (int i = 0; i < populationSize; i++)
        {
            Rigidbody r = dronePopulation[i].GetComponentInChildren<Rigidbody>();
            tmpVelo[i] += r.velocity.x + r.velocity.x + r.velocity.z ;
        }*/
        if (Time.time >= nextUpdate)
        {
            Destroy(droneBest);
            //Debug.Log(Time.time + ">=" + nextUpdate);
            // Change the next update (current second+1)
            nextUpdate = Mathf.FloorToInt(Time.time) + intervalUpdate;

            for (int i = 0; i < populationSize; i++)
            {
                externalEvaluations[i] = EvaluateIndividual(i);
            }
            //externalEvaluations = 100000 - tmpVelo * tmpVelo;

            gene.Run(externalEvaluations);
            droneBest = Instantiate(prefabDrone,new Vector3(-spacing - 2.0f, 0.0f, 0.0f),Quaternion.identity) as GameObject;

            //droneBest.GetComponent<MainBoard>().gameObject.transform.GetChild(0).GetChild(0).position = new Vector3(-spacing - 2.0f, 0.0f, 0.0f);

            droneBest.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(seed, rndGenerator, shapes, 1, initialValueWeights);


            droneBest.GetComponent<MainBoard>().mlp.Reset(false, BuildCustomWeights(gene.GetBestIndividual()));
            
            DestroyAll();

            //nextUpdate = Mathf.FloorToInt(Time.time) + intervalUpdate;

            ResetAll(rndGenerator);
            //externalEvaluations = Vector<float>.Build.Dense(populationSize);
            /*for (int i = 0; i < populationSize; i++)
            {
                mlpPopulation[i].Reset(0.0f, false, BuildCustomWeights(gene.GetIndividual(i)));
            }*/
            
        }
	}
}
