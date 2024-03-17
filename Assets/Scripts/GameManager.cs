using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float initialGameSpeed = 5f;
    public float gameSpeedIncrease = 0.1f;
    public float gameSpeed { 
        get; 
        private set; 
    }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Text timerText;    
    [SerializeField] private TextMeshProUGUI getReadyText;
    [SerializeField] private Button retryButton;

    [SerializeField] private Text score0Feedback;
    [SerializeField] private Text score20Feedback;
    [SerializeField] private Text score50Feedback;
    [SerializeField] private Text score100Feedback;
    [SerializeField] private Text score200Feedback;
    [SerializeField] private Text score300Feedback;
    [SerializeField] private Text score500Feedback;
    [SerializeField] private Text score750Feedback;

    private Player player;
    private Spawner spawner;
    private bool isGameActive = false;
    private float score;
    public float Score => score;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        spawner = FindObjectOfType<Spawner>();

        // Show "Get Ready" text and retry button initially
        getReadyText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
        retryButton.interactable = true;  // Enable retry button
        gameOverText.gameObject.SetActive(false);

        score0Feedback.gameObject.SetActive(true);
        score20Feedback.gameObject.SetActive(false);
        score50Feedback.gameObject.SetActive(false);
        score100Feedback.gameObject.SetActive(false);
        score200Feedback.gameObject.SetActive(false);
        score300Feedback.gameObject.SetActive(false);
        score500Feedback.gameObject.SetActive(false);
        score750Feedback.gameObject.SetActive(false);

        // Stop the timer when the game is over
        TimerManager.Instance.StopTimer();
    }

    public void NewGame()
    {
        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();

        foreach (var obstacle in obstacles) {
            Destroy(obstacle.gameObject);
        }

        score = 0f;
        gameSpeed = initialGameSpeed;
        enabled = true;

        // Start the timer when the game begins
        TimerManager.Instance.RestartTimer();

        player.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        getReadyText.gameObject.SetActive(false);

        // Enable retry button for the next game
        retryButton.interactable = true;

        // Allow player control
        isGameActive = true;
    }

    public void GameOver()
    {
        gameSpeed = 0f;
        enabled = false;

        player.gameObject.SetActive(false);
        spawner.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
        getReadyText.gameObject.SetActive(false);

        // Stop the timer when the game is over
        TimerManager.Instance.StopTimer();

        UpdateHiscore();

        // Disable player control
        isGameActive = false;
    }

    public void Retry()
    {
        // Hide "Game Over" text and retry button
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        // Show "Get Ready" text and retry button for the next game
        getReadyText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);

        // Enable retry button for the next game
        retryButton.interactable = true;

        // Start a new game
        NewGame();
    }

    private void Update()
    {
        if (!isGameActive)
        {
            // Disable player control during "Get Ready" phase
            return;
        }

        gameSpeed += gameSpeedIncrease * Time.deltaTime;
        score += gameSpeed * Time.deltaTime;
        scoreText.text = Mathf.FloorToInt(score).ToString("D5");

        // Check for score milestones and display feedback
        CheckScoreMilestones();
    }

    private void CheckScoreMilestones()
    {
        int intScore = Mathf.FloorToInt(score);

        if (intScore == 0)
        {
            StartCoroutine(DisplayFeedback(score0Feedback));
        }
        if (intScore == 50)
        {
            StartCoroutine(DisplayFeedback(score50Feedback));
        }
        else if (intScore == 100)
        {
            StartCoroutine(DisplayFeedback(score100Feedback));
        }
        else if (intScore == 200)
        {
            StartCoroutine(DisplayFeedback(score200Feedback));
        }
        else if (intScore == 300)
        {
            StartCoroutine(DisplayFeedback(score300Feedback));
        }
        else if (intScore == 500)
        {
            StartCoroutine(DisplayFeedback(score500Feedback));
        }
        else if (intScore == 750)
        {
            StartCoroutine(DisplayFeedback(score750Feedback));
        }
    }

    private System.Collections.IEnumerator DisplayFeedback(Text feedbackText)
    {
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        feedbackText.gameObject.SetActive(false);
    }

    private void UpdateHiscore()
    {
        float hiscore = PlayerPrefs.GetFloat("hiscore", 0);

        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);
        }

        hiscoreText.text = Mathf.FloorToInt(hiscore).ToString("D5");
    }
}
