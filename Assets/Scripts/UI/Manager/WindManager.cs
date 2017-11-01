using UnityEngine;
using System.Collections;

public class WindManager : MonoBehaviour
{ 
    public void SetWind(float val)
    {
        GameObject.Find("WindArea").GetComponent<WindArea>().windStrength = val;
    }
}
