using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject panel;
    public Transform levelButtonsContainer;

    [Header("Levels Data")]
    public List<LevelData> levels = new List<LevelData>();

    public void OpenPanel()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void SelectLevel(int index)
    {
        if (index >= 0 && index < levels.Count)
        {
            LevelData chosen = levels[index];
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.currentLevel = chosen;
            }

            SoundManager.Instance?.PlayClick();
            SceneManager.LoadScene("CafeScene");
        }
    }
}
