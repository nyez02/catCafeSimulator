using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatVFXManager : MonoBehaviour
{
    public static CatVFXManager Instance { get; private set; }

    private readonly Queue<ParticleSystem> heartPool = new Queue<ParticleSystem>();
    private readonly Queue<ParticleSystem> coinPool = new Queue<ParticleSystem>();
    private readonly Queue<ParticleSystem> confettiPool = new Queue<ParticleSystem>();
    private readonly Queue<ParticleSystem> steamPool = new Queue<ParticleSystem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PrewarmPools();
    }

    private void PrewarmPools()
    {
        for (int i = 0; i < 6; i++)
        {
            var psHeart = CreateHeartParticleInstance();
            psHeart.gameObject.SetActive(false);
            heartPool.Enqueue(psHeart);

            var psCoin = CreateCoinParticleInstance();
            psCoin.gameObject.SetActive(false);
            coinPool.Enqueue(psCoin);

            var psConfetti = CreateConfettiParticleInstance();
            psConfetti.gameObject.SetActive(false);
            confettiPool.Enqueue(psConfetti);

            var psSteam = CreateSteamParticleInstance();
            psSteam.gameObject.SetActive(false);
            steamPool.Enqueue(psSteam);
        }
    }

    private ParticleSystem CreateHeartParticleInstance()
    {
        GameObject vfxObj = new GameObject("PooledHeartBurstVFX");
        vfxObj.transform.SetParent(transform);

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 0.4f, 0.7f, 1f); // Màu hồng đào
        main.startSize = 0.35f;
        main.startSpeed = 1.8f;
        main.startLifetime = 1.2f;
        main.duration = 0.8f;
        main.loop = false;
        main.gravityModifier = -0.2f;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 12) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.2f;

        return ps;
    }

    private ParticleSystem CreateCoinParticleInstance()
    {
        GameObject vfxObj = new GameObject("PooledCoinSparkleVFX");
        vfxObj.transform.SetParent(transform);

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 0.85f, 0.1f, 1f); // Màu vàng kim
        main.startSize = 0.25f;
        main.startSpeed = 2.5f;
        main.startLifetime = 0.8f;
        main.duration = 0.5f;
        main.loop = false;
        main.gravityModifier = 0.4f;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 16) });

        return ps;
    }

    private ParticleSystem CreateConfettiParticleInstance()
    {
        GameObject vfxObj = new GameObject("PooledConfettiVFX");
        vfxObj.transform.SetParent(transform);

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.3f, 0.3f), // Đỏ
            new Color(0.3f, 0.8f, 1f)  // Xanh dương
        );
        main.startSize = 0.2f;
        main.startSpeed = 3.5f;
        main.startLifetime = 1.5f;
        main.duration = 0.8f;
        main.loop = false;
        main.gravityModifier = 0.6f;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.4f;

        return ps;
    }

    private ParticleSystem CreateSteamParticleInstance()
    {
        GameObject vfxObj = new GameObject("PooledSteamVFX");
        vfxObj.transform.SetParent(transform);

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 1f, 1f, 0.35f); // Khói trắng mờ
        main.startSize = 0.2f;
        main.startSpeed = 0.5f;
        main.startLifetime = 1.2f;
        main.duration = 0.6f;
        main.loop = false;
        main.gravityModifier = -0.1f;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 8) });

        return ps;
    }

    public void SpawnHeartBurst(Vector3 position)
    {
        SpawnFromPool(heartPool, CreateHeartParticleInstance, position, 1.4f);
    }
    public void PlayHeartBurst(Vector3 position) => SpawnHeartBurst(position);

    public void SpawnCoinSparkle(Vector3 position)
    {
        SpawnFromPool(coinPool, CreateCoinParticleInstance, position, 1.0f);
    }
    public void PlayCoinSparkle(Vector3 position) => SpawnCoinSparkle(position);

    public void SpawnConfettiCelebration(Vector3 position)
    {
        SpawnFromPool(confettiPool, CreateConfettiParticleInstance, position, 1.8f);
    }
    public void PlayConfetti(Vector3 position) => SpawnConfettiCelebration(position);

    public void SpawnCoffeeSteam(Vector3 position)
    {
        SpawnFromPool(steamPool, CreateSteamParticleInstance, position, 1.3f);
    }
    public void PlayCoffeeSteam(Vector3 position) => SpawnCoffeeSteam(position);

    private void SpawnFromPool(Queue<ParticleSystem> pool, System.Func<ParticleSystem> createFunc, Vector3 position, float delay)
    {
        ParticleSystem ps = null;
        while (pool.Count > 0 && ps == null)
        {
            ps = pool.Dequeue();
        }

        if (ps == null)
        {
            ps = createFunc();
        }

        ps.transform.position = position;
        ps.gameObject.SetActive(true);
        ps.Play();
        StartCoroutine(ReturnToPoolRoutine(ps, pool, delay));
    }

    private IEnumerator ReturnToPoolRoutine(ParticleSystem ps, Queue<ParticleSystem> targetPool, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ps != null)
        {
            ps.Stop();
            ps.gameObject.SetActive(false);
            ps.transform.SetParent(transform);
            targetPool.Enqueue(ps);
        }
    }
}
