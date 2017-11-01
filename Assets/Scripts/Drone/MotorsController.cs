using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
using Drone.Hardware;

public class MotorsController : Drone.Hardware.Component <ThrustSignal>
{
	
	public MotorThrust MotorFR;
	public MotorThrust MotorFL;
	public MotorThrust MotorRR;
	public MotorThrust MotorRL;


    #region implemented abstract members of Component
    public override ThrustSignal ProcessSignal(ThrustSignal signal)
    {
        MotorFR.ApplyTorque(signal.FRThrust);
        MotorFL.ApplyTorque(signal.FLThrust);
        MotorRR.ApplyTorque(signal.RRThrust);
        MotorRL.ApplyTorque(signal.RLThrust);

        return signal;
    }


    /*public override ThrustSignal ProcessSignal (ThrustSignal signal, MultiLayer mlp)
	{

        //Debug.Log("fsdf" + mlp.layers[mlp.shapesSize - 1][0, 0]);
        Debug.Log("MotorFR : " + mlp.layers[mlp.shapesSize - 2][0, 0]);
        Debug.Log("MotorFL : " + mlp.layers[mlp.shapesSize - 2][0, 1]);
        Debug.Log("MotorRR : " + mlp.layers[mlp.shapesSize - 2][0, 2]);
        Debug.Log("MotorRL : " + mlp.layers[mlp.shapesSize - 2][0, 3]);
        MotorFR.ApplyTorque(mlp.layers[mlp.shapesSize - 2][0, 0]);
        MotorFL.ApplyTorque(mlp.layers[mlp.shapesSize - 2][0, 1]);
        MotorRR.ApplyTorque(mlp.layers[mlp.shapesSize - 2][0, 2]);
        MotorRL.ApplyTorque(mlp.layers[mlp.shapesSize - 2][0, 3]);

        MotorFR.ApplyTorque (signal.FRThrust);
		MotorFL.ApplyTorque (signal.FLThrust);
		MotorRR.ApplyTorque (signal.RRThrust);
		MotorRL.ApplyTorque (signal.RLThrust);

        return signal;
	}*/
    #endregion

    /*public override float[,] gyro()
    {
        Vector2 gyro = Gyro.complete(MotorFR.transform.position, MotorRL.transform.position, MotorRR.transform.position, MotorFL.transform.position);
        float[,] tmp = new float[1, 2] { { gyro.x, gyro.y } };
        return tmp;
    }*/


}

