using UnityEngine;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour
{
    public GameObject carPrefab;  // Car prefab to instantiate
    public Transform startPosition;  // Starting position of the car

    private GeneticAlgorithm geneticAlgorithm;
    private List<GameObject> cars = new List<GameObject>();

    [SerializeField] private int currentGeneration = 1;  // Display generation in Inspector

    void Start()
    {
        int[] layers = { 5, 8, 1 };  // 5 inputs (sensors), 8 hidden neurons, 1 output (rotation)
        geneticAlgorithm = new GeneticAlgorithm(10, layers);  // 10 cars per generation
        InitializeGeneration();
    }

    void InitializeGeneration()
    {
        foreach (var car in cars)
        {
            Destroy(car);
        }
        cars.Clear();

        foreach (var nn in geneticAlgorithm.population)
        {
            GameObject car = Instantiate(carPrefab, startPosition.position, Quaternion.Euler(0, 0, -90));  // Set rotation to -90 degrees on Z-axis
            car.GetComponent<CarController>().NeuralNetwork = nn;  // Assign neural network
            cars.Add(car);
        }
    }

    void Update()
    {
        if (AllCarsCrashed())
        {
            // Assign fitness to each neural network
            for (int i = 0; i < cars.Count; i++)
            {
                float carFitness = cars[i].GetComponent<CarController>().GetFitness();
                geneticAlgorithm.population[i].fitness = carFitness;  // Transfer fitness
            }

            // Evolve population and move to the next generation
            geneticAlgorithm.Evolve();
            currentGeneration++;  // Increment generation counter
            InitializeGeneration();
        }
    }


    bool AllCarsCrashed()
    {
        foreach (var car in cars)
        {
            if (!car.GetComponent<CarController>().IsCrashed)
            {
                return false;
            }
        }
        return true;
    }
}
