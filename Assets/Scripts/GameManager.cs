using TMPro;
using UnityEngine;
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
    }

    private void SetUIScreenActive(GameObject screen)
    {
        GameObject[] screens = { startScreen, gameScreen, newWaveScreen, endScreen };

        foreach (var s in screens)
        {
            // Set the target screen to active and all others to inactive
            s.SetActive(s == screen);
        }
    }
    
    public void OnStartPressed()
    {
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
}
