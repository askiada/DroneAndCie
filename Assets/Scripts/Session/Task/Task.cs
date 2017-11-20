using UnityEngine;
using System.Collections;

namespace Lexmou.MachineLearning
{
    public abstract class Task {
        public abstract UCSignal UCSignal(Rigidbody rigid);
    }
}
