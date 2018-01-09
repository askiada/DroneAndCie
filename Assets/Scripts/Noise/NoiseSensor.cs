using UnityEngine;
using System.Collections;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class NoiseSensor  {

    //https://github.com/ethz-asl/kalibr/wiki/IMU-Noise-Model


    float mean, stdDevWhite, stdDevBrown, deltaT;
    float bd = 0.0f;
    SystemRandomSource rndGenerator;
    Normal normal, stdNormal;
    // Use this for initialization
    public NoiseSensor(float mean, float stdDevWhite, float stdDevBrown, float deltaT, SystemRandomSource rndGenerator) {
        this.rndGenerator = rndGenerator;
        this.mean = mean;
        this.stdDevWhite = stdDevWhite;
        this.stdDevBrown = stdDevBrown;
        this.deltaT = deltaT;
        normal = new Normal(mean, stdDevWhite, rndGenerator);
        stdNormal = new Normal(0.0f, 1.0f, rndGenerator);
    }
	
	// Update is called once per frame
	public float Add (float value) {
        return value + (float) normal.Sample();
	}


    public float AddDiscrete(float value, bool brown = true)
    {
        if (brown)
        {
            return AddDiscreteBrown(value + stdDevWhite * Mathf.Sqrt(1 / deltaT) * (float)stdNormal.Sample());
        }
        return value + stdDevWhite * Mathf.Sqrt(1 / deltaT) * (float)stdNormal.Sample();
    }

    public float AddDiscreteBrown(float value)
    {
        float tmpBd = stdDevBrown * Mathf.Sqrt(1 / deltaT) * (float)stdNormal.Sample();
        float result = value + bd + tmpBd;
        bd = tmpBd;
        return result;
    }
}
