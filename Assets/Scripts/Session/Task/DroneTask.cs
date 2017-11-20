using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lexmou.MachineLearning
{
    public abstract class DroneTask : Task
    {
        public float spacing = 5.0f;
        public float initialY = 5.0f;
        public int individualSize;
        public List<int> shapes;
        public UCSignal signal;
        public abstract Vector3 GetInitialposition(int i);
        public abstract float EvaluateIndividual(int i, Rigidbody rigid);
    }
}
