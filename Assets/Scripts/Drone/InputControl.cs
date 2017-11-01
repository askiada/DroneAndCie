using UnityEngine;
using System.Collections;
using Drone.Hardware;

public class InputControl : MonoBehaviour
{

    public MainBoard MainBoard;

    public bool manual = false;

    public bool constant = false;

    public float constantThrottle = 0.0f;
    public float constantRudder = 0.0f;
    public float constantElevator = 0.0f;
    public float constantAileron = 0.0f;

    void FixedUpdate()
    {
        ControlSignal signal = new ControlSignal();


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
                //Debug.Log("hhhhhhhhhhhhhhhhhhhhh" + MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 3]);
                signal.Throttle = 0.02f + MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 3];
                signal.Rudder = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 0];
                signal.Elevator = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 1];
                signal.Aileron = MainBoard.mlp.layers[MainBoard.mlp.shapesSize - 1][0, 2];



                /*float[,] tmp = MainBoard.inputMLP;
                //float[,] desired = new float[1, 6] { { 0.0f, 0.01f, 0.0f, 0.0f, 0.0f, 0.0f } };
                Debug.Log("Gyro Input" + tmp[0, 0] + " " + tmp[0, 1] + " " + tmp[0, 2]);
                float[,] tmp2 = new float[1, 6] { { 0.0f, 0.001f, 0.0f, -tmp[0, 3], -tmp[0, 4], -tmp[0, 5] } };
                MainBoard.mlp.Learn(tmp, tmp2, 0.01f, 0.5f, 1);*/
                //Debug.Log("InputMlp " + MainBoard.inputMLP);
                MainBoard.mlp.PropagateForward(MainBoard.inputMLP);

            }
        }


        //Debug.Log("InputControl Signal : " + signal.ToString());
        MainBoard.SendControlSignal(signal);

    }

}