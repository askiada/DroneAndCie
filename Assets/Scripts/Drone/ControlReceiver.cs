using UnityEngine;
using System.Collections;
using Drone.Hardware;

public class ControlReceiver : Drone.Hardware.Component <ControlSignal>
{
    public float ThrottleSensitivity = 1.0f;
	public float ElevatorSensitivity = .5f;
	public float AileronSensitivity = .5f;
	public float RudderSensitivity = .5f;
	
	public MainBoard MainBoard;

    #region implemented abstract members of Component


    public override ControlSignal ProcessSignal (ControlSignal signal)
	{
		ThrustSignal thrust = new ThrustSignal ();
		
		// Throttle
		if (signal.Throttle >= 0f)
		{
            float throttle = (signal.Throttle * ThrottleSensitivity);
			thrust.FRThrust = throttle;
			thrust.FLThrust = throttle;
			thrust.RRThrust = throttle;
			thrust.RLThrust = throttle;
		}
		
		// Rudder
		if (signal.Rudder > 0f)
		{
			// turn right
			float rudder = (signal.Rudder * RudderSensitivity) / 2;
			thrust.FRThrust -= rudder;
			thrust.FLThrust += rudder;
			thrust.RRThrust += rudder;
			thrust.RLThrust -= rudder;
		}
		else if (signal.Rudder < 0f)
		{
			// turn left
			float rudder = (-signal.Rudder * RudderSensitivity) / 2;
			thrust.FRThrust += rudder;
			thrust.FLThrust -= rudder;
			thrust.RRThrust -= rudder;
			thrust.RLThrust += rudder;
		}
		
		// Elevator
		if (signal.Elevator > 0f)
		{
			// go forward
			float elevator = (signal.Elevator * ElevatorSensitivity) / 2;
			thrust.FRThrust -= elevator;
			thrust.FLThrust -= elevator;
			thrust.RRThrust += elevator;
			thrust.RLThrust += elevator;
		}
		else if (signal.Elevator < 0f)
		{
			// go backward
			float elevator = (-signal.Elevator * ElevatorSensitivity) / 2;
			thrust.FRThrust += elevator;
			thrust.FLThrust += elevator;
			thrust.RRThrust -= elevator;
			thrust.RLThrust -= elevator;
		}
		
		// Aileron
		if (signal.Aileron > 0f)
		{
			// go right
			float aileron = (signal.Aileron * AileronSensitivity) / 2;
			thrust.FRThrust -= aileron;
			thrust.FLThrust += aileron;
			thrust.RRThrust -= aileron;
			thrust.RLThrust += aileron;
		}
		else if (signal.Aileron < 0f)
		{
			// go left
			float aileron = (-signal.Aileron * AileronSensitivity) / 2;
			thrust.FRThrust += aileron;
			thrust.FLThrust -= aileron;
			thrust.RRThrust += aileron;
			thrust.RLThrust -= aileron;
		}
		
		MainBoard.SendThrustSignal (thrust);
		
		return signal;
	}
    #endregion

    /*public override float[,] gyro()
    {
        return new float[,] { { 0.0f, 0.0f } };
    }*/

}