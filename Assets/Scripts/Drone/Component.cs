using System;
using UnityEngine;

namespace Drone.Hardware
{
	public abstract class Component<S> : MonoBehaviour
	{
        //public abstract float[,] gyro();
        public abstract S ProcessSignal(S signal);
        //public abstract S ProcessSignal (S signal, MultiLayer mlp);
	}
}