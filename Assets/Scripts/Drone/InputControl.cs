using UnityEngine;
using System.Collections;
using Drone.Hardware;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Lexmou.Utils;
public class InputControl : MonoBehaviour
{

    public MainBoard MainBoard;

    public bool manual = false;

    public bool constant = false;
    public bool stabilization = false;
    public float constantThrottle = 0.0f;
    public float constantRudder = 0.0f;
    public float constantElevator = 0.0f;
    public float constantAileron = 0.0f;


    public void SendSignalWithMLP()
    {
        ControlSignal signal = new ControlSignal();
        signal.Throttle = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1]
        [3, 0];
        signal.Rudder = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1]
        [0, 0];
        signal.Elevator = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1]
        [1, 0];
        signal.Aileron = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1]
        [2, 0];

        MainBoard.SendControlSignal(signal);
    }

    void FixedUpdate()
    {

        if (constant)
        {
            ControlSignal signal = new ControlSignal();
            signal.Throttle = constantThrottle;
            signal.Rudder = constantRudder;
            signal.Elevator = constantElevator;
            signal.Aileron = constantAileron;
            MainBoard.SendControlSignal(signal);
        }
        else
        {
            if (manual)
            {
                ControlSignal signal = new ControlSignal();
                if (stabilization)
                {
                    Rigidbody rigid = this.gameObject.GetComponentInChildren<Rigidbody>();
                    Debug.Log("Stable !");
                    MainBoard.inputMLP = Vector<float>.Build.DenseOfArray(new float[] { UAngle.SteerAngle(rigid.transform.eulerAngles.x), UAngle.SteerAngle(rigid.transform.eulerAngles.y), UAngle.SteerAngle(rigid.transform.eulerAngles.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z });
                    signal.Throttle = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][3, 0];
                    signal.Rudder = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 0];
                    signal.Elevator = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][1, 0];
                    signal.Aileron = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][2, 0];
                    MainBoard.SendControlSignal(signal);
                } else
                { 
                    signal.Throttle = Input.GetAxis("LeftY");
                    signal.Rudder = Input.GetAxis("LeftX");
                    signal.Elevator = Input.GetAxis("RightY");
                    signal.Aileron = Input.GetAxis("RightX");
                    MainBoard.SendControlSignal(signal);
                }

            }
        }
    }
}
    

       