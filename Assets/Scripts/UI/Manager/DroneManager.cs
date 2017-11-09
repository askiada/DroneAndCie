using UnityEngine;
using System.Collections;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using Lexmou.MachineLearning.Evolutionary;
using System.Collections.Generic;
using Lexmou.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


public class DroneManager : MonoBehaviour {

    public GameObject prefabDrone;

    public int stabilizationSeed;
    public int stabilizationGeneration;
    Genetic gene;

    public GameObject drone;
    public float initialY = 3.0f;
    List<Matrix<float>> tmpBuildCustomWeights;
    List<int> shapes = new List<int>() { 9, 4 };

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


    public void Restart(float mass = -1.0f, float drag = -1.0f, float maxThrust = 1.0f)
    {
        drone = Instantiate(prefabDrone, new Vector3(0.0f, initialY, -10.0f), Quaternion.identity) as GameObject;
        drone.GetComponent<MainBoard>().inputSize = 9;
        drone.GetComponent<InputControl>().manual = true;
        

        if (stabilizationGeneration != 0)
        {
            float[] floatArr = new float[40];
            gene.LoadBest(stabilizationGeneration, floatArr);
            gene.generation = stabilizationGeneration;
            drone.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(stabilizationSeed, null, shapes, 1, 0);
            BuildCustomWeights(Vector<float>.Build.DenseOfArray(floatArr));
            drone.GetComponent<MainBoard>().mlp.Reset(false, tmpBuildCustomWeights);
        }

        
        drone.name = "Drone";

        if(mass >= 0 && drag >= 0 && maxThrust >= 0)
        {
            Debug.Log(mass + " " + drag + " " + maxThrust);
            /*drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>().mass = mass;
            drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>().drag = mass;
            */
            SetMass(mass);
            SetDrag(drag);
            setMaxThrust(maxThrust);
        }
    }

    public void Stabilization()
    {
        //Debug.Log(drone.GetComponent<MainBoard>().inputMLP);
        drone.GetComponent<MainBoard>().mlp.PropagateForward(drone.GetComponent<MainBoard>().inputMLP);
    }

    void Awake()
    {
        tmpBuildCustomWeights = new List<Matrix<float>>();
        gene = new Genetic(stabilizationSeed, null, 100, 40, 1.0f, 0.1f, 0.1f, 0.1f, 0.1f, "Save/GeneSession/",false);
        Restart();
    }

    public void SetMass(float val)
    {
        GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().mass = val;
    }

    public void SetDrag(float val)
    {
        GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().drag = val;
    }

    public void setMaxThrust(float val)
    {
        GameObject[] motors = GameObject.FindGameObjectsWithTag("DroneMotor");

        foreach(GameObject motor in motors)
        {
            //Debug.Log("Motor " + motor.name);
            motor.GetComponent<MotorThrust>().MaxThrust = val;
        }
    }


    public void restartPositionRotation()
    {
        float droneMass = GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().mass;
        float droneDrag = GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().drag;
        float droneMaxThrust = GameObject.FindGameObjectsWithTag("DroneMotor")[0].GetComponent<MotorThrust>().MaxThrust;
        
        Destroy(drone);
        Restart(droneMass, droneDrag, droneMaxThrust);
    }

    /*void LateUpdate()
    {
        GameObject frame = GameObject.FindGameObjectsWithTag("Frame")[0];
        Rigidbody rigid = frame.GetComponent<Rigidbody>();
        rigid.isKinematic = false;
    }*/





}
