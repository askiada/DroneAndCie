using UnityEngine;
using System.Collections;
using System;
using Drone.Hardware;
public class ManualInputControl : InputControl2 {

    public override void SendSignal()
    {
        ControlSignal signal = new ControlSignal();
        signal.Throttle = Input.GetAxis("LeftY");
        signal.Rudder = Input.GetAxis("LeftX");
        signal.Elevator = Input.GetAxis("RightY");
        signal.Aileron = Input.GetAxis("RightX");
        MainBoard.SendControlSignal(signal);
    }
}
