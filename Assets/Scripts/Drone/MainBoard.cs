using UnityEngine;
using System.Collections;
using Drone.Hardware;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Lexmou.Utils;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;


public class MainBoard : MonoBehaviour
{

    public MonoBehaviour[] ThrustSignalBus;
    public MonoBehaviour[] ControlSignalBus;
    public MultiLayerMathsNet mlp;
    public int inputSize;
    //public Gyro gyro;
    public Vector<float> inputMLP;
    public Vector3 rotate;
    public Vector3 position;

    void Start()
    {
        //Debug.Log("InputSize " + name + inputSize);
        inputMLP = Vector<float>.Build.Dense(inputSize);
        //initMLP();
        //gyro = new Gyro(this);

    }

    //Wrap between 0 and 360 degrees
    float WrapAngle(float inputAngle)
    {
        //The inner % 360 restricts everything to +/- 360
        //+360 moves negative values to the positive range, and positive ones to > 360
        //the final % 360 caps everything to 0...360
        return (((inputAngle % 360f) + 360f) % 360f)/360;
    }



    public void SendThrustSignal(ThrustSignal signal)
    {
        ThrustSignal result = signal;
        foreach (Drone.Hardware.Component<ThrustSignal> component in ThrustSignalBus)
        {
            //Debug.Log("Signal : " + component.GetComponent<MotorsController>().MotorFR.transform.eulerAngles.ToString());
            //rotate = component.GetComponent<MotorsController>().MotorFR.transform.eulerAngles;
            Transform obj = this.gameObject.transform.GetChild(0).GetChild(0);
            rotate = obj.eulerAngles;
            Rigidbody rigid = this.gameObject.GetComponentInChildren<Rigidbody>();
            //Debug.Log(angularVelocity.x + "  " + angularVelocity.y + "  " + angularVelocity.z);
            //inputMLP = gyro.complete3(rotate);
            inputMLP = Vector<float>.Build.DenseOfArray(new float[] { UAngle.SteerAngle(rotate.x), UAngle.SteerAngle(rotate.y), UAngle.SteerAngle(rotate.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z});
            //Debug.Log(name + " " + inputMLP);
            result = component.ProcessSignal(result);

        }
    }

    public void SendControlSignal(ControlSignal signal)
    {
        ControlSignal result = signal;
        foreach (Drone.Hardware.Component<ControlSignal> component in ControlSignalBus)
        {
            result = component.ProcessSignal(result);
        }
    }



    /*public void initMLP()
    {
        List<int> shapes = new List<int>() { 6,5, 4, 6 };

        mlp = new MultiLayer(shapes, 35165, 0, rndGenerator, true);
        mlp.Reset(0.01f, true);

        float[] data = new float[10] { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f };
        mlp.PropagateForward(data);
    }*/
}

