using UnityEngine;
using System.IO;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

[System.Serializable]
public class GameSaveData
{
    public float currentMoney;
    // Add arrays here to save owned cats, purchased tables, etc.
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;
    private DatabaseReference dbReference;
    // In a real game, you'd use Firebase Auth to get the user ID
    private string userId = "test_user_01"; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase Realtime Database Initialized!");
                
                // You might want to auto-load game here once DB is ready
                // LoadGame();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
            }
        });
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();
        
        // Gather data from managers
        if (MoneyManager.Instance != null)
        {
            data.currentMoney = MoneyManager.Instance.CurrentMoney;
        }

        string json = JsonUtility.ToJson(data, true);
        
        // 1. Save locally as backup
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved Locally to: " + saveFilePath);

        // 2. Save to Firebase
        if (dbReference != null)
        {
            dbReference.Child("users").Child(userId).Child("saveData").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    Debug.Log("Game Saved to Firebase Realtime Database!");
                }
                else
                {
                    Debug.LogError("Failed to save to Firebase: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogWarning("Firebase DB Reference is null, could not save to cloud.");
        }
    }

    public void LoadGame()
    {
        if (dbReference != null)
        {
            dbReference.Child("users").Child(userId).Child("saveData").GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.LogError("Failed to load from Firebase, falling back to local. Error: " + task.Exception);
                    LoadGameLocally();
                }
                else if (task.IsCompleted && task.Result.Exists)
                {
                    string json = task.Result.GetRawJsonValue();
                    GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                    
                    RestoreDataToManagers(data);
                    Debug.Log("Game Loaded successfully from Firebase!");
                }
                else
                {
                    Debug.Log("No data found on Firebase, falling back to local load.");
                    LoadGameLocally();
                }
            });
        }
        else
        {
            LoadGameLocally();
        }
    }

    private void LoadGameLocally()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

            RestoreDataToManagers(data);
            Debug.Log("Game Loaded Locally!");
        }
        else
        {
            Debug.Log("No save file found locally or on Firebase, starting new game.");
        }
    }
    
    private void RestoreDataToManagers(GameSaveData data)
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.LoadMoney(data.currentMoney);
        }
        // Restore other managers (e.g. CatManager) here
    }
}
