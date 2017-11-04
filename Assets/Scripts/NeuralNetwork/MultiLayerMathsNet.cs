using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class MultiLayerMathsNet {

    public List<Matrix<float>> layers;
    public List<Matrix<float>> weights;
    List<Matrix<float>> dw;
    public List<int> shapes;
    public int shapesSize;
    bool save;
    int seed;
    int batchSize;
    SystemRandomSource rndGenerator;
    ContinuousUniform cu;
    float initialValueWeights = 1.0f;

    public MultiLayerMathsNet(int seed, SystemRandomSource rndGenerator, List<int> shapes, int batchSize, float initialValueWeights, bool save = false)
    {
        this.initialValueWeights = initialValueWeights;
        this.batchSize = batchSize;
        this.seed = seed;
        if (rndGenerator != null)
        {
            this.rndGenerator = rndGenerator;
        }
        else
        {
            this.rndGenerator = new SystemRandomSource(seed);
        }

        this.save = save;
        this.shapes = shapes;
        shapesSize = shapes.Count;
        layers = new List<Matrix<float>>();
        weights = new List<Matrix<float>>();
        dw = new List<Matrix<float>>();
        
        if (rndGenerator != null)
        {
            this.rndGenerator = rndGenerator;
        }
        else
        {
            rndGenerator = new SystemRandomSource(seed);
        }

        this.cu = new ContinuousUniform(-initialValueWeights, initialValueWeights, rndGenerator);

        //layers.Add(Matrix<float>.Build.Dense(shapes[0] + 1, batchSize));

        for (int i = 0; i < shapesSize - 1; i++)
        {
            layers.Add(Matrix<float>.Build.Dense(shapes[i] + 1, batchSize));
            layers[i].SetRow(shapes[i], Vector<float>.Build.Dense(batchSize, 1.0f));
        }

        layers.Add(Matrix<float>.Build.Dense(shapes[shapesSize - 1], batchSize));

        for (int i = 0; i < shapesSize - 1; i++)
        {
            weights.Add(Matrix<float>.Build.Dense(layers[i + 1].RowCount, layers[i].RowCount));
            dw.Add(Matrix<float>.Build.Dense(layers[i + 1].RowCount, layers[i].RowCount));

        }

        //layers[0].SetRow(shapes[0], Vector<float>.Build.Dense(batchSize, 1.0f));
    }




    float fillWeights(float value)
    {
        return (float)cu.Sample();
    }


    public void Reset(bool firstReset, List<Matrix<float>> customWeights = null)
    {
        /*if(firstReset && customWeights != null)
        {
            throw new System.ArgumentException("Wow ! firstReset is true and customWeights is not Null. I will ignore customWeights !");
        }*/
        if (firstReset)
        {
            for (int i = 0; i < shapesSize - 1; i++)
            {
                weights[i].MapInplace(fillWeights);
                dw[i].Clear();
                //Debug.Log(weights);
            }
        }

        if(customWeights != null)
        {
            for (int i = 0; i < shapesSize - 1; i++)
            {
                //Debug.Log(weights.Count + "        " + customWeights.Count);
                weights[i] = customWeights[i];
                //Debug.Log(weights[i]);
                //customWeights[i].CopyTo(weights[i]);
                dw[i].Clear();
            }
        }

        
        //WriteListArray("Weights", "init", weights);
    }



    public float Tanh(float value)
    {
        return (float)System.Math.Tanh(value);
    }

    public float SigExp(float value)
    {
        return (float)(1 / (1 + System.Math.Exp((float)(-value))));
    }

    public void PropagateForward(Vector<float> data)
    {
        for(int i=0; i < layers[0].ColumnCount; i++)
        {
            layers[0].SetColumn(i,0, shapes[0], data);
        }

        for (int i = 1; i < shapesSize - 1; i++)
        {
            layers[i].SetSubMatrix(0,0, weights[i - 1].Multiply(layers[i - 1]));
            //layers[i] = Matrix<float>.Build.DenseOfArray(Multiplication.FalkScheme(weights[i - 1].AsArray(), layers[i - 1].AsArray()));

            layers[i].PointwiseTanh();
        }
        weights[shapesSize - 2].Multiply(layers[shapesSize - 2], layers[shapesSize - 1]);
        //layers[shapesSize - 1] = Matrix<float>.Build.DenseOfArray(Multiplication.FalkScheme(weights[shapesSize - 2].AsArray(), layers[shapesSize - 2].AsArray()));
        layers[shapesSize - 1].MapInplace(SigExp);
    }

    public void PropagateForward2(Vector<float> data)
    {
        //Debug.Log(data);
        //Debug.Log(layers[0].RowCount + "" +data.Count);
        for (int i = 0; i < layers[0].ColumnCount; i++)
        {
            layers[0].SetColumn(i, 0, shapes[0], data);
        }

        for (int i = 1; i < shapesSize - 1; i++)
        {
            //weights[i - 1].Multiply(layers[i - 1], layers[i]);
            layers[i] = Matrix<float>.Build.DenseOfArray(Multiplication.FalkScheme(weights[i - 1].ToArray(), layers[i - 1].ToArray()));
            layers[i].PointwiseTanh();
            //layers[i].MapInplace(Tanh);
        }
        //weights[shapesSize - 2].Multiply(layers[shapesSize - 2], layers[shapesSize - 1]);
        layers[shapesSize - 1] = Matrix<float>.Build.DenseOfArray(Multiplication.FalkScheme(weights[shapesSize - 2].ToArray(), layers[shapesSize - 2].ToArray()));
        //layers[shapesSize - 1].MapInplace(Tanh);
        layers[shapesSize - 1].MapInplace(SigExp);
    }




}
