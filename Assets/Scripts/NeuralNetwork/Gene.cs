using UnityEngine;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;


public class Gene  {

    private Matrix<float> population;
    private int seed;
    private int populationSize;
    private int individualSize;
    private float mutationRate;
    private Vector<float> evaluations;
    private float sumEvaluations;
    private float randomIndividualsRate;
    private float bestIndividualsRate;
    private int bestIndividualIndex;
    private Vector<float> bestIndividual;
    public SystemRandomSource rndGenerator;
    //private int[] permutations;


    public float bestScore;
    public int generation;

    public Matrix<float> GetPopulation()
    {
        return population;
    }

    public Gene(int seed, SystemRandomSource rndGenerator, int populationSize, int individualSize, float mutationRate = 0.1f, float randomIndividualsRate = 0.05f, float bestIndividualsRate = 0.05f)
    {
        this.bestScore = 0.0f;
        this.generation = 1;
        this.seed = seed;
        if(rndGenerator != null)
        {
            this.rndGenerator = rndGenerator;
        }
        else
        {
            this.rndGenerator = new SystemRandomSource(seed);
        }
        
        this.populationSize = populationSize;
        this.individualSize = individualSize;
        //UnityEngine.Random.InitState(seed);

        population = Matrix<float>.Build.Random(individualSize, populationSize, new ContinuousUniform(-1.0f, 1.0f, rndGenerator));
        evaluations = Vector<float>.Build.Dense(populationSize);
        sumEvaluations = 0.0f;
        //Debug.Log("Population " + population);
        //RandomSeed.Robust()

        this.mutationRate = mutationRate;
        this.randomIndividualsRate = randomIndividualsRate;
        this.bestIndividualsRate = bestIndividualsRate;

        /*permutations = new int[populationSize];
        for(int i = 0; i < populationSize; i++)
        {
            permutations[i] = i;
        }*/

    }

    public Vector<float> SelectionOneElement()
    {
        //Debug.Log("sumEval " + sumEvaluations);
        float pick = (float) ContinuousUniform.Sample(rndGenerator, 0.0d, (double) sumEvaluations);
        float current = 0.0f;
        //Vector<float> tmp;

        for(int i = 0; i < populationSize; i++)
        {
            current += evaluations[i];
            //Debug.Log()
            if(current >= pick)
            {
                //Debug.Log(i);
                return  population.Column(i);
            }
        }
        return null;
    }


    public void Selection()
    {
        int randomIndividualsNumber = Mathf.RoundToInt(randomIndividualsRate * populationSize);
        int bestIndividualsNumber = Mathf.RoundToInt(bestIndividualsRate * populationSize);
        Matrix<float> tmp = Matrix<float>.Build.Dense(individualSize, populationSize);//, seed);

        //Debug.Log(randomIndividualsNumber + "   " + bestIndividualsNumber);

        for (int i = 0; i < bestIndividualsNumber; i++)
        {
            tmp.SetColumn(i, bestIndividual);
        }

        for (int i = bestIndividualsNumber; i < bestIndividualsNumber + randomIndividualsNumber; i++)
        {
            tmp.SetColumn(i, Vector<float>.Build.Random(individualSize, new ContinuousUniform(-1.0f, 1.0f, rndGenerator)));
        }

        //Debug.Log("liugliyg  " + (randomIndividualsNumber + bestIndividualsNumber) + "   " + (populationSize - randomIndividualsNumber - bestIndividualsNumber));

        //Debug.Log(evaluations);
        for (int i = randomIndividualsNumber + bestIndividualsNumber; i < populationSize; i++)
        {
            //Debug.Log(i, SelectionOneElement());
            tmp.SetColumn(i, SelectionOneElement());
        }
        //Debug.Log("Before Permu " + tmp);
        int[] p = Combinatorics.GeneratePermutation(populationSize);
        //Debug.Log(tmp);
        tmp.PermuteColumns(new Permutation(p));
        //Debug.Log(tmp);
        //Debug.Log("After Permu " + tmp);
        //tmp.CopyTo(population); 
        population = tmp;
    }

    /*public void CrossoverSliceTwoIndividuals(int idFirst, int idSecond, int sliceIndexA = 0, int sliceIndexB = 0)
    {
        if(sliceIndexA == 0 && sliceIndexB == 0)
        {
            sliceIndexA = rndGenerator.Next(1, individualSize - 1);
            sliceIndexB = rndGenerator.Next(1, individualSize - 1);
        }

        if(sliceIndexA > sliceIndexB)
        {
            int tmp = sliceIndexA;
            sliceIndexA = sliceIndexB;
            sliceIndexB = tmp;
        }

        //Debug.Log("Slice " + sliceIndexA + "---" + sliceIndexB);

        Matrix<float> sliceA = population.SubMatrix(0, sliceIndexA, idFirst, 1);
        Matrix<float> sliceB = population.SubMatrix(sliceIndexB, individualSize - sliceIndexB, idFirst, 1);

        population.SetSubMatrix(0, sliceIndexA, idFirst, 1, population.SubMatrix(0, sliceIndexA, idSecond, 1));
        population.SetSubMatrix(sliceIndexB, individualSize - sliceIndexB, idFirst, 1, population.SubMatrix(sliceIndexB, individualSize - sliceIndexB, idSecond, 1));
        population.SetSubMatrix(0, sliceIndexA, idSecond, 1, sliceA);
        population.SetSubMatrix(sliceIndexB, individualSize - sliceIndexB, idSecond, 1, sliceB);
        //Debug.Log("After Pop " + population);
    }*/

    public void CrossoverSliceTwoIndividuals(int idFirst, int idSecond, int sliceIndexA = 0, int sliceIndexB = 0)
    {
        if (sliceIndexA == 0 && sliceIndexB == 0)
        {
            sliceIndexA = rndGenerator.Next(1, individualSize - 1);
            sliceIndexB = rndGenerator.Next(1, individualSize - 1);
        }

        if (sliceIndexA > sliceIndexB)
        {
            int tmp = sliceIndexA;
            sliceIndexA = sliceIndexB;
            sliceIndexB = tmp;
        }

        //Debug.Log("Slice " + sliceIndexA + "---" + sliceIndexB);

        Vector<float> sliceA = population.Column(idFirst, 0 ,sliceIndexA);
        Vector<float> sliceB = population.Column(idFirst, sliceIndexB, individualSize - sliceIndexB);

        population.SetColumn(idFirst, 0, sliceIndexA, population.Column(idSecond, 0, sliceIndexA));
        population.SetColumn(idFirst, sliceIndexB, individualSize - sliceIndexB, population.Column(idSecond, sliceIndexB, individualSize - sliceIndexB));
        population.SetColumn(idSecond, 0, sliceIndexA, sliceA);
        population.SetColumn(idSecond, sliceIndexB, individualSize - sliceIndexB, sliceB);
        //Debug.Log("After Pop " + population);
    }

    public void CrossOver()
    {
        for(int i = 0; i <populationSize - 1; i = i + 2)
        {
            CrossoverSliceTwoIndividuals(i, i + 1);
        }
    }

    public Vector<float> GetIndividual(int i)
    {
        return population.Column(i);
    }

    public void Evaluation(Vector<float> externalEvaluations, string type)
    {
        evaluations = externalEvaluations;
        sumEvaluations = evaluations.Sum();
        if (type == "max")
        {
            bestIndividualIndex = evaluations.MaximumIndex();
        }else if(type == "min")
        {
            bestIndividualIndex = evaluations.MinimumIndex();
        }else
        {
            throw new System.ArgumentException("Not a good value for type");
        }
        bestIndividual = population.Column(bestIndividualIndex);
        bestScore = evaluations[bestIndividualIndex];
        //var M = Matrix<float>.Build;
        

        //Debug.Log(evaluations);
    }

    public Vector<float> GetBestIndividual()
    {
        return bestIndividual;
    }

    public int GetBestIndividualIndex()
    {
        return bestIndividualIndex;
    }

    float MutationOneElement(float value)
    {
        
        float draw = (float)ContinuousUniform.Sample(rndGenerator, 0.0f, 1.0f);
        
        if(draw <= mutationRate)
        {
            return (float)ContinuousUniform.Sample(rndGenerator, -1.0f, 1.0f);
        }
        return value;
       
    }

    public void Mutation()
    {
        population.MapInplace(MutationOneElement);
    }

    public Vector<float> Run(Vector<float> externalEvaluations)
    {

        Evaluation(externalEvaluations, "max");
        Debug.Log("Best Score : " + bestScore + " | Index : "+ bestIndividualIndex);
        Selection();
        CrossOver();
        Mutation();
        generation += 1;

        return bestIndividual;
    }
}
