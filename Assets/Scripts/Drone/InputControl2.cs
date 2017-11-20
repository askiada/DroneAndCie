using UnityEngine;
using System.Collections;

public abstract class InputControl2 : MonoBehaviour {

    public MainBoard MainBoard;

    public abstract void SendSignal();
}
