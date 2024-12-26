using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm
{
    public List<NeuralNetwork> population;
    private int populationSize;
    private int[] layers;
    private float mutationRate = 0.3f;

    public GeneticAlgorithm(int populationSize, int[] layers)
    {
        this.populationSize = populationSize;
        this.layers = layers;
        InitializePopulation();
    }

    void InitializePopulation()
    {
        population = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            population.Add(new NeuralNetwork(layers));
        }
    }

    // Evolve the population by selecting the best, breeding, and mutating
    public void Evolve()
    {
        // Sort by fitness (descending order)
        population = population.OrderByDescending(nn => nn.fitness).ToList();
        BreedNewPopulation();
    }

    // Create new generation by crossover and mutation
    void BreedNewPopulation()
    {
        List<NeuralNetwork> newPopulation = new List<NeuralNetwork>();

        // Keep the top 20% without modification
        int topPerformers = Mathf.RoundToInt(populationSize * 0.2f);
        for (int i = 0; i < topPerformers; i++)
        {
            Debug.Log(population[i].fitness);
            newPopulation.Add(population[i]);  // Add top performers directly
        }

        // Breed next generation through crossover
        for (int i = topPerformers; i < populationSize; i++)
        {
            NeuralNetwork parent1 = population[Random.Range(0, topPerformers)];
            NeuralNetwork parent2 = population[Random.Range(0, topPerformers)];
            NeuralNetwork child = parent1.Crossover(parent2);
            child.Mutate(mutationRate);  // Mutate with a chance
            newPopulation.Add(child);
        }

        population = newPopulation;
    }
}
