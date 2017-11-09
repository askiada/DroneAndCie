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
    /**
     * \class Genetic
     * \brief Classic implementation of a genetic algorithm.
     * \details Class Genetic is an "optimized" unity3D implementation of a standard genetic algorithm. The following picture shows the general principle. for more details, Wikipedia is a good start.
     *          ![Genetic Algorithm Scheme](./media/genetic/scheme.png)
     */

    public class Genetic
    {
        /**
         * \brief Seed number to generate a random sequence with a random generator #rndGenerator
         */ 
        private int seed;
        /**
         * \brief Relative path to save the results
         */
        private string path;
        /**
         * \brief Initial value range of the distribution to build the population
         */
        private float initialValueWeights;
        /**
         * \brief Matrix of the population. Each Column is an individual.
         */
        private Matrix<float> population;
        /**
         * \brief Random rate. At each generation, add \f$ populationSize \times  randomIndividualsRate\f$ individuals.
         */
        private float randomIndividualsRate;
        /**
         * \brief Best individual rate. At each generation, add \f$ populationSize \times  bestIndividualsRate\f$ times the best individual from the last generation.
         */
        private float bestIndividualsRate;
        /**
         * \brief Mutation rate. Set the probability to randomly change each element of an individual.
         */
        private float mutationRate;
        /**
         * \brief Empty value rate. The creation of a new individual ensure that \f$ individualSize \times emptyRate\f$ elements are zero.
         */
        private float emptyRate;



        private Vector<float> evaluations;
        private int bestIndividualIndex;
        private Vector<float> bestIndividual;

        private ContinuousUniform distribution;
        private ContinuousUniform subDistribution;
        private ContinuousUniform drawDistribution;

        private Vector<float> sliceA;
        private Vector<float> sliceB;
        private Matrix<float> tmp;
        private bool[] distEmpty;
        private bool save;


        /**
         * \brief Random Generator. Allow the generation of a repeatable random number sequence for each seed value.
         */
        public SystemRandomSource rndGenerator;
        /**
         * \brief Generation counter. Start at 1.
         */
        public int generation;
        /**
         * \brief Population size
         * \details The number of individual by generation is very important for the diversity. There is many ressources online about this.
         *          This class is not perfect and will only work with a even number. Condition never checked.
         *  \todo Odd number compatibility (Problem with Selection())          
         */
        public int populationSize;
        /**
         * \brief Size of each individual
         */
        public int individualSize;
        /**
         * \brief Best score over the population during the last generation.
         */
        public float bestScore;
        /**
         * \brief Sum of the individuals scores
         */
        public float sumEvaluations;
        /**
         * \brief Mean and standard deviation of the score over the population
         */
        public Tuple<float, float> meanStd;
        /**
         * \brief Median score over the population
         */
        public float median;
        /**
         * \brief StreamWriter to save and load results
         */
        public StreamWriter writer;


        /**
         * 
         */

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
                for (int j = 0; j < individualSize; j++)
                {
                    if(distEmpty[j])
                    {
                        population[j, i] = 0.000f;
                    }
                    
                }
            }

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

        /**
         * \brief Fitness proportionate selection of one element
         * \details If \f$ f_{i} \f$ is the fitness of individual \f$ i \f$ in the population, its probability of being selected is \f$  p_i = \frac{f_i}{\Sigma_{j=1}^{N} f_j} \f$, where \f$ N \f$ is the number of individuals in the population.
         * \return A \e MathNet.numerics.Vector of the selected individual 
         */

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
                for (int j = 0; j < individualSize; j++)
                {
                    if (distEmpty[j])
                    {
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

        /**
         * \brief Two-point crossover
         * \details Two-point crossover calls for two points to be selected on the parent organism. Everything between the two points is swapped between the parent organisms, rendering two child organisms
         *          ![Genetic Algorithm Crossover Two-point](./media/genetic/crossover.png)<!-- .element height="50%" width="50%" -->
         *  \param[in]  idFirst Index of the first parent
         *  \param[in]  idSecond Index of the second parent
         *  \param[in]  sliceIndexA Index of the first swap point
         *  \param[in]  sliceIndexB Index of the second swap point
         */

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

        /**
         * \brief Complete population crossover
         * \details Need an even population size
         */

        public void CrossOver()
        {
            for (int i = 0; i < populationSize - 1; i = i + 2)
            {
                CrossoverSliceTwoIndividuals(i, i + 1);
            }
        }


        /**
        * \brief Change an element with a probability of #mutationRate
        * \return A \e float representing the value of this element
        */

        public float MutationOneElement(float value)
        {

            float draw = (float)drawDistribution.Sample();

            if (draw <= mutationRate)
            {
                return (float)distribution.Sample();
            }
            return value;

        }

        /**
        * \brief Apply the mutation process over the population
        */

        public void Mutation()
        {
            population.MapInplace(MutationOneElement);
        }


        public string Run(Vector<float> externalEvaluations, float theoricBestScore, int intervalSave = 0)
        {
            string generationInfos;

            Evaluation(externalEvaluations, "max");
            if (save)
                UIO.WriteLine(writer, generation + ";" + bestScore + ";" + meanStd.Item1 + ";" + meanStd.Item2 + ";" + median);
            if (intervalSave > 0 && generation % intervalSave == 0)
            {
                Debug.Log("Save in Progress");
                SaveGeneration();
            }
            generationInfos = generation + "  | Best Score : " + bestScore + "/" + theoricBestScore + " | Index : " + bestIndividualIndex
                                + "\r\n" + "Score Mean : " + (sumEvaluations / populationSize);
            //Debug.Log(UMatrix.Make2DMatrix(bestIndividual, 4, 13).ToString(4,13, "G3"));
            Selection();
            CrossOver();
            Mutation();
            generation += 1;

            return generationInfos;
        }


        public Matrix<float> GetPopulation()
        {
            return population;
        }

        /**
         * \brief Get a specific individual
         * \param[in] i Index of the individual
         * \return A \e MathNet.Numerics.Vector representing the individual
         */

        public Vector<float> GetIndividual(int i)
        {
            return population.Column(i);
        }

        /**
        * \brief Get the best individual of the last generation
        * \return A \e MathNet.Numerics.Vector representing the best individual
        */

        public Vector<float> GetBestIndividual()
        {
            return bestIndividual;
        }

        /**
        * \brief Get the best individual index of the last generation
        * \return An \e int representing the index of the best individual
        */

        public int GetBestIndividualIndex()
        {
            return bestIndividualIndex;
        }



        /**
        * \brief Generate the path where to save/load the results
        * \return \e string Value of the path
        */

        string GeneratePath()
        {
            return this.path + "Seed-" + this.seed + "/";
        }

        

        /**
        * \brief Save the generation
        * \details Save the complete population and the best individual in two files. The path depends on GeneratePath()
        */

        public void SaveGeneration()
        {
                UIO.SaveFloatArray(GeneratePath() + generation, "population.gene", population.ToArray());
                UIO.SaveFloatArray(GeneratePath() + generation, "best.gene", GetBestIndividual().ToArray());
        }

        /**
        * \brief Load the generation
        * \details load the complete population and the best individual in two files. The path depends on GeneratePath()
        */

        public void LoadGeneration(int generationIndex, float[,] floatArr)
        {
            UIO.LoadFloatArray(GeneratePath() + generationIndex , "/population.gene", floatArr);
            population = Matrix<float>.Build.DenseOfArray(floatArr);
        }

        /**
        * \brief Load the best individual
        * \details load the complete population and the best individual in two files. The path depends on GeneratePath()
        */


        public void LoadBest(int generationIndex, float[] floatArr)
        {
            UIO.LoadFloatArray(GeneratePath() + generationIndex, "/best.gene", floatArr);

        }
        ~Genetic()
        {
            if(save)
                UIO.CloseStreamWriter(writer);
        }
    }
}
