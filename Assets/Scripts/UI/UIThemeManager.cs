using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UIThemePalette
{
    public string themeName = "Cozy Cafe";
    public Color panelBackgroundColor = new Color(0.98f, 0.94f, 0.88f, 0.95f); // Kem ấm
    public Color primaryAccentColor = new Color(0.95f, 0.55f, 0.25f, 1f);      // Cam đào
    public Color buttonNormalColor = new Color(0.92f, 0.65f, 0.35f, 1f);       // Nâu mật ong
    public Color textHeaderColor = new Color(0.35f, 0.22f, 0.15f, 1f);         // Nâu cà phê đậm
    public Color textBodyColor = new Color(0.25f, 0.25f, 0.25f, 1f);
}

public class UIThemeManager : MonoBehaviour
{
    public static UIThemeManager Instance { get; private set; }

    [Header("Available Themes")]
    public List<UIThemePalette> themes = new List<UIThemePalette>();
    public int currentThemeIndex = 0;

    public event Action<UIThemePalette> OnThemeChanged;

    private const string PREF_THEME_INDEX = "CatCafe_UIThemeIndex";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDefaultThemes();
        currentThemeIndex = PlayerPrefs.GetInt(PREF_THEME_INDEX, 0);
    }

    private void InitializeDefaultThemes()
    {
        if (themes.Count == 0)
        {
            // 1. Cozy Cafe (Mặc định: Ấm cúng, mộc mạc)
            themes.Add(new UIThemePalette
            {
                themeName = "Cozy Cafe (Ấm Áp)",
                panelBackgroundColor = new Color(0.98f, 0.94f, 0.88f, 0.96f),
                primaryAccentColor = new Color(0.95f, 0.55f, 0.25f, 1f),
                buttonNormalColor = new Color(0.88f, 0.58f, 0.28f, 1f),
                textHeaderColor = new Color(0.32f, 0.18f, 0.1f, 1f)
            });

            // 2. Lofi Midnight (Đêm Lofi: Tím huyền ảo, êm dịu mắt)
            themes.Add(new UIThemePalette
            {
                themeName = "Lofi Midnight (Đêm Huyền Ảo)",
                panelBackgroundColor = new Color(0.12f, 0.11f, 0.18f, 0.95f),
                primaryAccentColor = new Color(0.72f, 0.45f, 0.95f, 1f),
                buttonNormalColor = new Color(0.45f, 0.35f, 0.75f, 1f),
                textHeaderColor = new Color(0.92f, 0.88f, 1f, 1f)
            });

            // 3. Pastel Sakura (Hoa Anh Đào: Hồng phấn ngọt ngào)
            themes.Add(new UIThemePalette
            {
                themeName = "Pastel Sakura (Hoa Anh Đào)",
                panelBackgroundColor = new Color(0.99f, 0.92f, 0.95f, 0.96f),
                primaryAccentColor = new Color(0.96f, 0.45f, 0.65f, 1f),
                buttonNormalColor = new Color(0.92f, 0.55f, 0.72f, 1f),
                textHeaderColor = new Color(0.45f, 0.15f, 0.28f, 1f)
            });
        }
    }

    public UIThemePalette GetCurrentTheme()
    {
        if (themes.Count == 0) InitializeDefaultThemes();
        int idx = Mathf.Clamp(currentThemeIndex, 0, themes.Count - 1);
        return themes[idx];
    }

    public void SetTheme(int index)
    {
        if (index < 0 || index >= themes.Count) return;
        currentThemeIndex = index;
        PlayerPrefs.SetInt(PREF_THEME_INDEX, currentThemeIndex);
        PlayerPrefs.Save();

        OnThemeChanged?.Invoke(GetCurrentTheme());
        Debug.Log($"[UIThemeManager] Đã chuyển đổi theme giao diện sang: {themes[index].themeName}");
    }

    public void CycleNextTheme()
    {
        int next = (currentThemeIndex + 1) % themes.Count;
        SetTheme(next);

        if (UIManager.Instance != null)
        {
            Camera cam = Camera.main;
            Vector3 pos = cam != null ? cam.transform.position + cam.transform.forward * 2f : Vector3.zero;
            UIManager.Instance.ShowFloatingText($"🎨 Giao Diện: {GetCurrentTheme().themeName}!", pos, Color.cyan);
        }
    }
}
