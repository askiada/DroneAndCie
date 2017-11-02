using UnityEngine;
using System.Collections;
using Drone.Hardware;

public class Gyro  {

    public Vector3 targetAngle = new Vector3(0f, 0f, 0f);
    private Vector3 currentAngle;
    //private MainBoard m;

    public Gyro(MainBoard obj)
    {            
        //currentAngle = obj.transform.eulerAngles;
        //Debug.Log("Euler : " + currentAngle.x + " " + currentAngle.y + " " + currentAngle.z);
    }

    public float[,] complete3(Vector3 rotate)
    {
        //Debug.Log(rotate.w + " " + rotate.x + " " + rotate.y + " " + rotate.z);

        currentAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime), currentAngle.y, Mathf.LerpAngle(currentAngle.z, targetAngle.z, Time.deltaTime));
        return new float[1,3] { { currentAngle.x, currentAngle.y, currentAngle.z } };
    }
    


    public static float compute(Vector3 A, Vector3 B)
    {
        return A.y - B.y;
    }

    public static Vector2 complete(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        return new Vector2(compute(A,B), compute(C,D));
    }
}
