﻿using UnityEngine;
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

    /**
     * \class DroneStabilization
     * \brief Implementation of the stabilization task for a drone using a genetic algorithm and a MLP
     * \todo Replace the velocities parameters with the linear acceleration.
     */

    public class DroneStabilization : DroneTask
    {
        NoiseSensor noise;
        NoiseSensor lowNoise;
        /**
         * \brief Simple constructor of the task. Build all the hyper parameters of the model (Genetic algorithm + MLP). Define the shapes of the MLp and the size of an individual for the genetic part.
         */
        public DroneStabilization(SystemRandomSource rndGenerator, string fromTask) : base(rndGenerator)
        {
            float mean = 0.0f;
            float stdDev = 1.0f;
            float deltaT = 0.02f;
            noise = new NoiseSensor(mean, stdDev, stdDev, deltaT, rndGenerator);
            lowNoise = new NoiseSensor(mean, stdDev/5.0f, stdDev/5.0f, deltaT, rndGenerator);
            this.fromTask = "Stabilization";
            //Debug.Log(this.fromTask);
            rowIndex = 0;
            shapes = new List<int>() {9, 4};
            signal = new UCSignal(shapes[0]);
            individualSize = 0;
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                individualSize += (shapes[i] + 1) * shapes[i + 1];
            }
        }


        /**
         * \brief Define the initial position of the i-th individuals. The repartition is along the X axis.
         */

        public override Vector3 GetInitialposition(int i)
        {
            return new Vector3(i * spacing, initialY, 0.0f);
        }

        /**
         * \brief The target position of each individual during the task is the initial position. The objective is to stabilize the drone without moving it.
         */
        public override Vector3 GetTargetPosition(Vector3 fromPosition)
        {
            return fromPosition;
        }

        /**
         * \brief The target position of each individual during the task is the initial position. The objective is to stabilize the drone without moving it.
         */

        public override void GetTargetPosition(int index, Vector3[] targetPosition)
        {
            Vector3 initPos = GetInitialposition(index);
            targetPosition[index].x = initPos.x;
            targetPosition[index].y = initPos.y;
            targetPosition[index].z = initPos.z;
        }

        /**
         * \brief Define the objective function of the stabilization task. This version is perfectly working but not really realistic.
         * \f[ evaluation = \frac{1}{1+(|v_y| + |\theta| + |\phi| + |\psi| + |v_{\theta}| + |v_{\phi}| + |v_{\psi}| )} \f]
         * Technically it is very difficult to use the speed of the drone because a standard IMU provides linear acceleration.
         */

        public override float EvaluateIndividual(int i, Rigidbody rigid, Vector3 targetPosition)
        {
            return 1 / (1 + Mathf.Abs(rigid.velocity.y) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.x)) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.y)) + Mathf.Abs(UAngle.SteerAngle(rigid.transform.eulerAngles.z)) + Mathf.Abs(rigid.angularVelocity.x) + Mathf.Abs(rigid.angularVelocity.z) + Mathf.Abs(rigid.angularVelocity.y));
        }

        /**
         * \brief Define the inputs of the MLP for the stabilization task. \f[
         * \begin{pmatrix}
         *   \theta & \phi & \psi & v_{\theta} & v_{\phi} & v_{\psi} & v_x & v_y & v_z \\
         * \end{pmatrix}
         * \f] 
         * Like the objective funtion, it's not very realistic.
         * Technically it is very difficult to use the speed of the drone because a standard IMU provides linear acceleration.
         */

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

            /*signal.input[0] = noise.AddDiscrete(UAngle.SteerAngle(rigid.transform.eulerAngles.x));
            signal.input[1] = noise.AddDiscrete(UAngle.SteerAngle(rigid.transform.eulerAngles.y));
            signal.input[2] = noise.AddDiscrete(UAngle.SteerAngle(rigid.transform.eulerAngles.z));
            signal.input[3] = lowNoise.AddDiscrete(rigid.angularVelocity.x);
            signal.input[4] = lowNoise.AddDiscrete(rigid.angularVelocity.y);
            signal.input[5] = lowNoise.AddDiscrete(rigid.angularVelocity.z);
            signal.input[6] = lowNoise.AddDiscrete(rigid.velocity.x);
            signal.input[7] = lowNoise.AddDiscrete(rigid.velocity.y);
            signal.input[8] = lowNoise.AddDiscrete(rigid.velocity.z);*/
        }
    }
}
