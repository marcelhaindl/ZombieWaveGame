using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    // Controller
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ZombieController zombieController;

    // GameScreen
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private TextMeshProUGUI zombieCounter;
    [SerializeField] private TextMeshProUGUI waveCounter;

    // NewWaveScreen
    [SerializeField] private GameObject newWaveScreen;
    
    // Start Screen
    [SerializeField] private GameObject startScreen;
    
    // End Screen
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI endText;

    // Pause Screen
    [SerializeField] private GameObject pauseScreen;

    // Local Variables
    private ZombieGenerator zombieGenerator;
    private GameConfig gameConfiguration;
    private float width, height;
    private int currentWave = 1;
    private Vector3 initialPlayerCoordinates;

    // Current number of zombies
    public int CurrentNumberOfZombies
    {
        get => currentNumberOfZombies;
        set
        {
            currentNumberOfZombies = value;
            if (currentNumberOfZombies <= 0)
            {
                // New Wave
                SetUIScreenActive(newWaveScreen);
                ResetPlayerPosition();
            }
        }
    }
    private int currentNumberOfZombies;
    
    // Start is called before the first frame update
    void Start()
    {
        PauseGame(); // Pause game
        zombieGenerator = GetComponent<ZombieGenerator>(); // Initialize zombie generator
        initialPlayerCoordinates = playerController.transform.position; // Save the initial player coordinates
        
        // UI Screen and Health bar size
        SetUIScreenActive(startScreen);
        height = healthBar.sizeDelta.y;
        width = healthBar.sizeDelta.x;
        var newWidth = (float)playerController.Health / (float)playerController.MaxHealth * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
    }

    // Update is called once per frame
    void Update()
    {
        // Set healthbar size
        var newWidth = (float)playerController.Health / (float)playerController.MaxHealth * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
        
        // Update  UI Texts
        zombieCounter.text = CurrentNumberOfZombies + " left";
        waveCounter.text = "Wave " + currentWave;
        endText.text = "You died in Wave " + currentWave + ".";
        
        // Show end screen if player died
        if(playerController.Health <= 0) SetUIScreenActive(endScreen);
        
        // Check if game is paused
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PauseGame();
            SetUIScreenActive(pauseScreen);
        }
    }

    // Set UI Screen function
    private void SetUIScreenActive(GameObject screen)
    {
        GameObject[] screens = { startScreen, gameScreen, newWaveScreen, endScreen, pauseScreen };

        foreach (var s in screens)
        {
            // Set the target screen to active and all others to inactive
            s.SetActive(s == screen);
        }
    }
    
    // On Start Button Pressed function
    public void OnStartPressed()
    {
        ResumeGame();
        zombieGenerator.SpawnWave(currentWave); // Spawn the current wave
        CurrentNumberOfZombies = GameObject.FindGameObjectsWithTag("Zombie").Length; // Get the current number of zombies
        SetUIScreenActive(gameScreen);
    }

    // On Continue Button Pressed function
    public void OnContinuePressed()
    {
        currentWave++; // Count wave up
        zombieGenerator.SpawnWave(currentWave); // Spawn the next wave
        CurrentNumberOfZombies = GameObject.FindGameObjectsWithTag("Zombie").Length; // Get the current number of zombies
        SetUIScreenActive(gameScreen);
    }

    // On Quit Button Pressed function
    public void OnQuitPressed()
    {
        PauseGame();
        #if UNITY_EDITOR
        // Stop play mode in the Unity Editor
        EditorApplication.isPlaying = false;
        #else
        // Quit the application in a built game
        Application.Quit();
        #endif
    }

    // Function to reset player position to initial coordinates
    private void ResetPlayerPosition()
    {
        playerController.transform.position = initialPlayerCoordinates;
    }
    
    // Function to pause game
    public void PauseGame ()
    {
        Time.timeScale = 0;
    }
    
    // Function to resume game
    public void ResumeGame ()
    {
        Time.timeScale = 1;
        SetUIScreenActive(gameScreen);
    }

    // Function to go back to start screen
    public void BackToStart()
    {
        currentWave = 1;
        PauseGame();
        // Destroy all zombies
        foreach (var zombie in GameObject.FindGameObjectsWithTag("Zombie"))
        {
            Destroy(zombie);
        }
        ResetPlayerPosition();
        playerController.Health = playerController.MaxHealth;
        SetUIScreenActive(startScreen);
    }
}
