using UnityEngine;

public class CatVFXManager : MonoBehaviour
{
    public static CatVFXManager Instance { get; private set; }

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

    /// <summary>
    /// Bắn chùm hạt tim hồng bay lên khi vuốt ve mèo
    /// </summary>
    public void SpawnHeartBurst(Vector3 position)
    {
        GameObject vfxObj = new GameObject("HeartBurstVFX");
        vfxObj.transform.position = position;

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = vfxObj.GetComponent<ParticleSystemRenderer>();

        var main = ps.main;
        main.startColor = new Color(1f, 0.4f, 0.7f, 1f); // Màu hồng đào
        main.startSize = 0.35f;
        main.startSpeed = 1.8f;
        main.startLifetime = 1.2f;
        main.duration = 0.8f;
        main.loop = false;
        main.gravityModifier = -0.2f; // Bay bổng lên trời

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 12) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.2f;

        ps.Play();
        Destroy(vfxObj, 1.5f);
    }

    /// <summary>
    /// Bắn tia sáng vàng lấp lánh khi nhận tiền thanh toán
    /// </summary>
    public void SpawnCoinSparkle(Vector3 position)
    {
        GameObject vfxObj = new GameObject("CoinSparkleVFX");
        vfxObj.transform.position = position;

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 0.85f, 0.1f, 1f); // Màu vàng kim
        main.startSize = 0.25f;
        main.startSpeed = 2.5f;
        main.startLifetime = 0.8f;
        main.duration = 0.5f;
        main.loop = false;
        main.gravityModifier = 0.4f;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 16) });

        ps.Play();
        Destroy(vfxObj, 1.2f);
    }
}
