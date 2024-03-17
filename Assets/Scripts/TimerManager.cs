using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }
    public int ElapsedTime { get; internal set; }

    [SerializeField] private Text timerText;
    private float elapsedTime;
    private bool isTimerRunning = true;  // Ensure the timer starts running

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void RestartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;  // Restart the timer
    }

    public void StopTimer()
    {
        isTimerRunning = false;  // Stop the timer
    }
}
