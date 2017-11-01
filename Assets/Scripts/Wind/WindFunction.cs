using UnityEngine;
using System.Collections;

public static class WindFunction  {

    public static Vector3 linear(Vector3 direction, float value)
    {
        return direction * value;
    } 
    
}
