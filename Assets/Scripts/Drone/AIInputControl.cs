using UnityEngine;
using System.Collections;
using System;
using Drone.Hardware;

public class AIInputControl : InputControl2 {

    public override void SendSignal()
    {
        MainBoard.SendSensorSignal();
    }
}
