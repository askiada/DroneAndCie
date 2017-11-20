using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class UCSignal {
    public Vector<float> input;
    public UCSignal(int inputSize)
    {
        input = Vector<float>.Build.Dense(inputSize);
    }
}
