using UnityEngine;

public class NeuralNetwork
{
    private int[] layers;  // Number of neurons in each layer
    private float[][] neurons;  // Neuron values by layer
    private float[][][] weights;  // Weights between neurons

    public float fitness = 0;  // Fitness score for evaluation

    public NeuralNetwork(int[] layers)
    {
        this.layers = layers;
        InitializeNeurons();
        InitializeWeights();
    }

    // Initialize neurons (layer structure)
    void InitializeNeurons()
    {
        neurons = new float[layers.Length][];
        for (int i = 0; i < layers.Length; i++)
        {
            neurons[i] = new float[layers[i]];  // Create neuron arrays for each layer
        }
    }

    // Initialize weights with random values
    void InitializeWeights()
    {
        weights = new float[layers.Length - 1][][];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = new float[layers[i + 1]][];
            for (int j = 0; j < layers[i + 1]; j++)
            {
                weights[i][j] = new float[layers[i]];
                for (int k = 0; k < layers[i]; k++)
                {
                    weights[i][j][k] = Random.Range(-1f, 1f);  // Random initialization between -1 and 1
                }
            }
        }
    }

    // Forward propagation through the neural network
    public float[] FeedForward(float[] inputs)
    {
        // Safeguard to avoid index errors
        if (inputs.Length != neurons[0].Length)
        {
            Debug.LogError($"Input size mismatch: Expected {neurons[0].Length}, but got {inputs.Length}");
            return new float[layers[layers.Length - 1]];  // Return empty array if mismatch
        }

        // Copy inputs to the first layer (input layer)
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        // Propagate forward through each layer
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i]; j++)
            {
                float sum = 0f;
                for (int k = 0; k < layers[i - 1]; k++)
                {
                    sum += weights[i - 1][j][k] * neurons[i - 1][k];  // Weighted sum from previous layer
                }
                neurons[i][j] = Sigmoid(sum);  // Activation function
            }
        }

        // Return output layer values
        return neurons[layers.Length - 1];
    }

    // Crossover between two neural networks to create offspring
    public NeuralNetwork Crossover(NeuralNetwork partner)
    {
        NeuralNetwork child = new NeuralNetwork(layers);
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    child.weights[i][j][k] = Random.value > 0.5f ? this.weights[i][j][k] : partner.weights[i][j][k];
                }
            }
        }
        return child;
    }

    // Mutate weights by randomly adjusting them
    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    if (Random.value < mutationRate)
                    {
                        weights[i][j][k] += Random.Range(-0.5f, 0.5f);  // Small random mutation
                    }
                }
            }
        }
    }

    // Sigmoid activation function
    float Sigmoid(float x)
    {
        return 1f / (1f + Mathf.Exp(-x));
    }
}
