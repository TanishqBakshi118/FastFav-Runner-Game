using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    public GameObject[] chunkPrefabs;
    public float baseScrollSpeed = 50f;
    public float chunkLength = 100f;
    public int numberOfChunks = 6;

    [Header("Scoring")]
    public TextMeshProUGUI scoreText;

    public float score = 0f;
    [HideInInspector] public float currentScrollSpeed;

    [Header("Obstacles")]
    public GameObject rockPrefab;    // For Water Lane (Left)
    public GameObject barrierPrefab; // For Road Lane (Center)
    public GameObject blimpPrefab;   // For Air (Any lane, high Y)
    [Range(0f, 1f)] public float obstacleSpawnChance = 0.6f;

    [Header("Pickups")]
    public GameObject powerUpPrefab;
    [Range(0f, 1f)]
    public float powerUpSpawnChance = 0.1f; // 10% chance to spawn

    private List<GameObject> activeChunks = new List<GameObject>();
    private int lastHazardIndex = -1;
    private bool spawnSafeChunkNext = false;

    [Header("Game State")]
    public bool isGameOver = false;

    void Start()
    {
        currentScrollSpeed = baseScrollSpeed;

        for (int i = 0; i < numberOfChunks; i++)
        {
            if (i < 3) SpawnSpecificChunk(0);
            else SpawnPacedChunk();
        }
    }

    void Update()
    {
        // STOP everything if the game is over
        if (isGameOver) return;

        // 1. Calculate Score
        score += currentScrollSpeed * Time.deltaTime;
        if (scoreText != null) scoreText.text = "SCORE: " + Mathf.FloorToInt(score);

        // Gradually increase the target speed over time (0.5 units per second)
        baseScrollSpeed += Time.deltaTime * 0.5f;

        // 2. Recover speed gradually if penalized (AND accelerate to match new base speed)
        if (currentScrollSpeed < baseScrollSpeed)
        {
            currentScrollSpeed += Time.deltaTime * 10f;
        }

        // 3. Move Chunks
        foreach (GameObject chunk in activeChunks)
        {
            chunk.transform.Translate(Vector3.back * currentScrollSpeed * Time.deltaTime);
        }

        // 4. Recycle Chunks
        if (activeChunks[0].transform.position.z < -chunkLength)
        {
            GameObject oldChunk = activeChunks[0];
            activeChunks.RemoveAt(0);
            Destroy(oldChunk);
            SpawnPacedChunk();
        }
    }

    void SpawnSpecificChunk(int index)
    {
        float spawnPositionZ = 0f;
        if (activeChunks.Count > 0)
        {
            spawnPositionZ = activeChunks[activeChunks.Count - 1].transform.position.z + chunkLength;
        }

        GameObject chunk = Instantiate(chunkPrefabs[index], new Vector3(0, 0, spawnPositionZ), Quaternion.identity);
        activeChunks.Add(chunk);

        // Check if we should spawn obstacles, and flag whether this is a canyon (index == 2)
        if (activeChunks.Count > 3 && Random.value <= obstacleSpawnChance)
        {
            bool isCanyon = (index == 2);
            SpawnObstaclesOnChunk(chunk.transform, isCanyon);
        }
    }

    void SpawnPacedChunk()
    {
        if (spawnSafeChunkNext)
        {
            SpawnSpecificChunk(0);
            spawnSafeChunkNext = false;
        }
        else
        {
            int randomHazard = Random.Range(1, chunkPrefabs.Length);
            while (randomHazard == lastHazardIndex && chunkPrefabs.Length > 2)
            {
                randomHazard = Random.Range(1, chunkPrefabs.Length);
            }

            lastHazardIndex = randomHazard;
            SpawnSpecificChunk(randomHazard);
            spawnSafeChunkNext = true;
        }
    }

    void SpawnObstaclesOnChunk(Transform chunkTransform, bool isCanyon)
    {
        // Spawns between 3 and 5 obstacles per chunk
        int obstaclesToSpawn = Random.Range(3, 6);

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            // 0 = Water, 1 = Road, 2 = Dirt, 3 = Air
            int laneType = Random.Range(0, 4);

            // --- CANYON OVERRIDE ---
            // If it is a canyon chunk, force the spawn to the Air lane
            if (isCanyon)
            {
                laneType = 3;
            }

            GameObject obstacleToSpawn = null;
            Vector3 localSpawnPos = Vector3.zero;

            // Pick a random spot along the Z length of the chunk
            float randomZ = Random.Range(10f, chunkLength - 10f);

            if (laneType == 0) // Left Lane: Water
            {
                obstacleToSpawn = rockPrefab;
                localSpawnPos = new Vector3(-10f, 1.5f, randomZ);
            }
            else if (laneType == 1) // Center Lane: Road
            {
                obstacleToSpawn = barrierPrefab;
                localSpawnPos = new Vector3(0f, 1f, randomZ);
            }
            else if (laneType == 2) // Right Lane: Dirt
            {
                obstacleToSpawn = rockPrefab;
                localSpawnPos = new Vector3(10f, 1.5f, randomZ);
            }
            else // Air
            {
                obstacleToSpawn = blimpPrefab;
                localSpawnPos = new Vector3(Random.Range(-10f, 10f), Random.Range(8f,12f), randomZ);
            }

            // --- POWERUP LOGIC ---
            if (powerUpPrefab != null && Random.value <= powerUpSpawnChance)
            {
                obstacleToSpawn = powerUpPrefab;
            }

            // Instantiate and attach to the chunk
            GameObject obs = Instantiate(obstacleToSpawn, chunkTransform);
            obs.transform.localPosition = localSpawnPos;
        }
    }
}