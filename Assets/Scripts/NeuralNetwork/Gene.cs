using UnityEngine;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
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
    //private int[] permutations;


    public float bestScore;
    public int generation;

    public Gene(int seed, int populationSize, int individualSize, float mutationRate = 0.1f, float randomIndividualsRate = 0.05f, float bestIndividualsRate = 0.05f)
    {
        this.bestScore = 1000000000000.0f;
        this.generation = 1;
        this.seed = seed;
        this.populationSize = populationSize;
        this.individualSize = individualSize;
        //UnityEngine.Random.InitState(seed);

        population = Matrix<float>.Build.Random(individualSize, populationSize);//, seed);
        evaluations = Vector<float>.Build.Dense(populationSize);
        sumEvaluations = 0.0f;
        //Debug.Log("Population " + population);


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
        float pick = UnityEngine.Random.Range(0.0f, sumEvaluations);
        float current = 0.0f;
        //Vector<float> tmp;

        for(int i = 0; i < populationSize; i++)
        {
            current += evaluations[i];
            //Debug.Log()
            if(current >= pick)
            {
                
                return  population.Column(i, 0, individualSize);
            }
        }
        return null;
    }


    public void Selection()
    {
        int randomIndividualsNumber = Mathf.RoundToInt(randomIndividualsRate * populationSize);
        int bestIndividualsNumber = Mathf.RoundToInt(bestIndividualsRate * populationSize);
        Matrix<float> tmp = Matrix<float>.Build.Dense(individualSize, populationSize);//, seed);

        for(int i = 0; i < randomIndividualsNumber; i++)
        {
            tmp.SetColumn(i, Vector<float>.Build.Dense(individualSize));
        }

        for (int i = 0; i < bestIndividualsNumber; i++)
        {
            tmp.SetColumn(i, bestIndividual);
        }

        for (int i = randomIndividualsNumber + bestIndividualsNumber; i < populationSize - randomIndividualsNumber - bestIndividualsNumber; i++)
        {
            //Debug.Log(i, SelectionOneElement());
            tmp.SetColumn(i, SelectionOneElement());
        }
        tmp.PermuteColumns(new Permutation(Combinatorics.GeneratePermutation(populationSize)));
        population = tmp; 
    }

    public void CrossoverSliceTwoIndividuals(int idFirst, int idSecond)
    {
        int sliceIndexA = UnityEngine.Random.Range(1, individualSize - 1);
        int sliceIndexB = UnityEngine.Random.Range(1, individualSize - 1);

        if(sliceIndexA > sliceIndexB)
        {
            int tmp = sliceIndexA;
            sliceIndexA = sliceIndexB;
            sliceIndexB = tmp;
        }

        Matrix<float> sliceA = population.SubMatrix(0, sliceIndexA, idFirst, 1);
        Matrix<float> sliceB = population.SubMatrix(sliceIndexB, individualSize - sliceIndexB, idFirst, 1);

        population.SetSubMatrix(0, sliceIndexA, idFirst, 1, population.SubMatrix(0, sliceIndexA, idSecond, 1));
        population.SetSubMatrix(sliceIndexB, individualSize - 1 - sliceIndexB, idFirst, 1, population.SubMatrix(sliceIndexB, individualSize - 1 - sliceIndexB, idSecond, 1));
        population.SetSubMatrix(0, sliceIndexA, idSecond, 1, sliceA);
        population.SetSubMatrix(sliceIndexB, individualSize - 1 - sliceIndexB, idSecond, 1, sliceB);
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

    public void Evaluation(Vector<float> externalEvaluations)
    {
        evaluations = externalEvaluations;
        sumEvaluations = evaluations.Sum();
        bestIndividualIndex = evaluations.MaximumIndex();
        bestIndividual = population.Column(bestIndividualIndex);
        bestScore = evaluations[bestIndividualIndex];
        //var M = Matrix<float>.Build;
        

        //Debug.Log(evaluations);
    }


    float MutationOneElement(float value)
    {
        float draw = UnityEngine.Random.Range(0.0f, 1.0f);
        if(draw <= mutationRate)
        {
            return UnityEngine.Random.Range(-1.0f, 1.0f);
        }
        return value;
       
    }

    public void Mutation()
    {
        population.MapInplace(MutationOneElement);
    }

    public Vector<float> Run(Vector<float> externalEvaluations)
    {

        Evaluation(externalEvaluations);
        Debug.Log("Best Score : " + bestScore);
        Selection();
        CrossOver();
        Mutation();
        generation += 1;

        return bestIndividual;
    }
}
