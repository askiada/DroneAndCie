using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    public Transform Target;
    Vector4 O1;
    Vector4 O2;
    public float distance = 5.0f;
    public bool follow = true;
    // Update is called once per frame

    float Det(float A, float B, float C)
    {
        float tmp = B * B - 4 * A * C;
        if ((tmp > -0.1f) && (tmp < 0.0f))
        {
            return 0;
        }
        return tmp;
    }

    float A(float d)
    {
        return d * d + 1;
    }

    float B(Vector4 O1, float a, float d)
    {
        return -2 * O1.x + 2 * O1.y * d - 2 * a * d;
    }

    float C(Vector4 O1, float a)
    {
        return O1.x * O1.x + O1.y * O1.y - 2 * O1.y * a + a * a - O1.w * O1.w;
    }


    float a(Vector4 O1, Vector4 O2)
    {
        return (-O1.x * O1.x - O1.y * O1.y + O2.x * O2.x + O2.y * O2.y + O1.w * O1.w - O2.w * O2.w) / (2 * (O2.y - O1.y));
    }

    float d(Vector4 O1, Vector4 O2)
    {
        return (O2.x - O1.x) / (O2.y - O1.y);
    }


    Vector3 I(Vector4 O1, Vector4 O2, float otherAxis, int numAxis, float alpha)
    {

        float aF = a(O1, O2);
        //Debug.Log("a " + aF);
        float dF = d(O1, O2);
        //Debug.Log("d " + aF);
        float AF = A(dF);
        //Debug.Log("A " + AF);
        float BF = B(O1, aF, dF);
        //Debug.Log("B " + BF);
        float CF = C(O1, aF);
        //Debug.Log("C " + CF);
        float detF = Det(AF, BF, CF);
        //Debug.Log("det " + detF);
        float sign = 1.0f;

        if (alpha > 180)
        {
            sign = -1;
        }
        float X = (-BF - sign * Mathf.Sqrt(detF)) / (2 * AF);
        float Y = (aF - X * dF);

        if(numAxis == 0)
        {
            return new Vector3(otherAxis, X, Y);
        } else if(numAxis == 1)
        {
            return new Vector3(X, otherAxis, Y);
        }
        return new Vector3(X, Y, otherAxis);


    }



    void Update()
    {
        if (follow)
        {
            O1.x = Target.transform.position.x;
            O1.y = Target.transform.position.z;
            O1.z = Target.transform.position.y;
            // O1.z = 5.0f * Mathf.Cos(Mathf.Deg2Rad*Target.transform.eulerAngles.y);
            O1.w = distance;

            O2.x = Target.transform.position.x;
            O2.y = Target.transform.position.z - (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.y));
            O2.z = Target.transform.position.y;
            O2.w = Mathf.Sin(Mathf.Deg2Rad * Target.transform.eulerAngles.y) * (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.y));


            /*Vector3 Otmp = I(O1, O2, Target.transform.position.y + 2.0f, 1, Target.transform.eulerAngles.y);


            Vector4 O3;
            O3.x = Target.transform.position.y;
            O3.y = Target.transform.position.z;
            O3.z = Target.transform.position.x;
            O3.w = distance;

            Vector4 O4;
            O4.x = Target.transform.position.y;
            O4.y = Target.transform.position.z - (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.x)); ;
            O4.z = Target.transform.position.x;
            O4.w = Mathf.Sin(Mathf.Deg2Rad * Target.transform.eulerAngles.x) * (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.x)); ;
            Vector3 Of = I(O3, O4, Target.transform.position.x , 0, Target.transform.eulerAngles.x);
            */
            transform.eulerAngles = new Vector3(0.0f, Target.transform.eulerAngles.y, Target.transform.eulerAngles.z);
            transform.position = I(O1, O2, Target.transform.position.y + 2.0f, 1, Target.transform.eulerAngles.y);
            //transform.position = new Vector3(Of.x, Of.y, Of.z);

            /*O1.x = Target.transform.position.x;
            O1.y = Target.transform.position.z;
            // O1.z = 5.0f * Mathf.Cos(Mathf.Deg2Rad*Target.transform.eulerAngles.y);
            O1.z = distance;

            O2.x = Target.transform.position.x;
            O2.y = Target.transform.position.z - (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.y));
            //O2.z = 5.0f * Mathf.Sin(Mathf.Deg2Rad*Target.transform.eulerAngles.y);
            O2.z = Mathf.Sin(Mathf.Deg2Rad * Target.transform.eulerAngles.y) * (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.y));


            Vector3 Otmp = I(O1, O2, Target.transform.position.y + 2.0f, Target.transform.eulerAngles.y);

            Vector3 O3;
            O3.x= Otmp.x;
            O3.y = Otmp.y - (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.z)); ;
            O3.z = Mathf.Sin(Mathf.Deg2Rad * Target.transform.eulerAngles.z) * (distance / Mathf.Cos(Mathf.Deg2Rad * Target.transform.eulerAngles.z));

            Vector3 Of = I(O1, O2, Otmp.z, Target.transform.eulerAngles.z);

            transform.eulerAngles = new Vector3(0.0f, Target.transform.eulerAngles.y, 0.0f);
            //transform.position = I(O1, O2, Target.transform.position.y + 2.0f, Target.transform.eulerAngles.y);
            transform.position = new Vector3(Of.x, Of.y, Of.z);*/

        }
        else
        {
            transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y + 2.0f, Target.transform.position.z - 5.0f);
        }

        //transform.LookAt(Target);
    }
}
