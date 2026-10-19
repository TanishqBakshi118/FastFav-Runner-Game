using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerController : MonoBehaviour
{
    public enum VehicleType { Car, Boat, Plane }
    public VehicleType currentVehicle = VehicleType.Car;

    [Header("Dependencies")]
    public WorldManager worldManager;

    [Header("Health System")]
    public int maxHealth = 10;
    private int currentHealth;
    public Image healthFill;

    [Header("Out of Bounds")]
    public float fallDeathY = -5f;

    public GameObject gameOverScreen;

    public TMPro.TextMeshProUGUI finalScoreText;
    public TMPro.TextMeshProUGUI highScoreText;

    [Header("Meshes")]
    public GameObject carMesh;
    public GameObject boatMesh;
    public GameObject planeMesh;

    [Header("Movement Settings")]
    public float lateralSpeed = 20f;
    public float flightSpeedY = 15f;
    public float raycastDistance = 2f;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;

    [Header("Effects")]
    public GameObject hitSparkPrefab;

    [Header("PowerUps")]
    public GameObject glowIndicator;
    private bool isInvincible = false;
    private Coroutine invulnerabilityCoroutine;

    [Header("Audio Settings")]
    public AudioClip hitSound;
    public AudioClip gameOverSound;
    private AudioSource audioSource;

    [Header("Visual Effects")]
    public GameObject swapParticlePrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // Initialize Health
        currentHealth = maxHealth;
        UpdateHealthUI();
        if (gameOverScreen != null) gameOverScreen.SetActive(false);

        SwitchVehicle(VehicleType.Car);
    }

    void Update()
    {
        // Stop logic if already dead
        if (worldManager.isGameOver) return;

        // --- OUT OF BOUNDS CHECK ---
        if (transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchVehicle(VehicleType.Car);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchVehicle(VehicleType.Boat);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchVehicle(VehicleType.Plane);
    }

    void FixedUpdate()
    {
        if (worldManager.isGameOver)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 targetVelocity = rb.linearVelocity;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance);

        if (currentVehicle == VehicleType.Car)
        {
            if (isGrounded && hit.collider.CompareTag("Road"))
                targetVelocity.x = horizontalInput * lateralSpeed;
            else
                targetVelocity.x = Mathf.Lerp(targetVelocity.x, 0, Time.fixedDeltaTime * 5f);
        }
        else if (currentVehicle == VehicleType.Boat)
        {
            if (isGrounded && hit.collider.CompareTag("Water"))
                targetVelocity.x = horizontalInput * lateralSpeed;
            else
                targetVelocity.x = Mathf.Lerp(targetVelocity.x, 0, Time.fixedDeltaTime * 5f);
        }
        else if (currentVehicle == VehicleType.Plane)
        {
            targetVelocity.x = horizontalInput * lateralSpeed;
            targetVelocity.y = verticalInput * flightSpeedY;
        }

        rb.linearVelocity = targetVelocity;
    }

    void LateUpdate()
    {
        if (worldManager.isGameOver) return;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -10f, 10f);
        clampedPosition.z = 0f;
        transform.position = clampedPosition;

        if (transform.position.x <= -10f || transform.position.x >= 10f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, rb.linearVelocity.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (worldManager.isGameOver) return;

        // --- POWERUP LOGIC ---
        if (other.CompareTag("PowerUp"))
        {
            Destroy(other.gameObject);

            if (invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
            }
            invulnerabilityCoroutine = StartCoroutine(InvincibilityRoutine());
            return;
        }

        // --- OBSTACLE LOGIC ---
        if (other.CompareTag("Obstacle"))
        {
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            if (isInvincible)
            {
                if (hitSparkPrefab != null) Instantiate(hitSparkPrefab, other.transform.position, Quaternion.identity);
                Destroy(other.gameObject);
                return;
            }

            // Normal damage logic
            if (hitSparkPrefab != null) Instantiate(hitSparkPrefab, transform.position, Quaternion.identity);
            StartCoroutine(HitStopEffect());
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    // For inviincible duration
    private IEnumerator InvincibilityRoutine()
    {
        // Turn ON invincibility and the glow
        isInvincible = true;
        if (glowIndicator != null) glowIndicator.SetActive(true);

        // Wait for exactly 7 seconds
        yield return new WaitForSeconds(7f);

        // Turn OFF invincibility and the glow
        isInvincible = false;
        if (glowIndicator != null) glowIndicator.SetActive(false);
    }

    private IEnumerator HitStopEffect()
    {
        // Freeze time briefly
        Time.timeScale = 0.1f;

        // Wait for a tiny fraction of a second (using realtime so it isn't affected by the timescale)
        yield return new WaitForSecondsRealtime(0.1f);

        // Return time to normal
        Time.timeScale = 1f;
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Clamp health so it doesn't drop below 0
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Still alive, apply the speed penalty
            if (worldManager != null) worldManager.currentScrollSpeed = 10f;
        }
    }

    void UpdateHealthUI()
    {
        if (healthFill == null) return;

        // Update the bar length
        healthFill.fillAmount = (float)currentHealth / maxHealth;

        // Update the bar color based on thresholds
        if (currentHealth > 5)
        {
            healthFill.color = Color.green;
        }
        else if (currentHealth <= 5 && currentHealth > 2)
        {
            healthFill.color = new Color(1f, 0.5f, 0f);
        }
        else if (currentHealth <= 2)
        {
            healthFill.color = Color.red;
        }
    }

    void Die()
    {
        worldManager.isGameOver = true;

        // Play the Game Over sound
        if (gameOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        float finalScore = worldManager.score;
        int finalScoreInt = Mathf.FloorToInt(finalScore);

        // 1. Load existing JSON data
        int savedHighScore = 0;
        string path = Application.persistentDataPath + "/savefile.json";

        if (File.Exists(path))
        {
            string jsonLoad = File.ReadAllText(path);
            SaveData dataLoad = JsonUtility.FromJson<SaveData>(jsonLoad);
            savedHighScore = dataLoad.highScore;
        }

        // 2. Compare and Save via JSON if higher
        if (finalScoreInt > savedHighScore)
        {
            savedHighScore = finalScoreInt;

            SaveData dataSave = new SaveData();
            dataSave.highScore = savedHighScore;

            string jsonSave = JsonUtility.ToJson(dataSave);
            File.WriteAllText(path, jsonSave);
        }

        // 3. Update the UI
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = "SCORE: " + finalScoreInt;

            if (highScoreText != null)
                highScoreText.text = "HIGH SCORE: " + savedHighScore;
        }
    }

    // Call this from a UI Button to reset the scene
    public void RestartGame()
    {
        // Resets time scale in case it was frozen by the HitStop Coroutine during death
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void SwitchVehicle(VehicleType newVehicle)
    {
        // --- NEW ADDITION: Spawn the smoke puff --- currently disabled
        if (swapParticlePrefab != null)
        {
            Instantiate(swapParticlePrefab, transform.position, Quaternion.identity);
        }

        currentVehicle = newVehicle;

        carMesh.SetActive(false);
        boatMesh.SetActive(false);
        planeMesh.SetActive(false);

        switch (currentVehicle)
        {
            case VehicleType.Car:
                carMesh.SetActive(true);
                rb.useGravity = true;
                break;
            case VehicleType.Boat:
                boatMesh.SetActive(true);
                rb.useGravity = true;
                break;
            case VehicleType.Plane:
                planeMesh.SetActive(true);
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 5f, 0);
                break;
        }
    }
}

//save system
[System.Serializable]
public class SaveData
{
    public int highScore;
}