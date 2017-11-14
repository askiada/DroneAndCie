using UnityEngine;
using System.Collections;

namespace Lexmou.MachineLearning
{
    public abstract class Task {

        public abstract void Build(params object[] args);
    }
}
