using UnityEngine;
using System.Collections;
using Drone.Hardware;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Lexmou.Utils;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using Lexmou.MachineLearning.DroneSession2;

public class MainBoard : MonoBehaviour
{
    public MonoBehaviour[] ThrustSignalBus;
    public MonoBehaviour[] ControlSignalBus;
    public MonoBehaviour[] SensorSignalBus;
    public MonoBehaviour[] UCSignalBus;
    public MultiLayerMathsNet mlp;
    //public int inputSize;
    //public Gyro gyro;
    public Vector<float> _inputMLP;
    public Vector<float>  inputMLP {
        get { return _inputMLP; }
        set
        {
            //Debug.Log("SetInputMLP and propagate");
            _inputMLP = value;
            mlp.PropagateForward(_inputMLP);
        }
    }
    public Vector3 rotate;
    //public Vector3 initPosition;
    public Vector3 deltaPosition;
    public Vector3 initDeltaPosition;



    /*void Start()
    {
        Debug.Log(inputSize);
        inputMLP = Vector<float>.Build.Dense(inputSize);
    }*/


    public void SendThrustSignal(ThrustSignal signal)
    {
        //Debug.Log("SendThrustSignal");
        ThrustSignal result = signal;
        foreach (Drone.Hardware.Component<ThrustSignal> component in ThrustSignalBus)
        {
            result = component.ProcessSignal(result);
        }
    }

    public void SendControlSignal(ControlSignal signal)
    {
        //Debug.Log("SendControlSignal");
        ControlSignal result = signal;
        foreach (Drone.Hardware.Component<ControlSignal> component in ControlSignalBus)
        {
            result = component.ProcessSignal(result);
        }
    }

    public void SendSensorSignal()
    {
        /*foreach (Drone.Hardware.Component<SensorSignal> component in SensorSignalBus)
        {
           
            SensorSignal result = component.ProcessSignal(GetComponent<Rigidbody>());
            foreach (Drone.Hardware.Component<UCSignal> ucComponent in UCSignalBus)
            {
                ControlSignal controlSignal = ucComponent.ProcessSignal(result);
                SendControlSignal(controlSignal);
            }
        }*/
    }

    public void SendUCSignal(UCSignal signal)
    {
        UCSignal result = signal;
        foreach (Drone.Hardware.Component<UCSignal> component in UCSignalBus)
        {
            result = component.ProcessSignal(result);
        }
    }
}

