using UnityEngine;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;


namespace Lexmou.MachineLearning.Evolutionary
{

    public class Genetic
    {

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
        float initialValueWeights;

        public float bestScore;
        public int generation;
        ContinuousUniform distribution;
        public Matrix<float> GetPopulation()
        {
            return population;
        }

        public Genetic(int seed, SystemRandomSource rndGenerator, int populationSize, int individualSize, float initialValueWeights, float mutationRate = 0.1f, float randomIndividualsRate = 0.05f, float bestIndividualsRate = 0.05f)
        {
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

            this.populationSize = populationSize;
            this.individualSize = individualSize;

            this.population = Matrix<float>.Build.Random(individualSize, populationSize, distribution);
            this.evaluations = Vector<float>.Build.Dense(populationSize);
            this.sumEvaluations = 0.0f;


            this.mutationRate = mutationRate;
            this.randomIndividualsRate = randomIndividualsRate;
            this.bestIndividualsRate = bestIndividualsRate;

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
            Matrix<float> tmp = Matrix<float>.Build.Dense(individualSize, populationSize);

            for (int i = 0; i < bestIndividualsNumber; i++)
            {
                tmp.SetColumn(i, bestIndividual);
            }

            for (int i = bestIndividualsNumber; i < bestIndividualsNumber + randomIndividualsNumber; i++)
            {
                tmp.SetColumn(i, Vector<float>.Build.Random(individualSize, distribution));
            }

            for (int i = randomIndividualsNumber + bestIndividualsNumber; i < populationSize; i++)
            {
                tmp.SetColumn(i, SelectionOneElement());
            }

            int[] p = Combinatorics.GeneratePermutation(populationSize);

            tmp.PermuteColumns(new Permutation(p));

            population = tmp;
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

            Vector<float> sliceA = population.Column(idFirst, 0, sliceIndexA);
            Vector<float> sliceB = population.Column(idFirst, sliceIndexB, individualSize - sliceIndexB);

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

            if (draw <= mutationRate)
            {
                return (float)ContinuousUniform.Sample(rndGenerator, -1.0f, 1.0f);
            }
            return value;

        }

        public void Mutation()
        {
            population.MapInplace(MutationOneElement);
        }

        public string Run(Vector<float> externalEvaluations, float theoricBestScore)
        {
            string generationInfos;
            Evaluation(externalEvaluations, "max");
            generationInfos = generation + "  | Best Score : " + bestScore + "/" + theoricBestScore + " | Index : " + bestIndividualIndex;
            Selection();
            CrossOver();
            Mutation();
            generation += 1;

            return generationInfos;
        }
    }
}
