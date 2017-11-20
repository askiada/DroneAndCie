using UnityEngine;
using System.Collections;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using Lexmou.MachineLearning.Evolutionary;
using System.Collections.Generic;
using Lexmou.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

namespace Lexmou.Manager
{

    public class DroneManager : MonoBehaviour
    {

        public GameObject prefabDrone;
        public GameObject prefabDoor;
        public int stabilizationSeed;
        public int stabilizationGeneration;

        public int moveSeed;
        public int moveGeneration;

        Genetic gene;

        public GameObject drone;
        public GameObject door;
        public float initialY = 3.0f;
        List<Matrix<float>> tmpBuildCustomWeights;
        List<int> shapes = new List<int>() { 9, 4 };
        ContinuousUniform deltaDistribution;

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
            Destroy(door);
            door = Instantiate(prefabDoor, new Vector3(0.0f, initialY + 0.02f, -10.0f), Quaternion.identity) as GameObject;
            drone = Instantiate(prefabDrone, new Vector3(0.0f, initialY, -10.0f), Quaternion.identity) as GameObject;

            drone.GetComponent<InputControl>().manual = true;


            if (stabilizationGeneration != 0)
            {
                shapes = new List<int>() { 9, 4 };
                //drone.GetComponent<MainBoard>().inputSize = 9;
                float[] floatArr = new float[40];
                gene.LoadBest(stabilizationGeneration, floatArr);
                gene.generation = stabilizationGeneration;
                drone.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(stabilizationSeed, null, shapes, 1, 0);
                BuildCustomWeights(Vector<float>.Build.DenseOfArray(floatArr));
                drone.GetComponent<MainBoard>().mlp.Reset(false, tmpBuildCustomWeights);
            }

            if (moveGeneration != 0)
            {

                shapes = new List<int>() { 12, 4 };
                //drone.GetComponent<MainBoard>().inputSize = 12;
                float[] floatArr = new float[52];
                gene.LoadBest(moveGeneration, floatArr);
                gene.generation = moveGeneration;
                drone.GetComponent<MainBoard>().mlp = new MultiLayerMathsNet(moveSeed, null, shapes, 1, 0);
                BuildCustomWeights(Vector<float>.Build.DenseOfArray(floatArr));
                drone.GetComponent<MainBoard>().mlp.Reset(false, tmpBuildCustomWeights);
            }


            drone.name = "Drone";

            if (mass >= 0 && drag >= 0 && maxThrust >= 0)
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

        public void SetNewMovePosition()
        {
            drone.GetComponent<MainBoard>().initDeltaPosition = new Vector3((float)deltaDistribution.Sample(), (float)deltaDistribution.Sample(), (float)deltaDistribution.Sample());
            door.transform.position = new Vector3(door.transform.position.x + drone.GetComponent<MainBoard>().initDeltaPosition.x, door.transform.position.y + drone.GetComponent<MainBoard>().initDeltaPosition.y, door.transform.position.z + drone.GetComponent<MainBoard>().initDeltaPosition.z);
        }

        public void Move()
        {
            Rigidbody rigid = drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>();
            //Debug.Log(drone.GetComponent<MainBoard>().inputMLP);
            drone.GetComponent<MainBoard>().deltaPosition = new Vector3(rigid.transform.localPosition.x - drone.GetComponent<MainBoard>().initDeltaPosition.x, rigid.transform.localPosition.y - drone.GetComponent<MainBoard>().initDeltaPosition.y, rigid.transform.localPosition.z - drone.GetComponent<MainBoard>().initDeltaPosition.z);
            drone.GetComponent<MainBoard>().mlp.PropagateForward(drone.GetComponent<MainBoard>().inputMLP);
        }

        void Awake()
        {
            if (stabilizationGeneration != 0)
                gene = new Genetic(stabilizationSeed, null, 100, 40, 1.0f, 0.1f, 0.1f, 0.1f, 0.1f, "Save/GeneSession/Task-stabilization/", false);
            if (moveGeneration != 0)
                gene = new Genetic(stabilizationSeed, null, 100, 40, 1.0f, 0.1f, 0.1f, 0.1f, 0.1f, "Save/GeneSession/Task-move/", false);
            deltaDistribution = new ContinuousUniform(-2, 2);
            tmpBuildCustomWeights = new List<Matrix<float>>();

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

            foreach (GameObject motor in motors)
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
}
