using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    public float lifeTime = 10f;
    public float initialY = 5.0f;

    Rigidbody rb;

    Vector3 origin;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        /* if (lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
            {
                Destruction();
            }
        } */

        if (transform.position.y <= 0.1f)
        {
            // newPosition.y += 0.6f;
            //float a = 0.6f;
            transform.position = new Vector3(origin.x, initialY, origin.z);
            rb.velocity = Vector3.zero;
            //Destruction();
        }
    }

    void Destruction()
    {
        Destroy(this.gameObject);
    }
}