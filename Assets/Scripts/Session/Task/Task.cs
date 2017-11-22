using UnityEngine;
using System.Collections;
using MathNet.Numerics.Random;

namespace Lexmou.MachineLearning
{
    /**
     * \interface Task
     * \brief Task interface used during a Session
     * \details This interface define a Random Generator and the global structure of a task
     */ 
    public abstract class Task {

        /**
         * \brief Random Generator. Allow the generation of a repeatable random number sequence for each seed value.
         */
        public SystemRandomSource rndGenerator;
        /**
         * Constructor which use an existing Random Generator
         */
        public Task(SystemRandomSource rndGenerator)
        {
            this.rndGenerator = rndGenerator;
        }
        /**
         * \brief Define the UCSignal of the task. This UCSignal is a vector containing specific task info. Most of the time, it requires a Rigidbody. 
         */
        public abstract void UCSignal(Rigidbody rigid, Vector3 targetPosition);
    }
}
