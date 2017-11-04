using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WindArea : MonoBehaviour
{
    List<Rigidbody> rigidbodiesInWindzoneList = new List<Rigidbody>();
    public Vector3 windDirection = Vector3.left;
    //public Vector3 prevWindDirection = Vector3.right;
    public bool show = false;
    public float windStrength = 5;
    public float initialY;
    public int cubeDepth = 1;
    public int numSelectors = 5;
    public GameObject[] selectorArr;
    public GameObject selector; //selected in the editor
    /*private float windDirectionX = 0.0f;
    private float windDirectionY = 0.0f;
    private float windDirectionZ = 0.0f;
    private float windValue = 0.0f;*/
    /*ParticleSystem wind;
    ParticleSystem.Particle[] wind_array;
    private float strength = 0.0f;*/


    void Awake()
    {
        if (show)
        {
            int cubeWidth = (int)Mathf.Floor(Mathf.Sqrt(numSelectors));


            GameObject firstObject = Instantiate(selector, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            Vector3 selectorScale = firstObject.transform.localScale;

            // float initialY = firstObject.initialY;
            //int cubeDepth = (int)Mathf.Floor(initialY / (5*selectorScale.x));


            selectorArr = new GameObject[cubeWidth * cubeWidth * cubeDepth];

            firstObject.transform.position = new Vector3(-selectorScale.x * (cubeWidth - 1), initialY, -selectorScale.x * (cubeWidth - 1));
            selectorArr[0] = firstObject;
            int index = 0;
            for (int i = 0; i < cubeWidth; i++)
            {
                for (int j = 0; j < cubeWidth; j++)
                {
                    for (int k = 0; k < cubeDepth; k++)
                    {
                        index = i + cubeWidth * (j + cubeDepth * k);
                        if (index == 0)
                        {
                            break;
                        }
                        else
                        {
                            GameObject go = Instantiate(selector, new Vector3(-selectorScale.x * (cubeWidth - 1) + (float)i * selectorScale.x * 4, initialY - (float)k * selectorScale.x * 5, -selectorScale.x * (cubeWidth - 1) + (float)j * selectorScale.x * 4), Quaternion.identity) as GameObject;
                            selectorArr[index] = go;
                        }

                    }

                }

            }
        }      
    }

    private void OnTriggerEnter(Collider col)
    {
        Rigidbody objectRigid = col.gameObject.GetComponent<Rigidbody>();

        if (objectRigid != null)
            rigidbodiesInWindzoneList.Add(objectRigid);
    }

    private void OnTriggerExit(Collider col)
    {
        Rigidbody objectRigid = col.gameObject.GetComponent<Rigidbody>();

        if (objectRigid != null)
            rigidbodiesInWindzoneList.Remove(objectRigid);
    }
    

    private void FixedUpdate()
    {
        /*int length = wind.GetParticles(wind_array);
        for (int i = 0; i < length; i++)
        {
            windDirectionX = Noise(wind_array[i].position.x, wind_array[i].position.y, wind_array[i].position.z);
            windDirectionY = Noise(wind_array[i].position.y, wind_array[i].position.z, wind_array[i].position.x);
            windDirectionZ = Noise(wind_array[i].position.z, wind_array[i].position.x, wind_array[i].position.y);
            windDirection = new Vector3(windDirectionX, windDirectionY, windDirectionZ);
            //windValue = Mathf.PerlinNoise(wind_array[i].position.x, wind_array[i].position.z);
            wind_array[i].velocity = new Vector3(windDirectionX, windDirectionY, windDirectionZ);
        }

        wind.SetParticles(wind_array, length);*/

        if (rigidbodiesInWindzoneList.Count > 0)
        {
            foreach (Rigidbody rigid in rigidbodiesInWindzoneList)
            {
                /*windDirectionX = Mathf.PerlinNoise(rigid.position.y, rigid.position.x);
                windDirectionY = Mathf.PerlinNoise(rigid.position.z, rigid.position.y);
                windDirectionZ = Mathf.PerlinNoise(rigid.position.x, rigid.position.z);
                windDirection = new Vector3(windDirectionX, windDirectionY, windDirectionZ);*/
                /*windDirectionX = Perlin.Noise(rigid.position.x, rigid.position.y, rigid.position.z);
                windDirectionY = Perlin.Noise(rigid.position.y, rigid.position.z, rigid.position.x);
                windDirectionZ = Perlin.Noise(rigid.position.z, rigid.position.x, rigid.position.y);
                windDirection = new Vector3(windDirectionX, windDirectionY, windDirectionZ);*/
                //windValue = Mathf.PerlinNoise(rigid.position.y, rigid.position.z);
                //Debug.Log("X - Z  : " + rigid.position.x + " - " + rigid.position.z);
                if (rigid != null)
                {
                    
                    float a = Perlin.Noise(rigid.position);
                    //Debug.Log("a : " + a);
                    Vector3 tmp = new Vector3(a, a, a);
                    
                    float coeff = 1.0f;
                    //Debug.Log(rigid.position + "   " + (windDirection + tmp) + " -------- " + windStrength + "   " + WindFunction.linear(windDirection + tmp, windStrength * coeff));
                    if (rigid.CompareTag("DroneMotor"))
                    {
                        coeff = rigid.mass;
                    }
                    Vector3 wind = WindFunction.linear(windDirection + tmp, windStrength * coeff);
                    //Debug.Log("a : " + a);
                    //Debug.Log("wind : " + wind);
                    rigid.AddForce(wind, ForceMode.Force);
                }
            }
        }
    }
}