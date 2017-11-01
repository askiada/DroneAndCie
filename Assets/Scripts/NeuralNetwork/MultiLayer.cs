using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class MultiLayer {

    public List<float[,]> layers;
    public List<float[,]> weights;
    List<float[,]> dw;
    List<int> shapes;
    public int shapesSize;
    bool saveInfos;

    public MultiLayer(List<int> shapeList, int seed, int batchSize, bool save = false)
    {
        saveInfos = save;
        shapes = shapeList;
        shapesSize = shapes.Count;
        layers = new List<float[,]>();
        weights = new List<float[,]>();
        dw = new List<float[,]>();
        layers.Add(new float[1, shapeList[0] + 1]);

        for (int i = 1; i < shapesSize; i++)
        {
            //Debug.Log("i " + i);
            if((i+1) == shapesSize)
            {
                layers.Add(new float[1, shapeList[i]]);
            }else
            {
                layers.Add(new float[1, shapeList[i]]);
            }
            
        }
        layers[0][0, layers[0].GetLength(1) - 1] = 1.0f;

        for (int i = 0; i < shapesSize - 1; i++)
        {
            weights.Add(new float[layers[i].GetLength(1), layers[i+1].GetLength(1)]);
            dw.Add(new float[layers[i].GetLength(1), layers[i + 1].GetLength(1)]);
        }
        UnityEngine.Random.InitState(seed);
    }



    

    public void Reset(float initialValueWeights, bool firstReset, List<float[,]> customWeights = null)
    {
        for (int i = 0; i < shapesSize - 1; i++)
        {
            for(int j = 0; j < weights[i].GetLength(0);j++)
            {
                for (int k = 0; k < weights[i].GetLength(1); k++)
                {
                    //Debug.Log("i, j , k " + i + " - "+j + " - " + k);
                    //Debug.Log("Lenght " + weights[i].GetLength(0) + "-" + weights[i].GetLength(1));
                    if(customWeights != null)
                    {
                        weights[i][j, k] = customWeights[i][j, k];
                    }
                    else
                    {
                        weights[i][j, k] = UnityEngine.Random.Range(-initialValueWeights, initialValueWeights);
                    }
                    
                    dw[i][j, k] = 0.0f;
                }
            }
   
        }
        WriteListArray("Weights", "init", weights);
    }

    

    public void PropagateForward(float[,] data)
    {
        for (int i = 0; i < layers[0].GetLength(0); i++)
        {
            for (int j = 0; j < layers[0].GetLength(1) - 1; j++)
            {
                //Debug.Log("layers[0]" + layers[0].GetLength(0) + "-" + layers[0].GetLength(1));
                //Debug.Log("i,j" + i + "-" + j);
                layers[0][i, j] = data[0,j];
            }
        }
            

        for(int i = 1; i < shapesSize - 1; i++)
        {
            layers[i] = Activation.InternalActivation(Multiplication.FalkScheme(layers[i-1], weights[i-1]), "Sigmoid", false);
        }
        //Debug.Log("output  " + layers[shapesSize - 2].GetLength(0) +" -- " + layers[shapesSize - 2].GetLength(1));
        //Debug.Log("outputscsd  " + weights[shapesSize - 2].GetLength(0) + " -- " + weights[shapesSize - 2].GetLength(1));
        layers[shapesSize - 1] = Activation.OutputActivation(Multiplication.FalkScheme(layers[shapesSize - 2], weights[shapesSize - 2]), "Sigmoid", false);

        //Debug.Log("output" + layers[shapesSize - 1][0,0]);
        WriteListArray("Layers", "propagate", layers);
    }

    public float[,] Error(float[,] estimated, float[,] desired)
    {
        float[,] error = new float[1,desired.Length];
        for(int i = 0; i < desired.Length; i++)
        {
            //Debug.Log(desired.GetLength(0) +"-" + desired.GetLength(1));
            //Debug.Log(estimated.GetLength(0) + "-" + estimated.GetLength(1));
            error[0,i] = desired[0,i] - estimated[0,i];
        }
        Debug.Log("Error : " + ((error[0, 0] * error[0, 0]) + (error[0, 1] * error[0, 1])));
        return error;
    }


    

    public List<float[,]> Delta(float[,] error)
    {
        List<float[,]> deltas = new List<float[,]>();

        float[,] delta = new float[1,shapes[shapesSize - 1]];
        //Debug.Log("Error " + error.GetLength(0) + "-" + error.GetLength(1));
        //Debug.Log("layers[shapesSize - 1] " + DerivativeActivation(layers[shapesSize - 1]).GetLength(0) + "-" + DerivativeActivation(layers[shapesSize - 1]).GetLength(1));

        delta = Multiplication.ElementWise(error, Activation.OutputDerivativeActivation(layers[shapesSize - 1], "Sigmoid"));
        deltas.Add(delta);
        for (int i = shapesSize - 2; i > 0; i--)
        {
            //Debug.Log("Deltas " + deltas[deltas.Count - 1].GetLength(0) + "-" + deltas[deltas.Count - 1].GetLength(1));
            //Debug.Log("Deltas " + weights[i].GetLength(0) + "-" + weights[i].GetLength(1));
            //float[,] tmp = Multiplication.ElementWise(Multiplication.ElementWise(deltas[deltas.Count - 1], weights[i]), DerivativeActivation(layers[i]));
            //float[,] tmp = weights[i].TransposeRowsAndColumns();
            deltas.Add(Multiplication.ElementWise(Multiplication.FalkScheme(deltas[deltas.Count - 1], weights[i], false, true), Activation.InternalDerivativeActivation(layers[i], "Sigmoid")));
        }
        WriteListArray("Deltas", "deltas", deltas);
        return deltas;
    }

    public void Learn(float[,] sampleInput, float[,] sampleOutputDesired, float lRate, float momentum, int epochs)
    {
        for(int epoch = 0; epoch < epochs; epoch++)
        {
            PropagateForward(sampleInput);
            List<float[,]> deltas = Delta(Error(layers[shapesSize - 1], sampleOutputDesired));
            for (int i = 0; i < shapesSize - 1; i++)
            {
                //Debug.Log("layers " + layers[i].GetLength(0) + "-" + layers[i].GetLength(1));
                //Debug.Log("Deltas " + deltas[shapesSize - 2 - i].GetLength(0) + "-" + deltas[shapesSize - 2 - i].GetLength(1));
                
                //float[,] tmp = layers[i].TransposeRowsAndColumns();
                //Debug.Log("layers " + layers[i][0, 0] + "-" + layers[i][0, 1]);
                //Debug.Log("tmp " + tmp[0, 0] + "-" + tmp[1, 0]);
                //Debug.Log("deltas " + deltas[shapesSize - 2 - i][0, 0] + "-" + deltas[shapesSize - 2 - i][0, 1]);
                float[,] dwTmp = Multiplication.FalkScheme(layers[i], deltas[shapesSize - 2 - i], true, false);
                //Debug.Log("dwTmp " + dwTmp[0, 0] + "-" + dwTmp[0, 1]);
                for (int j=0; j < dwTmp.GetLength(0); j++)
                {
                    for (int k = 0; k < dwTmp.GetLength(1); k++)
                    {
                        weights[i][j, k] += dwTmp[j, k] * lRate;

                        //dwTmp[j, k] *= lRate;
                        dw[i][j, k] *= momentum;
                    }
                }
                //weights[i] = dwTmp;// + dw[i];
                //Debug.Log("weights " + weights[i][0, 0] + "-" + weights[i][0, 1]);
                dw[i] = dwTmp;
            }
            WriteListArray("Weights", "afterLearn", weights);
            WriteListArray("Dw", "afterLearn", dw);
        }
    } 


    void WriteListArray(string dir, string name, List<float[,]> L)
    {
        if (saveInfos)
        {
            string filePath = "Save/" + dir + "/";
            string path = filePath + name + ".txt";
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

            string tmp = "";
            for (int i = 0; i < L.Count; i++)
            {
                for (int j = 0; j < L[i].GetLength(0); j++)
                {

                    for (int k = 0; k < L[i].GetLength(1); k++)
                    {
                        tmp += " " + L[i][j, k];
                    }
                    tmp += Environment.NewLine;
                }
                tmp += Environment.NewLine + Environment.NewLine;
            }
            File.WriteAllText(path, tmp);
        }
    }

    /*void WriteLayer()
    {
        string path = "Tmp/Layer/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);

        for(int i = 0; i < shapesSize; i++)
        {
            for(int j = 0; j < layers[i].GetLength(0); j++)
            {
                string tmp = "";
                for (int k = 0; k < layers[i].GetLength(1); k++)
                {
                    tmp += " " + layers[i][j, k];
                }
                writer.WriteLine(tmp + "\n");
            }
            writer.WriteLine("\n\n\n");
        }
        writer.WriteLine("-----------------------------");
        writer.Close();
    }

    void WriteWeight()
    {
        string path = "Tmp/Weight/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);

        for (int i = 0; i < shapesSize - 1; i++)
        {
            for (int j = 0; j < weights[i].GetLength(0); j++)
            {
                string tmp = "";
                for (int k = 0; k < weights[i].GetLength(1); k++)
                {
                    tmp += " " + weights[i][j, k];
                }
                writer.WriteLine(tmp + "\n");
            }
            writer.WriteLine("\n\n\n");
        }
        writer.WriteLine("-----------------------------");
        writer.Close();
    }*/


}
