using UnityEngine;
using System.Collections;

public class DroneManager : MonoBehaviour {

    public GameObject prefabDrone;

    GameObject drone;
    public float initialY = 3.0f;

    public void Restart(float mass = -1.0f, float drag = -1.0f, float maxThrust = 1.0f)
    {
        drone = Instantiate(prefabDrone, new Vector3(0.0f, initialY, -10.0f), Quaternion.identity) as GameObject;
        drone.GetComponent<MainBoard>().inputSize = 1;
        drone.GetComponent<InputControl>().manual = true;
        drone.name = "Drone";

        if(mass >= 0 && drag >= 0 && maxThrust >= 0)
        {
            Debug.Log(mass + " " + drag + " " + maxThrust);
            /*drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>().mass = mass;
            drone.GetComponent<MainBoard>().GetComponentInChildren<Rigidbody>().drag = mass;
            */
            SetMass(mass);
            //SetDrag(drag);
            setMaxThrust(maxThrust);
        }
    }

    void Awake()
    {
        Restart();
    }

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
        float droneMass = GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().mass;
        float droneDrag = GameObject.FindGameObjectsWithTag("Frame")[0].GetComponent<Rigidbody>().drag;
        float droneMaxThrust = GameObject.FindGameObjectsWithTag("DroneMotor")[0].GetComponent<MotorThrust>().MaxThrust;
        
        Destroy(drone);
        Restart(droneMass, droneDrag, droneMaxThrust);
    }

    /*void LateUpdate()
    {
        GameObject frame = GameObject.FindGameObjectsWithTag("Frame")[0];
        Rigidbody rigid = frame.GetComponent<Rigidbody>();
        rigid.isKinematic = false;
    }*/





}
