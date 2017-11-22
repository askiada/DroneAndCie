using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Drone.Hardware;

public class CompassReceiver : Component<SensorSignal>
{

    public override SensorSignal ProcessSignal(SensorSignal signal)
    {
        //this.sensorOutput = Vector<float>.Build.DenseOfArray(new float[3] { rigidbody.transform.eulerAngles.x, rigidbody.transform.eulerAngles.y, rigidbody.transform.eulerAngles.z });
        return signal;
    }
}
