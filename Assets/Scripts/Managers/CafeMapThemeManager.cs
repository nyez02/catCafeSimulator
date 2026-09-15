using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatCafe.Maps
{
    [Serializable]
    public class CafeMapTheme
    {
        public string id;
        public string displayName;
        public string description;
        public int requiredLevel;
        public Color floorColor;
        public Color wallColor;
        public Color ambientLightColor;
        public float lightIntensity;
        public string ambientVFX; // "none", "sakura", "stars", "beach_sun"
        public string musicMood;  // "lofi", "japanese", "tropical", "jazz"
    }

    /// <summary>
    /// Quản lý Đa Bản Đồ & Nhiều Màn Chơi (Multi-Map & Theme System).
    /// Cho phép người chơi mở khóa và chuyển đổi qua lại giữa các Quán Cafe phong cách khác nhau:
    /// 1. Cozy Vintage Lounge (Cổ điển ấm áp)
    /// 2. Sakura Zen Garden (Vườn Hoa Anh Đào Nhật Bản)
    /// 3. Tropical Sunset Beach (Bãi biển hoàng hôn nhiệt đới)
    /// 4. Skyline Penthouse (Tầng thượng ngắm sao đêm)
    /// </summary>
    public class CafeMapThemeManager : MonoBehaviour
    {
        public static CafeMapThemeManager Instance { get; private set; }

        [Header("Current Map Theme")]
        public string currentMapId = "cozy_vintage";

        [Header("Available Map Themes")]
        public List<CafeMapTheme> availableThemes = new List<CafeMapTheme>();

        public event Action<CafeMapTheme> OnMapThemeChanged;

        private ParticleSystem ambientVFXInstance;
        private const string PREF_CURRENT_MAP = "CatCafe_SelectedMapTheme";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeThemes();
            currentMapId = PlayerPrefs.GetString(PREF_CURRENT_MAP, "cozy_vintage");
        }

        private void Start()
        {
            ApplyTheme(currentMapId, false);
        }

        private void InitializeThemes()
        {
            availableThemes.Clear();

            availableThemes.Add(new CafeMapTheme
            {
                id = "cozy_vintage",
                displayName = "☕ Quán Gỗ Sồi Cổ Điển",
                description = "Không gian gỗ ấm cúng, đèn vàng lofi dịu nhẹ, nơi thư thái nhất của những chú mèo.",
                requiredLevel = 1,
                floorColor = new Color(0.72f, 0.52f, 0.35f),
                wallColor = new Color(0.96f, 0.93f, 0.88f),
                ambientLightColor = new Color(1f, 0.92f, 0.78f),
                lightIntensity = 1.15f,
                ambientVFX = "none",
                musicMood = "lofi"
            });

            availableThemes.Add(new CafeMapTheme
            {
                id = "sakura_zen",
                displayName = "🌸 Quán Trà Hoa Anh Đào",
                description = "Phong cách Nhật Bản thanh tịnh với chiếu tatami, cánh hoa anh đào hồng bay lơ lửng và đèn lồng Kyoto.",
                requiredLevel = 2,
                floorColor = new Color(0.85f, 0.78f, 0.62f), // Sàn chiếu Tatami
                wallColor = new Color(0.98f, 0.92f, 0.94f),  // Tường hồng phấn nhạt
                ambientLightColor = new Color(1f, 0.88f, 0.92f), // Ánh sáng hồng dịu
                lightIntensity = 1.25f,
                ambientVFX = "sakura",
                musicMood = "japanese"
            });

            availableThemes.Add(new CafeMapTheme
            {
                id = "tropical_beach",
                displayName = "🏖️ Quán Cafe Bãi Biển Hoàng Hôn",
                description = "Tận hưởng làn gió biển nhiệt đới mát rượi, sàn gỗ trắng ven biển và ánh hoàng hôn cam vàng rực rỡ.",
                requiredLevel = 3,
                floorColor = new Color(0.90f, 0.86f, 0.78f), // Sàn gỗ tẩy trắng biển
                wallColor = new Color(0.88f, 0.95f, 0.98f),  // Tường lam ngọc mát mẻ
                ambientLightColor = new Color(1f, 0.75f, 0.55f), // Hoàng hôn rực rỡ
                lightIntensity = 1.35f,
                ambientVFX = "beach_sun",
                musicMood = "tropical"
            });

            availableThemes.Add(new CafeMapTheme
            {
                id = "skyline_penthouse",
                displayName = "🏙️ Tầng Thượng Ngắm Sao Đêm",
                description = "Skyline Lounge hiện đại trên đỉnh tòa nhà chọc trời, sàn đá cẩm thạch ngắm dải ngân hà huyền ảo.",
                requiredLevel = 4,
                floorColor = new Color(0.16f, 0.18f, 0.24f), // Đá cẩm thạch đen xám sang trọng
                wallColor = new Color(0.12f, 0.14f, 0.20f),  // Tường đêm sâu thẳm
                ambientLightColor = new Color(0.6f, 0.75f, 1f), // Ánh sáng trăng xanh sao đêm
                lightIntensity = 0.95f,
                ambientVFX = "stars",
                musicMood = "jazz"
            });
        }

        public void ApplyTheme(string themeId, bool showToast = true)
        {
            CafeMapTheme theme = availableThemes.Find(t => t.id == themeId);
            if (theme == null) return;

            currentMapId = themeId;
            PlayerPrefs.SetString(PREF_CURRENT_MAP, currentMapId);
            PlayerPrefs.Save();

            // 1. Áp dụng vật liệu sàn và tường trong Cafe_Interior_Environment
            GameObject mapRoot = GameObject.Find("Cafe_Interior_Environment");
            if (mapRoot != null)
            {
                ApplyColorsToEnvironment(mapRoot, theme);
            }

            // 2. Điều chỉnh Ánh Sáng Quán (Lighting & Ambient)
            RenderSettings.ambientLight = theme.ambientLightColor * 0.8f;
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional || l.type == LightType.Point)
                {
                    l.color = theme.ambientLightColor;
                    l.intensity = theme.lightIntensity;
                }
            }

            // 3. Hiệu ứng hạt môi trường đặc trưng (Sakura petals, Star dust, ...)
            ApplyAmbientVFX(theme.ambientVFX);

            if (showToast)
            {
                ToastManager.Instance?.ShowToast($"Đã chuyển sang Bản Đồ: {theme.displayName}!", "✨", new Color(0.3f, 0.7f, 1f));
                SoundManager.Instance?.PlayUpgrade();
            }

            OnMapThemeChanged?.Invoke(theme);
        }

        private void ApplyColorsToEnvironment(GameObject root, CafeMapTheme theme)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                string objName = r.gameObject.name.ToLower();
                if (objName.Contains("floor") || objName.Contains("parquet") || objName.Contains("san"))
                {
                    r.material.color = theme.floorColor;
                }
                else if (objName.Contains("wall") || objName.Contains("tuong"))
                {
                    r.material.color = theme.wallColor;
                }
            }
        }

        private void ApplyAmbientVFX(string vfxType)
        {
            if (ambientVFXInstance != null)
            {
                Destroy(ambientVFXInstance.gameObject);
            }

            if (vfxType == "none") return;

            GameObject vfxObj = new GameObject("AmbientMapVFX_" + vfxType);
            ambientVFXInstance = vfxObj.AddComponent<ParticleSystem>();

            var main = ambientVFXInstance.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;

            var shape = ambientVFXInstance.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(36f, 1f, 26f);
            vfxObj.transform.position = new Vector3(0f, 6.5f, -2f);

            var emission = ambientVFXInstance.emission;
            emission.rateOverTime = 20f;

            var renderer = ambientVFXInstance.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material vfxMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            if (vfxMat.shader == null) vfxMat = new Material(Shader.Find("Particles/Standard Unlit"));

            if (vfxType == "sakura")
            {
                // Cánh hoa anh đào rơi phấp phới
                main.startLifetime = 7f;
                main.startSpeed = 0.8f;
                main.startSize = 0.28f;
                main.startColor = new Color(1f, 0.72f, 0.85f, 0.8f);
                main.gravityModifier = 0.08f;
            }
            else if (vfxType == "stars")
            {
                // Bụi sao lấp lánh trên tầng thượng
                main.startLifetime = 4f;
                main.startSpeed = 0.3f;
                main.startSize = 0.2f;
                main.startColor = new Color(0.6f, 0.9f, 1f, 0.9f);
                main.gravityModifier = -0.02f;
            }
            else if (vfxType == "beach_sun")
            {
                // Đốm nắng vàng hoàng hôn biển
                main.startLifetime = 5f;
                main.startSpeed = 0.5f;
                main.startSize = 0.25f;
                main.startColor = new Color(1f, 0.82f, 0.45f, 0.7f);
                main.gravityModifier = 0.03f;
            }

            renderer.sharedMaterial = vfxMat;
            ambientVFXInstance.Play();
        }
    }
}
