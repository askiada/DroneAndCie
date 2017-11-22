using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lexmou.MachineLearning;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using MathNet.Numerics.Random;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using Lexmou.Utils;

namespace Lexmou.MachineLearning
{
    public class DroneStabilization : DroneTask
    {


        public DroneStabilization(SystemRandomSource rndGenerator) : base(rndGenerator)
        {
            fromTask = "Stabilization";
            rowIndex = 0;
            shapes = new List<int>() { 9, 4 };
            signal = new UCSignal(shapes[0]);
            individualSize = 0;
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                individualSize += (shapes[i] + 1) * shapes[i + 1];
            }
        }



        public override Vector3 GetInitialposition(int i)
        {
            return new Vector3(i * spacing, initialY, 0.0f);
        }
        public override Vector3 GetTargetPosition()
        {
            return Vector3.zero;
        }

        public override void GetTargetPosition(int index, Vector3[] targetPosition)
        {
            targetPosition[index].x = 0f;
            targetPosition[index].y = 0f;
            targetPosition[index].z = 0f;
        }

        public override float EvaluateIndividual(int i, Rigidbody rigid, Vector3 targetPosition)
        {
            return 1 / (1 + Mathf.Abs(rigid.velocity.y) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.x)) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.y)) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.z)) + Mathf.Abs(rigid.angularVelocity.x) + Mathf.Abs(rigid.angularVelocity.z) + Mathf.Abs(rigid.angularVelocity.y));
        }

        public override void UCSignal(Rigidbody rigid, Vector3 targetPosition)
        {

            signal.input[0] = UAngle.SteerAngle(rigid.transform.eulerAngles.x);
            signal.input[1] = UAngle.SteerAngle(rigid.transform.eulerAngles.y);
            signal.input[2] = UAngle.SteerAngle(rigid.transform.eulerAngles.z);
            signal.input[3] = rigid.angularVelocity.x;
            signal.input[4] = rigid.angularVelocity.y;
            signal.input[5] = rigid.angularVelocity.z;
            signal.input[6] = rigid.velocity.x;
            signal.input[7] = rigid.velocity.y;
            signal.input[8] = rigid.velocity.z;
            //signal.input = Vector<float>.Build.DenseOfArray(new float[] { UAngle.SteerAngle(rigid.transform.eulerAngles.x), UAngle.SteerAngle(rigid.transform.eulerAngles.y), UAngle.SteerAngle(rigid.transform.eulerAngles.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z });
            //return signal;
        }
    }


}
