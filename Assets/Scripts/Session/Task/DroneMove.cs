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
        public DroneMove(SystemRandomSource rndGenerator, string fromTask) : base(rndGenerator)
        {
            angleRandomRotation = new float[7] { 2, 10, 20, 30, 40, 50, 60 };
            medianThreshold = new float[7] { 70, 70, 70, 70, 70, 70, 70 };
            bestThreshold = new float[7] { 170, 170, 170, 170, 170, 170, 170};
            if(fromTask == "Stabilization")
            {
                rowIndex = 12;
            }else if(fromTask == "Move")
            {
                rowIndex = 0;
            }
            this.fromTask = fromTask;
            
            deltaDistribution = new ContinuousUniform(-5, 5, rndGenerator);
            shapes = new List<int>() {12, 4};
            signal = new UCSignal(shapes[0]);
            individualSize = 0;

            for (int i = 0; i < shapes.Count - 1; i++)
            {
                individualSize += (shapes[i] + 1) * shapes[i + 1];
            }
        }

        public override Vector3 GetTargetPosition(Vector3 fromPosition)
        {
            return new Vector3(fromPosition.x + (float)deltaDistribution.Sample(), fromPosition.y +(float)deltaDistribution.Sample(), fromPosition.z + (float)deltaDistribution.Sample());
        }

        public override void GetTargetPosition(int index, Vector3[] targetPosition)
        {
            Vector3 initPos = GetInitialposition(index);
            targetPosition[index].x = initPos.x + (float)deltaDistribution.Sample();
            targetPosition[index].y = initPos.y + (float)deltaDistribution.Sample();
            targetPosition[index].z = initPos.z + (float)deltaDistribution.Sample();
        }

        public override Vector3 GetInitialposition(int i)
        {
            return new Vector3(i * spacing, initialY, 0.0f);
        }

        public override float EvaluateIndividual(int i, Rigidbody rigid, Vector3 targetPosition)
        {
            //Vector3 initPos = GetInitialposition(i);
            return 1/(1+ Mathf.Abs(rigid.transform.position.x - targetPosition.x) + Mathf.Abs(rigid.transform.position.y - targetPosition.y) + Mathf.Abs(rigid.transform.position.z - targetPosition.z));
        }

        public override void UCSignal(Rigidbody rigid, Vector3 targetPosition)
        {
            signal.input[0] = rigid.transform.position.x  - targetPosition.x;
            signal.input[1] = rigid.transform.position.y  - targetPosition.y;
            signal.input[2] = rigid.transform.position.z  - targetPosition.z;
            signal.input[3] = UAngle.SteerAngle(rigid.transform.eulerAngles.x);
            signal.input[4] = UAngle.SteerAngle(rigid.transform.eulerAngles.y);
            signal.input[5] = UAngle.SteerAngle(rigid.transform.eulerAngles.z);
            signal.input[6] = rigid.angularVelocity.x;
            signal.input[7] = rigid.angularVelocity.y;
            signal.input[8] = rigid.angularVelocity.z;
            signal.input[9] = rigid.velocity.x;
            signal.input[10] = rigid.velocity.y;
            signal.input[11] = rigid.velocity.z;
        }
    }
}