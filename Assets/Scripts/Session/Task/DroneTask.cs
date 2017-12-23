using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using Lexmou.Utils;
using Lexmou.Environment.Wind;

namespace Lexmou.MachineLearning
{

    /**
     * \interface DroneTask
     * \brief DroneTask interface used during a DroneSession
     * \details This interface define the global structure of a drone task
     * \todo Redefine the subtask with the maximum euler angle orientation.
     */
    public abstract class DroneTask : Task
    {
        /**
         * \brief Space along a direction between two instances of a drone 
         */
        public float spacing = 5.0f;
        /**
        * \brief Initial height of each drone along Y axis
        */
        public float initialY = 5.0f;
        /**
        * \brief Number of parameters to evaluate for each drone (number of neurons, number of alleles  ...)
        */
        public int individualSize;
        /**
        * \brief Build the pramaters of a task from another one
        */
        public string fromTask;
        /**
        * \brief Build the matrix of paramaters with an offset (index of the row)
        */
        public int rowIndex;
        /**
        * \brief Shape of the MLP for each drone
        */
        public List<int> shapes;

        /**
        * \brief Task specific signal containing a vector of the inputs
        */
        public UCSignal signal;
        /**
        * \brief Define a initial position for the drone i
        */
        public abstract Vector3 GetInitialposition(int i);
        /**
        * \brief Define a target position from another position
        */
        public abstract Vector3 GetTargetPosition(Vector3 fromPosition);
        /**
        * \brief Define a target position for the drone num index.
        */
        public abstract void GetTargetPosition(int index, Vector3[] targetPosition);

        /**
        * \brief Define an objective function. Ideally this function is concave
        */
        public abstract float EvaluateIndividual(int i, Rigidbody rigid, Vector3 targetPosition);

        /**
        * \brief Bad implementation of random orientation subtask. This index define the progress with this subtask.
        */
        int indexRRArr = 0;

        /**
        * \brief Define the maximum of each euler angle during the progress of the subtask.
        */
        public float[] angleRandomRotation = new float[7] { 2, 10, 20, 30, 40, 50, 60 };
        /**
        * \brief Define the median threshold (of the objective function) before changing the maximum angle
        */
        public float[] medianThreshold = new float[7] {200, 180, 170, 170, 165, 165, 165};
        /**
        * \brief Define the best score threshold (of the objective function) before changing the maximum angle
        */
        public float[] bestThreshold = new float[7] { 240, 220, 220, 220, 220, 220, 220 };
        /**
        * \brief Define the actual maximum angle
        */
        public float _angleRandomRotation = 0f;
        /**
        * \brief Define the property of the maximum angle. Set this angle change the boundaries of the random distibution used.
        */
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

        /**
        * \brief Define the actual wind value.
        */

        public float _windStrength = 0.0f;
        /**
        * \brief Define the property of the maximum wind value.
        */
        public float WindStrength
        {
            get { return _windStrength; }
            set
            {
                _windStrength = value;
                GameObject.Find("WindArea").GetComponent<WindArea>().windStrength = _windStrength;
            }
        }

        /**
        * \brief Continue uniform distribution for the maximum euler angle subtask.
        */
        ContinuousUniform distribution;

        public DroneTask(SystemRandomSource rndGenerator) : base(rndGenerator)
        {
            distribution = new ContinuousUniform(-AngleRandomRotation, AngleRandomRotation, rndGenerator);
        }

        /**
        * \brief Reset the orientation of the drone define by an index. Take care of the maximum euler angle subtask.
        */

        public bool ResetOrientation(Rigidbody rigid, float best, float median, int index,  int populationSize)
        {
            bool b = (best > bestThreshold[indexRRArr] && median > medianThreshold[indexRRArr] && index == populationSize - 1);
            
            return ResetOrientation(b,rigid);
        }

        /**
        * \brief Reset the orientation of the drone define by an index. Take care of a specific condition to define a specific orientation.
        */
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
