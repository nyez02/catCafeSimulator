using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    ShiftSummary
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Currencies & Economy")]
    public int pawGems = 20; // Khởi đầu 20 Kim Cương Mèo

    [Header("Current State")]
    public GameState currentState = GameState.Playing;

    public event Action<int> OnGemsChanged;
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        OnGemsChanged?.Invoke(pawGems);
    }

    public void AddGems(int amount)
    {
        pawGems += amount;
        OnGemsChanged?.Invoke(pawGems);
    }

    public bool SpendGems(int amount)
    {
        if (pawGems >= amount)
        {
            pawGems -= amount;
            OnGemsChanged?.Invoke(pawGems);
            return true;
        }
        return false;
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;
        if (currentState == GameState.Paused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        OnGameStateChanged?.Invoke(currentState);
    }

    public void PauseGame()
    {
        SetGameState(GameState.Paused);
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
    }

    public void LoadGameScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
