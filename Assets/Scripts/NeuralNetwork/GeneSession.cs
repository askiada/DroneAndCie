using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class GeneSession : MonoBehaviour {


    public GameObject prefabDrone;
    private Gene gene;
    public int seed;
    public int populationSize;
    public int individualSize = 60;
    public float mutationRate = 0.1f;
    public float randomIndividualsRate = 0.1f;
    public float bestIndividualsRate = 0.1f;
    public float spacing = 2.0f;
    List<int> shapes;
    //bool nextGen = false;
    private GameObject[] dronePopulation;
    private MultiLayer[] mlpPopulation;

    private int nextUpdate;

    private int intervalUpdate = 4;

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

    List<float[,]> BuildCustomWeights(Vector<float> individual)
    {
        List<float[,]> tmp = new List<float[,]>();
        for (int i=0; i < shapes.Count - 1; i++)
        {
            if(i == 0)
            {
                /*Debug.Log((shapes[i] + 1) * shapes[i + 1]);
                Debug.Log("Weights " + i + " - " + individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]));
                Debug.Log("Weights Array " + i + " - " + Make2DArray(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]).AsArray(), shapes[i] + 1, shapes[i + 1]));*/
                tmp.Add(Make2DArray(individual.SubVector(0, (shapes[i] + 1) * shapes[i + 1]).AsArray(), shapes[i] + 1, shapes[i + 1]));
            }
            else
            {
                /*Debug.Log((shapes[i - 1] + 1) * shapes[i] + "    " + shapes[i] * shapes[i + 1]);
                Debug.Log("Weights " + i + " - " + individual.SubVector((shapes[i - 1] + 1) * shapes[i], shapes[i] * shapes[i + 1]));
                Debug.Log("Weights Array " + i + " - " + Make2DArray(individual.SubVector((shapes[i - 1] + 1) * shapes[i], shapes[i] * shapes[i + 1]).AsArray(), shapes[i] , shapes[i + 1]));*/
                tmp.Add(Make2DArray(individual.SubVector((shapes[i-1] + 1) * shapes[i], shapes[i] * shapes[i + 1]).AsArray(), shapes[i], shapes[i + 1]));
            }

        }
        return tmp;
    }

    // Use this for initialization
    void ResetAll()
    {
        for (int i = 0; i < populationSize; i++)
        {
            dronePopulation[i] = Instantiate(prefabDrone, new Vector3(i * spacing, 0.1f, 0.0f), Quaternion.identity) as GameObject;

            mlpPopulation[i] = new MultiLayer(shapes, seed, 1);
            dronePopulation[i].GetComponent<MainBoard>().mlp = mlpPopulation[i];
            mlpPopulation[i].Reset(0.0f, true, BuildCustomWeights(gene.GetIndividual(i)));
            /*Debug.Log("dronePopulation[i].GetComponent<MainBoard>().mlp " + dronePopulation[i].GetComponent<MainBoard>().mlp.weights[0][0, 5]);
            Debug.Log("mlpPopulation[i] " + mlpPopulation[i].weights[0][0, 5]);*/
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


    void Awake () {
        nextUpdate = intervalUpdate;
        gene = new Gene(seed, populationSize, individualSize, mutationRate, randomIndividualsRate, bestIndividualsRate);

        shapes = new List<int> {6,5,4};
        //mlp = new MultiLayer(shape, seed, 1);


        dronePopulation = new GameObject[populationSize];
        mlpPopulation = new MultiLayer[populationSize];

        ResetAll();
    }
	
	// Update is called once per frame

    float EvaluateIndividual(int i)
    {

        

        //Transform obj dronePopulation[i].GetComponent<MainBoard>.gameObject.transform.GetChild(0).GetChild(0);
        Transform t = dronePopulation[i].GetComponent<MainBoard>().gameObject.transform.GetChild(0).GetChild(0);
        //rotate = t.localEulerAngles;

        /*float[,] final = new float[1, 6] { { obj.position.x, obj.position.y, obj.position.z, rotate.x, rotate.y, rotate.z } };
        //inputMLP = gyro.complete3(rotate);

        inputMLP = new float[1, 6] { { obj.position.x, obj.position.y, obj.position.z, rotate.x, rotate.y, rotate.z } };*/

        float final = t.position.z;

        return final;
    }

	void Update () {

        if (Time.time >= nextUpdate)
        {
            //Debug.Log(Time.time + ">=" + nextUpdate);
            // Change the next update (current second+1)
            nextUpdate = Mathf.FloorToInt(Time.time) + intervalUpdate;

            Vector<float> externalEvaluations = Vector<float>.Build.Dense(populationSize);
            for(int i=0; i < populationSize; i++)
            {
                externalEvaluations[i] = EvaluateIndividual(i);
            }
            gene.Run(externalEvaluations);

            DestroyAll();
            ResetAll();
            for (int i = 0; i < populationSize; i++)
            {
                mlpPopulation[i].Reset(0.0f, true, BuildCustomWeights(gene.GetIndividual(i)));
            }
        }
	}
}
