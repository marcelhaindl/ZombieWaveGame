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

    private ZombieGenerator zombieGenerator;
    private GameConfig gameConfiguration;
    
    private float width, height;

    private int currentWave = 1;
    private int currentNumberOfZombies;

    private Vector3 initialPlayerCoordinates;

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
    
    // Start is called before the first frame update
    void Start()
    {
        PauseGame();
        zombieGenerator = GetComponent<ZombieGenerator>();
        initialPlayerCoordinates = playerController.transform.position;
        
        // UI
        SetUIScreenActive(startScreen);
        height = healthBar.sizeDelta.y;
        width = healthBar.sizeDelta.x;
        var newWidth = (float)playerController.Health / (float)playerController.MaxHealth * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
    }

    // Update is called once per frame
    void Update()
    {
        var newWidth = (float)playerController.Health / (float)playerController.MaxHealth * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
        
        zombieCounter.text = CurrentNumberOfZombies + " left";
        waveCounter.text = "Wave " + currentWave;

        endText.text = "You died in Wave " + currentWave + ".";
        
        if(playerController.Health <= 0) SetUIScreenActive(endScreen);
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PauseGame();
            SetUIScreenActive(pauseScreen);
        }
    }

    private void SetUIScreenActive(GameObject screen)
    {
        GameObject[] screens = { startScreen, gameScreen, newWaveScreen, endScreen, pauseScreen };

        foreach (var s in screens)
        {
            // Set the target screen to active and all others to inactive
            s.SetActive(s == screen);
        }
    }
    
    public void OnStartPressed()
    {
        ResumeGame();
        zombieGenerator.SpawnWave(currentWave);
        CurrentNumberOfZombies = GameObject.FindGameObjectsWithTag("Zombie").Length;
        SetUIScreenActive(gameScreen);
    }

    public void OnContinuePressed()
    {
        currentWave++;
        zombieGenerator.SpawnWave(currentWave);
        CurrentNumberOfZombies = GameObject.FindGameObjectsWithTag("Zombie").Length;
        SetUIScreenActive(gameScreen);
    }

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

    private void ResetPlayerPosition()
    {
        playerController.transform.position = initialPlayerCoordinates;
    }
    
    public void PauseGame ()
    {
        Time.timeScale = 0;
    }
    
    public void ResumeGame ()
    {
        Time.timeScale = 1;
        SetUIScreenActive(gameScreen);
    }

    public void BackToStart()
    {
        currentWave = 1;
        PauseGame();
        foreach (var zombie in GameObject.FindGameObjectsWithTag("Zombie"))
        {
            Destroy(zombie);
        }
        ResetPlayerPosition();
        playerController.Health = playerController.MaxHealth;
        SetUIScreenActive(startScreen);
    }
}
