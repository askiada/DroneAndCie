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

    /**
     * \class MultiLayerMathsNet
     * \brief Partial implementation of a Multilayer perceptron
     * \details Class MultiLayerMathsNet is an "optimized" unity3D implementation of a Multilayer perceptron. The following picture shows the general principle. For more details, Wikipedia is a good start.
     *          ![MLP Scheme](./media/MLP/scheme.png)
     */
    public class MultiLayerMathsNet
    {
        /**
         * \brief List of layers. Each element represents the values of the neurons in the layer for each element of the batch (Dimension \f$ layerSize \times batchSize\f$)
         */
        public List<Matrix<float>> layers;

        /**
         * \brief List of weights (\f$ shapeSize - 1 elements \f$). the i-th element represents the values of the weights between the i-th layer and the (i+1)-th (Dimension \f$ layers[i + 1].RowCount \times layers[i].RowCount\f$)
         */

        public List<Matrix<float>> weights;
        List<Matrix<float>> dw;

        /**
         * \brief List of the size of the layers. The first element represents the input layer.
         */
        public List<int> shapes;
        /**
         * \brief Total number of layers (include input and output layers)
         */
        public int shapesSize;
        bool save;
        /**
         * \brief Seed number to generate a random sequence with a random generator #rndGenerator
         */
        int seed;
        /**
         * \brief Size of the batch for MiniBatch/Batch Learning.
         */
        int batchSize;

        /**
         * \brief Random Generator. Allow the generation of a repeatable random number sequence for each seed value.
         */
        SystemRandomSource rndGenerator;
        ContinuousUniform cu;
        /**
         * \brief Initial value range of the distribution to build the weights
         */
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



        /**
         *  \brief Return a float from a continous uniform distribution. This function take a float argument in order to be use with a MapInPlace function.
         */

        float fillWeights(float value)
        {
            return (float)cu.Sample();
        }

        /**
         * \brief Set the weights of the MLP under conditions.
         * \param[in] firstReset fill the weights with random values
         * \param[in] customWeights Custom list of Matrix containing the weights used.
         */

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



        /**
         * \brief Forward Propagation to get the output.
         * \details In this example, there is only one output neuron (/f$ y /f$) like on the picture (the biasis are not represented)
         * \f[
         * 
         *  y = a(\sum_{j=0}^{M}w^{(y)}_{j}\times h_j + b_{H})
         *  \f]
         *  \f[
         *  h_j = a(\sum_{i=0}^{N}w^{(h_j)}_{i}\times x_i + b_{X})
         *  \f]
         */

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
