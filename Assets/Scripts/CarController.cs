using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 200f;
    public float[] sensorDistances;
    public Transform[] sensors;

    public NeuralNetwork NeuralNetwork { get; set; }
    public bool IsCrashed { get; private set; }

    [SerializeField] private float fitness = 0f;
    private int checkpointsPassed = 0;
    private Transform nextCheckpoint;
    private float lastDistanceToCheckpoint;

    private List<Transform> checkpointList = new List<Transform>();
    private Transform lastCheckpointPassed;

    void Start()
    {   
        InitializeCheckpoints();

        lastCheckpointPassed = checkpointList[0];
        checkpointsPassed++;
        fitness = 0f;
        IsCrashed = false;
        checkpointsPassed = 0;

        FindNextCheckpoint();
    }

    void Update()
    {
        if (!IsCrashed)
        {
            Move();
            Rotate();
            UpdateSensors();
        }
    }

    void Move()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void Rotate()
    {
        float[] output = NeuralNetwork.FeedForward(sensorDistances);
        float rotationInput = output[0] - 0.5f;
        transform.Rotate(Vector3.forward, rotationInput * rotationSpeed * Time.deltaTime);
    }

    void UpdateSensors()
    {
        int carLayerMask = LayerMask.GetMask("Car");
        int checkpointLayerMask = LayerMask.GetMask("Checkpoint");

        int ignoreLayers = carLayerMask | checkpointLayerMask;

        for (int i = 0; i < sensors.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(sensors[i].position, sensors[i].up, 10f, ~ignoreLayers);
            sensorDistances[i] = hit.collider ? hit.distance : 10f;
        }
    }

    // Draw sensors in Scene and Game View
    void OnDrawGizmos()
    {
        if (sensors == null || IsCrashed) 
            return;  // Don't draw gizmos if the car has crashed or sensors don't exist

        Gizmos.color = Color.green;

        for (int i = 0; i < sensors.Length; i++)
        {
            Vector3 sensorEndPosition = sensors[i].position + sensors[i].up * sensorDistances[i];
            Gizmos.DrawLine(sensors[i].position, sensorEndPosition);
        }
    }
    
    // Detect when a car passes through a checkpoint
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint") && other.transform == nextCheckpoint)
        {
            checkpointsPassed++;

            lastCheckpointPassed = nextCheckpoint;  // Store the last checkpoint passed
            FindNextCheckpoint();
        }
    }

    // Initialize the list of checkpoints based on hierarchy order
    void InitializeCheckpoints()
    {
        GameObject checkpointParent = GameObject.Find("Checkpoints");
        if (checkpointParent != null)
        {
            foreach (Transform checkpoint in checkpointParent.transform)
            {
                if (checkpoint.CompareTag("Checkpoint"))
                {
                    checkpointList.Add(checkpoint);
                }
            }
        }
    }

    // Locate the next checkpoint in sequence
    void FindNextCheckpoint()
    {
        if (checkpointList.Count > 0)
        {
            nextCheckpoint = checkpointList[(checkpointsPassed + 1) % checkpointList.Count];
            lastDistanceToCheckpoint = Vector2.Distance(transform.position, nextCheckpoint.position);
        }
    }

   // Calculate fitness when the car crashes
    void OnCollisionEnter2D(Collision2D collision)
    {
        IsCrashed = true;

        // Reduce car opacity by lowering sprite alpha
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color carColor = sr.color;
            carColor.a = 0.3f;  // 30% opacity
            sr.color = carColor;
        }

        // Disable raycast sensors by turning off their GameObjects
        foreach (Transform sensor in sensors)
        {
            sensor.gameObject.SetActive(false);  // Disable each sensor
        }

        // Fitness calculation after crash
        if (lastCheckpointPassed != null)
        {
            float crashDistance = Vector2.Distance(transform.position, nextCheckpoint.position);

            // Calculate fitness by tracing back through checkpoints
            int lastCheckpointIndex = checkpointList.IndexOf(lastCheckpointPassed);
            float totalDistance = 0f;

            for (int i = lastCheckpointIndex; i > 0; i--)
            {
                float segmentDistance = Vector2.Distance(checkpointList[i].position, checkpointList[i - 1].position);
                totalDistance += segmentDistance;
            }

            // Add progress between last checkpoint and next checkpoint
            float nextSegmentDistance = Vector2.Distance(lastCheckpointPassed.position, nextCheckpoint.position);
            totalDistance += nextSegmentDistance - crashDistance;

            fitness += totalDistance;  // Add distance to fitness
        }
    }


    // Return the final fitness score for selection
    public float GetFitness()
    {
        return fitness;
    }
}
