using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lexmou.MachineLearning;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using MathNet.Numerics.Random;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using System;
using Lexmou.Utils;

namespace Lexmou.MachineLearning
{
    public class DroneMove : DroneTask
    {

        ContinuousUniform deltaDistribution;

        public DroneMove(SystemRandomSource rndGenerator) : base(rndGenerator)
        {
            fromTask = "Stabilization";
            rowIndex = 12;
            deltaDistribution = new ContinuousUniform(-1, 1, rndGenerator);
            shapes = new List<int>() {12, 4};
            signal = new UCSignal(shapes[0]);
            individualSize = 0;
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                individualSize += (shapes[i] + 1) * shapes[i + 1];
            }
        }

        public override Vector3 GetTargetPosition()
        {
            return new Vector3((float)deltaDistribution.Sample(),(float)deltaDistribution.Sample(),(float)deltaDistribution.Sample());
            /* (float)deltaDistribution.Sample();
            targetPosition[index].y = (float)deltaDistribution.Sample();
            targetPosition[index].z = (float)deltaDistribution.Sample();
            */
        }

        public override void GetTargetPosition(int index, Vector3[] targetPosition)
        {
            targetPosition[index].x = (float)deltaDistribution.Sample();
            targetPosition[index].y = (float)deltaDistribution.Sample();
            targetPosition[index].z = (float)deltaDistribution.Sample();
        }

        public override Vector3 GetInitialposition(int i)
        {
            return new Vector3(i * spacing, initialY, 0.0f);
        }

        public override float EvaluateIndividual(int i, Rigidbody rigid, Vector3 targetPosition)
        {
            return 1/(1+ Mathf.Abs(rigid.transform.localPosition.x - targetPosition.x) + Mathf.Abs(rigid.transform.localPosition.y - targetPosition.y) + Mathf.Abs(rigid.transform.localPosition.z - targetPosition.z));
        }

        public override void UCSignal(Rigidbody rigid, Vector3 targetPosition)
        {
            signal.input[0] = rigid.transform.localPosition.x - targetPosition.x;
            signal.input[1] = rigid.transform.localPosition.y - targetPosition.y;
            signal.input[2] = rigid.transform.localPosition.z - targetPosition.z;
            signal.input[3] = UAngle.SteerAngle(rigid.transform.eulerAngles.x);
            signal.input[4] = UAngle.SteerAngle(rigid.transform.eulerAngles.y);
            signal.input[5] = UAngle.SteerAngle(rigid.transform.eulerAngles.z);
            signal.input[6] = rigid.angularVelocity.x;
            signal.input[7] = rigid.angularVelocity.y;
            signal.input[8] = rigid.angularVelocity.z;
            signal.input[9] = rigid.velocity.x;
            signal.input[10] = rigid.velocity.y;
            signal.input[11] = rigid.velocity.z;
            //signal.input = Vector<float>.Build.DenseOfArray(new float[] { UAngle.SteerAngle(rigid.transform.eulerAngles.x), UAngle.SteerAngle(rigid.transform.eulerAngles.y), UAngle.SteerAngle(rigid.transform.eulerAngles.z), rigid.angularVelocity.x, rigid.angularVelocity.y, rigid.angularVelocity.z, rigid.velocity.x, rigid.velocity.y, rigid.velocity.z });

        }
    }


}