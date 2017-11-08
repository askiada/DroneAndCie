using UnityEngine;
using System.Collections;
using Drone.Hardware;

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
        signal.Throttle = MainBoard.mlp.layers [MainBoard.mlp.shapesSize - 1]
        [3, 0];
        signal.Rudder = MainBoard.mlp.layers [MainBoard.mlp.shapesSize - 1]
        [0, 0];
        signal.Elevator = MainBoard.mlp.layers [MainBoard.mlp.shapesSize - 1]
        [1, 0];
        signal.Aileron = MainBoard.mlp.layers [MainBoard.mlp.shapesSize - 1]
        [2, 0];

        MainBoard.SendControlSignal(signal);
    }

    /*void FixedUpdate()
    {
        ControlSignal signal = new ControlSignal();

        if (stabilization)
        {
            Debug.Log("Stable !");
            signal.Throttle = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][3, 0];
            signal.Rudder = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 0];
            signal.Elevator = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][1, 0];
            signal.Aileron = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][2, 0];
        }else
        {
            if (constant)
            {
                signal.Throttle = constantThrottle;
                signal.Rudder = constantRudder;
                signal.Elevator = constantElevator;
                signal.Aileron = constantAileron;
            }
            else
            {
                if (manual)
                {
                    signal.Throttle = Input.GetAxis("LeftY");
                    signal.Rudder = Input.GetAxis("LeftX");
                    signal.Elevator = Input.GetAxis("RightY");
                    signal.Aileron = Input.GetAxis("RightX");
                }
                else
                {
                    Debug.Log("FixedUpdate InputControl");
                    signal.Throttle = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][3, 0];
                    signal.Rudder = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 0];
                    signal.Elevator = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][1, 0];
                    signal.Aileron = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][2, 0];
                }
            }
        }

       


        //Debug.Log("InputControl Signal : " + signal.ToString());
        MainBoard.SendControlSignal(signal);

    }*/

}