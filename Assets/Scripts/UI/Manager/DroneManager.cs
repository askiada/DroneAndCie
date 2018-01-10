using UnityEngine;
using System;
using System.Collections;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using Lexmou.MachineLearning.Evolutionary;
using System.Collections.Generic;
using Lexmou.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using Lexmou.MachineLearning;
using Drone.Hardware;

namespace Lexmou.Manager
{

    public class DroneManager : MonoBehaviour
    {
        public SystemRandomSource rndGenerator;
        public string task = "Stabilization";
        DroneTask taskObject;
        public GameObject prefabDrone;
        public GameObject prefabDoor;
        public int fromSeed;
        public int fromGeneration;

        MultiLayerMathsNet mlp;
        //Genetic gene;

        public GameObject drone;
        public GameObject door;
        public float initialY = 3.0f;
        List<Matrix<float>> tmpBuildCustomWeights;
        List<int> shapes = new List<int>() { 9, 4 };
        ContinuousUniform deltaDistribution;

        string path;
        Vector3 targetPosition;
        private ControlSignal signalControl;

        /*RaycastHit hit;
        Ray downRay;*/

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

            //downRay = new Ray(drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>().transform.position, -Vector3.up);

            drone.GetComponent<InputControl>().manual = true;

            targetPosition = door.transform.position;


            drone.name = "Drone";

            if (mass >= 0 && drag >= 0 && maxThrust >= 0)
            {
                Debug.Log(mass + " " + drag + " " + maxThrust);
                SetMass(mass);
                SetDrag(drag);
                setMaxThrust(maxThrust);
            }
        }

        public void Stabilization()
        {
            Rigidbody rigid = drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>();
            //Debug.Log(drone.GetComponent<MainBoard>().inputMLP);
            /*drone.GetComponent<MainBoard>().deltaPosition = new Vector3(rigid.transform.localPosition.x - drone.GetComponent<MainBoard>().initDeltaPosition.x, rigid.transform.localPosition.y - drone.GetComponent<MainBoard>().initDeltaPosition.y, rigid.transform.localPosition.z - drone.GetComponent<MainBoard>().initDeltaPosition.z);
            drone.GetComponent<MainBoard>().mlp.PropagateForward(drone.GetComponent<MainBoard>().inputMLP);*/
            taskObject.UCSignal(rigid, targetPosition);
            //Debug.Log(taskObject.signal.input);
            mlp.PropagateForward(taskObject.signal.input, true);
            //Debug.Log(mlp.layers[taskObject.shapes.Count - 1]);
            signalControl.Throttle = mlp.layers[taskObject.shapes.Count - 1][3, 0];
            signalControl.Rudder = mlp.layers[taskObject.shapes.Count - 1][0, 0];
            signalControl.Elevator = mlp.layers[taskObject.shapes.Count - 1][1, 0];
            signalControl.Aileron = mlp.layers[taskObject.shapes.Count - 1][2, 0];
            Debug.Log(mlp.weights[0]);
            drone.GetComponent<MainBoard>().SendControlSignal(signalControl);
        }

        public void SetNewMovePosition()
        {
            Rigidbody rigid = drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>();
            /*Debug.Log(rigid.centerOfMass);
            rigid.centerOfMass = new Vector3(0.5f, 0f, 0f);*/
            //Debug.Log("Init target pos : " + rigid.transform.localPosition);
            //Debug.Log("Rigid position : " + rigid.transform.position);
            targetPosition = taskObject.GetTargetPosition(rigid.transform.position);
            //drone.GetComponent<MainBoard>().initDeltaPosition = new Vector3((float)deltaDistribution.Sample(), (float)deltaDistribution.Sample(), (float)deltaDistribution.Sample());
            //Debug.Log("drone Pos : " + targetPosition);
            //Debug.Log("drone local Pos" + drone.transform.localPosition);
            door.transform.position = targetPosition;
        }

        public void Move()
        {
            Rigidbody rigid = drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>();
            /*if (Physics.Raycast(downRay, out hit))
            {
                //float hoverError = hoverHeight - hit.distance;
                Debug.Log(hit.distance);
            }*/
            //Debug.Log(drone.GetComponent<MainBoard>().inputMLP);
            /*drone.GetComponent<MainBoard>().deltaPosition = new Vector3(rigid.transform.localPosition.x - drone.GetComponent<MainBoard>().initDeltaPosition.x, rigid.transform.localPosition.y - drone.GetComponent<MainBoard>().initDeltaPosition.y, rigid.transform.localPosition.z - drone.GetComponent<MainBoard>().initDeltaPosition.z);
            drone.GetComponent<MainBoard>().mlp.PropagateForward(drone.GetComponent<MainBoard>().inputMLP);*/
            taskObject.UCSignal(rigid, targetPosition);
            //Debug.Log(taskObject.signal.input);
            mlp.PropagateForward(taskObject.signal.input, true);
            signalControl.Throttle = mlp.layers[taskObject.shapes.Count - 1][3, 0];
            signalControl.Rudder = mlp.layers[taskObject.shapes.Count - 1][0, 0];
            signalControl.Elevator = mlp.layers[taskObject.shapes.Count - 1][1, 0];
            signalControl.Aileron = mlp.layers[taskObject.shapes.Count - 1][2, 0];

            drone.GetComponent<MainBoard>().SendControlSignal(signalControl);
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

        void Awake()
        {
            signalControl = new ControlSignal();
            path = "Save/DroneSession/" + "Task-" + task + "/Seed-" + fromSeed + "/";
            rndGenerator = new SystemRandomSource(fromSeed);
            taskObject = (DroneTask)Activator.CreateInstance(Type.GetType("Lexmou.MachineLearning.Drone" + task), rndGenerator, task);

            mlp = new MultiLayerMathsNet(fromSeed, null, taskObject.shapes, 1, 0);
            float[] floatArr = new float[taskObject.individualSize];

           //float[] floatArr = new float[] {0,-0.33f,0,0,-0.33f,0,0,0,0,0,0.5f,0,0,-1,0,0,-1,0,0,0.5f,0,1,0,0,0,0,0,0,0,0,0,-1,0,0,0,0,0,0,0,0.5f};

            Genetic.LoadBest(path, fromGeneration, floatArr);
            BuildCustomWeights(mlp.weights, taskObject.shapes, Vector<float>.Build.DenseOfArray(floatArr));
            //Debug.Log(mlp.weights[0]);

            /*if (stabilizationGeneration != 0)
            {
                Debug.Log("Gene Move");
                gene = new Genetic(stabilizationSeed, null, 100, 40, 1.0f, 0.1f, 0.1f, 0.1f, 0.1f, "Save/DroneSession/Task-stabilization/", false);
            }
            if (moveGeneration != 0)
            {
                Debug.Log("Gene Move");
                gene = new Genetic(moveSeed, null, 100, 52, 1.0f, 0.1f, 0.1f, 0.1f, 0.1f, "Save/DroneSession/Task-move/", false);
            }
            deltaDistribution = new ContinuousUniform(-2, 2);
            tmpBuildCustomWeights = new List<Matrix<float>>();*/

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
