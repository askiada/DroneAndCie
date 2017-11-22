using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using Lexmou.Utils;
namespace Lexmou.MachineLearning.NeuralNetwork.FeedForward
{

    public class MultiLayerMathsNet
    {

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

        }




        float fillWeights(float value)
        {
            return (float)cu.Sample();
        }


        public void Reset(bool firstReset, List<Matrix<float>> customWeights = null)
        {
            if (firstReset)
            {
                for (int i = 0; i < shapesSize - 1; i++)
                {
                    weights[i].MapInplace(fillWeights);
                    dw[i].Clear();
                }
            }

            if (customWeights != null)
            {
                for (int i = 0; i < shapesSize - 1; i++)
                {
                    weights[i] = customWeights[i];
                    dw[i].Clear();
                }
            }
        }





        public void PropagateForward(Vector<float> data, bool falk = false)
        {
            //Debug.Log(data);
            for (int i = 0; i < layers[0].ColumnCount; i++)
            {
                layers[0].SetColumn(i, 0, shapes[0], data);
            }
            //Danger c'est pas worth niveau Garbage Collection si shapes est grand
            for (int i = 1; i < shapesSize - 1; i++)
            {
                layers[i].SetSubMatrix(0, 0, weights[i - 1].Multiply(layers[i - 1]));

                layers[i].MapInplace(UActivation.Tanh);
            }

            if (falk)
            {
                Multiplication.FalkSchemeM(weights[shapesSize - 2], layers[shapesSize - 2], layers[shapesSize - 1], UActivation.Tanh);
            }
            else
            {
                weights[shapesSize - 2].Multiply(layers[shapesSize - 2], layers[shapesSize - 1]);
                layers[shapesSize - 1].MapInplace(UActivation.Tanh);
            }           
        }

        public void PropagateForward2(Vector<float> data)
        {
            for (int i = 0; i < layers[0].ColumnCount; i++)
            {
                layers[0].SetColumn(i, 0, shapes[0], data);
            }

            for (int i = 1; i < shapesSize - 1; i++)
            {
                layers[i] = Matrix<float>.Build.DenseOfArray(Multiplication.FalkScheme(weights[i - 1].ToArray(), layers[i - 1].ToArray()));
                layers[i].PointwiseTanh();

            }

            layers[shapesSize - 1] = Matrix<float>.Build.DenseOfArray(Multiplication.FalkScheme(weights[shapesSize - 2].ToArray(), layers[shapesSize - 2].ToArray()));

            layers[shapesSize - 1].PointwiseTanh();
        }
    }
}
