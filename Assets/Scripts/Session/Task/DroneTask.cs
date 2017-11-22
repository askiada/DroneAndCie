using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using Lexmou.Utils;
using Lexmou.Environment.Wind;

namespace Lexmou.MachineLearning
{
    public abstract class DroneTask : Task
    {
        public float spacing = 5.0f;
        public float initialY = 5.0f;
        public int individualSize;
        public string fromTask;
        public int rowIndex;
        public List<int> shapes;
        public UCSignal signal;
        public abstract Vector3 GetInitialposition(int i);
        public abstract Vector3 GetTargetPosition();
        public abstract void GetTargetPosition(int index, Vector3[] targetPosition);
        //public abstract float EvaluateIndividual(int i, Rigidbody rigid);
        public abstract float EvaluateIndividual(int i, Rigidbody rigid, Vector3 targetPosition);

        int indexRRArr = 0;

        float[] angleRandomRotation = new float[7] { 2, 10, 20, 30, 40, 50, 60 };
        float[] medianThreshold = new float[7] {200, 180, 170, 170, 165, 165, 165};
        float[] bestThreshold = new float[7] { 240, 220, 220, 220, 220, 220, 220 };
        public float _angleRandomRotation = 0f;
        public float AngleRandomRotation
        {
            get { return _angleRandomRotation; }
            set
            {
                _angleRandomRotation = value;
                Debug.Log("New Angle : " + _angleRandomRotation);
                this.distribution = new ContinuousUniform(-AngleRandomRotation, AngleRandomRotation, rndGenerator);
            }
        }

        public float _windStrength = 0.0f;
        public float WindStrength
        {
            get { return _windStrength; }
            set
            {
                _windStrength = value;
                GameObject.Find("WindArea").GetComponent<WindArea>().windStrength = _windStrength;
            }
        }


        ContinuousUniform distribution;

        public DroneTask(SystemRandomSource rndGenerator) : base(rndGenerator)
        {
            distribution = new ContinuousUniform(-AngleRandomRotation, AngleRandomRotation, rndGenerator);
        }

        public bool ResetOrientation(Rigidbody rigid, float best, float median, int index,  int populationSize)
        {
            bool b = (best > bestThreshold[indexRRArr] && median > medianThreshold[indexRRArr] && index == populationSize - 1);
            
            return ResetOrientation(b,rigid);
        }


        public bool ResetOrientation(bool condition, Rigidbody rigid)
        {
            if (condition)
            {
                AngleRandomRotation = angleRandomRotation[indexRRArr++];
            }

            if(indexRRArr == angleRandomRotation.Length)
            {
                return false;
            }

            rigid.transform.eulerAngles = new Vector3(UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()), UAngle.ReverseSteerAngle((float)distribution.Sample()));
            return true;
        }
    }
}
