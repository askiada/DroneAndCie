using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lexmou.MachineLearning;
using Lexmou.MachineLearning.NeuralNetwork.FeedForward;
using MathNet.Numerics.Random;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Lexmou.MachineLearning
{
    public class DroneStabilization : DroneTask
    {

        public DroneStabilization()
        {
            shapes = new List<int>() { 9, 4 };
            individualSize = 0;
            for (int i = 0; i < shapes.Count - 1; i++)
            {
                individualSize += (shapes[i] + 1) * shapes[i + 1];
            }
        }

        public override void Build(params object[] args)
        {
            /*GameObject[] dronePopulation = (GameObject[]) args[0];
            MultiLayerMathsNet[] mlpPopulation = (MultiLayerMathsNet[])args[1];*/
            //Debug.Log(dronePopulation.Length);
            for (int i = 0; i < ((GameObject[])args[0]).Length; i++)
            {
                ((GameObject[]) args[0])[i] = MonoBehaviour.Instantiate((GameObject)Resources.Load("DroneGene"), new Vector3(i * spacing, initialY, 0.0f), Quaternion.identity) as GameObject;
                ((GameObject[])args[0])[i].GetComponent<MainBoard>().inputSize = shapes[0];
                ((MultiLayerMathsNet[])args[1])[i] = new MultiLayerMathsNet(-1, (SystemRandomSource) args[2], shapes, 1, (float) args[3]);
                ((GameObject[])args[0])[i].GetComponent<MainBoard>().mlp = ((MultiLayerMathsNet[])args[1])[i];
                //Problème ici, je reconstruis pas tmpBuildCustomWeigths
                ((MultiLayerMathsNet[])args[1])[i].Reset(false, (List<Matrix<float>>) args[4]);
            }
        }
    }
}
