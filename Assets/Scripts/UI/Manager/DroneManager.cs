using UnityEngine;
using System.Collections;

public class DroneManager : MonoBehaviour {

    public void SetMass(float val)
    {
        GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().mass = val;
    }

    public void SetDrag(float val)
    {
        GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().drag = val;
    }

    public void setMaxThrust(float val)
    {
        GameObject[] motors = GameObject.FindGameObjectsWithTag("DroneMotor");

        foreach(GameObject motor in motors)
        {
            //Debug.Log("Motor " + motor.name);
            motor.GetComponent<MotorThrust>().MaxThrust = val;
        }
    }


    public void restartPositionRotation()
    {
        GameObject drone = GameObject.FindGameObjectsWithTag("Drone")[0];

        Debug.Log("drone " +  drone.transform.position.ToString());
        //drone.transform.position = new Vector3(0.0f, 10.0f, 0.0f);
        GameObject frame = GameObject.FindGameObjectsWithTag("Frame")[0];
        Debug.Log("frame " + frame.transform.eulerAngles.ToString());
        //frame.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        //drone.transform.position = new Vector3(0.0f, 10.0f, 0.0f);


        Rigidbody rigid = frame.GetComponent<Rigidbody>();
        /*rigid.velocity = new Vector3(0f, 0f, 0f);
        rigid.angularVelocity = new Vector3(0f, 0f, 0f);*/
        //Time.timeScale = 0f;

        frame.transform.eulerAngles = Vector3.zero;
        frame.transform.position = new Vector3(0.0f, 10.0f, 0.0f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;


    }
    /*void LateUpdate()
    {
        GameObject frame = GameObject.FindGameObjectsWithTag("Frame")[0];
        Rigidbody rigid = frame.GetComponent<Rigidbody>();
        rigid.isKinematic = false;
    }*/





}
