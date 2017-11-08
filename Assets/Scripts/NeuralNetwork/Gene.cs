using UnityEngine;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.IO;
using Lexmou.Utils;

namespace Lexmou.MachineLearning.Evolutionary
{

    public class Genetic
    {
        private string path;
        private Matrix<float> population;
        private int seed;
        public int populationSize;
        public int individualSize;
        private float mutationRate;
        private Vector<float> evaluations;
        public float sumEvaluations;
        private float randomIndividualsRate;
        private float bestIndividualsRate;
        private int bestIndividualIndex;
        private Vector<float> bestIndividual;
        public SystemRandomSource rndGenerator;
        float initialValueWeights;

        public Tuple<float, float> meanStd;
        public float median;

        public float bestScore;
        public int generation;
        ContinuousUniform distribution;
        ContinuousUniform subDistribution;
        ContinuousUniform drawDistribution;
        public StreamWriter writer;
        Vector<float> sliceA;
        Vector<float> sliceB;
        Matrix<float> tmp;
        float emptyRate;
        bool[] distEmpty;
        bool save;
         
        public Matrix<float> GetPopulation()
        {
            return population;
        }

        public Genetic(int seed, SystemRandomSource rndGenerator, int populationSize, int individualSize, float initialValueWeights, float mutationRate = 0.1f, float randomIndividualsRate = 0.05f, float bestIndividualsRate = 0.05f, float emptyRate = 0.0f, string path = "",bool save = true)
        {
            this.save = save;
            tmp = Matrix<float>.Build.Dense(individualSize, populationSize);
            this.path = path;
            this.bestScore = 0.0f;
            this.generation = 1;
            this.seed = seed;
            this.initialValueWeights = initialValueWeights;
            if (rndGenerator != null)
            {
                this.rndGenerator = rndGenerator;
            }
            else
            {
                this.rndGenerator = new SystemRandomSource(seed);
            }

            distribution = new ContinuousUniform(-initialValueWeights, initialValueWeights, rndGenerator);
            subDistribution = new ContinuousUniform(-0.01, 0.01, rndGenerator);
            drawDistribution = new ContinuousUniform(0.0f, 1.0f, rndGenerator);
            this.populationSize = populationSize;
            this.individualSize = individualSize;

            this.population = Matrix<float>.Build.Random(individualSize, populationSize, distribution);
            for(int i =0; i < populationSize;i++)
            {
                distEmpty = Combinatorics.GenerateCombination(individualSize, Mathf.RoundToInt(emptyRate * individualSize), rndGenerator);
                //Debug.Log(dist);
                for (int j = 0; j < individualSize; j++)
                {
                    if(distEmpty[j])
                    {
                        //Debug.Log(dist[j]);
                        //population[j,i] = (float)subDistribution.Sample();
                        population[j, i] = 0;
                    }
                    
                }
            }

            //Debug.Log(population.ToString("G40"));

            this.evaluations = Vector<float>.Build.Dense(populationSize);
            this.sumEvaluations = 0.0f;


            this.mutationRate = mutationRate;
            this.randomIndividualsRate = randomIndividualsRate;
            this.bestIndividualsRate = bestIndividualsRate;
            this.emptyRate = emptyRate;
            if (save)
            {
                writer = UIO.CreateStreamWriter(GeneratePath(), "GeneticResults.csv", false);
                UIO.WriteLine(writer, "Generation;Best;Mean;Std Deviation;Median");
            }


        }

        public Vector<float> SelectionOneElement()
        {
            float pick = (float)ContinuousUniform.Sample(rndGenerator, 0.0d, (double)sumEvaluations);
            float current = 0.0f;

            for (int i = 0; i < populationSize; i++)
            {
                current += evaluations[i];
                if (current >= pick)
                {
                    return population.Column(i);
                }
            }
            return null;
        }


        public void Selection()
        {
            int randomIndividualsNumber = Mathf.RoundToInt(randomIndividualsRate * populationSize);
            int bestIndividualsNumber = Mathf.RoundToInt(bestIndividualsRate * populationSize);
            

            for (int i = 0; i < bestIndividualsNumber; i++)
            {
                tmp.SetColumn(i, bestIndividual);
            }

            for (int i = bestIndividualsNumber; i < bestIndividualsNumber + randomIndividualsNumber; i++)
            {
                tmp.SetColumn(i, Vector<float>.Build.Random(individualSize, distribution));
                distEmpty = Combinatorics.GenerateCombination(individualSize, Mathf.RoundToInt(emptyRate * individualSize), rndGenerator);
                //Debug.Log(dist);
                for (int j = 0; j < individualSize; j++)
                {
                    if (distEmpty[j])
                    {
                        //Debug.Log(dist[j]);
                        population[j, i] = 0;
                    }
                }
            }

            for (int i = randomIndividualsNumber + bestIndividualsNumber; i < populationSize; i++)
            {
                tmp.SetColumn(i, SelectionOneElement());
            }

            int[] p = Combinatorics.GeneratePermutation(populationSize);

            tmp.PermuteColumns(new Permutation(p));

            tmp.CopyTo(population);
        }


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

            sliceA = population.Column(idFirst, 0, sliceIndexA);
            sliceB = population.Column(idFirst, sliceIndexB, individualSize - sliceIndexB);

            population.SetColumn(idFirst, 0, sliceIndexA, population.Column(idSecond, 0, sliceIndexA));
            population.SetColumn(idFirst, sliceIndexB, individualSize - sliceIndexB, population.Column(idSecond, sliceIndexB, individualSize - sliceIndexB));
            population.SetColumn(idSecond, 0, sliceIndexA, sliceA);
            population.SetColumn(idSecond, sliceIndexB, individualSize - sliceIndexB, sliceB);
        }

        public void CrossOver()
        {
            for (int i = 0; i < populationSize - 1; i = i + 2)
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
            }
            else if (type == "min")
            {
                bestIndividualIndex = evaluations.MinimumIndex();
            }
            else
            {
                throw new System.ArgumentException("Not a good value for type");
            }
            bestIndividual = population.Column(bestIndividualIndex);
            bestScore = evaluations[bestIndividualIndex];
            meanStd = UVector.BuildMeanStdVariation(evaluations);
            median = UVector.BuildMedian(evaluations);
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

            float draw = (float)drawDistribution.Sample();

            if (draw <= mutationRate)
            {
                return (float) distribution.Sample();
            }
            return value;

        }

        public void Mutation()
        {
            population.MapInplace(MutationOneElement);
        }
        
        string GeneratePath()
        {
            return this.path + "Seed-"+ this.seed + "/";
        }

        public void SaveGeneration()
        {
                UIO.SaveFloatArray(GeneratePath() + generation, "population.gene", population.ToArray());
                UIO.SaveFloatArray(GeneratePath() + generation, "best.gene", GetBestIndividual().ToArray());
        }

        public void LoadGeneration(int generationIndex, float[,] floatArr)
        {
            UIO.LoadFloatArray(GeneratePath() + generationIndex , "/population.gene", floatArr);
            population = Matrix<float>.Build.DenseOfArray(floatArr);
        }

        public void LoadBest(int generationIndex, float[] floatArr)
        {
            UIO.LoadFloatArray(GeneratePath() + generationIndex, "/best.gene", floatArr);

        }


        public string Run(Vector<float> externalEvaluations, float theoricBestScore, int intervalSave = 0)
        {
            string generationInfos;

            Evaluation(externalEvaluations, "max");
            if (save)
                UIO.WriteLine(writer, generation +";" +bestScore+";"+meanStd.Item1+";"+meanStd.Item2+";"+median);
            if (intervalSave > 0 && generation % intervalSave == 0)
            {
                Debug.Log("Save in Progress");
                SaveGeneration();
            }
            generationInfos = generation + "  | Best Score : " + bestScore + "/" + theoricBestScore + " | Index : " + bestIndividualIndex
                                + "\r\n" + "Score Mean : " + (sumEvaluations/populationSize);
            //Debug.Log(UMatrix.Make2DMatrix(bestIndividual, 4, 10).ToString("G40"));
            Selection();
            CrossOver();
            Mutation();
            generation += 1;

            return generationInfos;
        }

        ~Genetic()
        {
            if(save)
                UIO.CloseStreamWriter(writer);
        }
    }
}
