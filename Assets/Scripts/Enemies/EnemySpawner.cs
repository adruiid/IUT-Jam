using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Survival horde spawner. Continuously spawns enemies on the NavMesh in a ring around
/// the player, capped at a max alive count, with the spawn rate ramping up over time.
/// Enemy type is chosen by weighted random — higher weight = spawns more often.
/// </summary>
[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject prefab;
        [Tooltip("Base relative spawn chance on night 1. Weight 2 spawns twice as often as weight 1. 0 = never.")]
        [Min(0f)] public float weight = 1f;
        [Tooltip("Weight multiplies by this each night. >1 = more common later (elites), <1 = rarer later (grunts), 1 = unchanged.")]
        [Min(0f)] public float weightPerNight = 1f;
    }

    [Header("Enemies (weighted random)")]
    [SerializeField] private EnemyType[] enemyTypes;
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Maximum enemies alive at once on night 1.")]
    [SerializeField] private int maxAlive = 20;
    [Tooltip("Max-alive cap multiplies by this each night (1.3 = 30% more per night).")]
    [SerializeField] private float maxAlivePerNight = 1.3f;

    [Header("Rate (per-night)")]
    [Tooltip("Seconds between spawns on the FIRST night.")]
    [SerializeField] private float baseInterval = 2f;
    [Tooltip("Fastest allowed interval, so it never gets absurd.")]
    [SerializeField] private float minInterval = 0.4f;
    [Tooltip("Each night the spawn RATE multiplies by this (interval divides by it). 1.5 = 50% faster per night.")]
    [SerializeField] private float nightRateMultiplier = 1.5f;

    [Header("Placement (ring around the player)")]
    [SerializeField] private float minRadius = 12f;
    [SerializeField] private float maxRadius = 20f;
    [Tooltip("Search radius for a NavMesh point at each candidate spot.")]
    [SerializeField] private float navSampleRadius = 4f;
    [SerializeField] private int placementAttempts = 8;

    [Header("No-spawn zones")]
    [Tooltip("Layers of keep-out volumes (put colliders over the base / barn / hut on this layer). " +
             "Candidate points inside one are rejected. Leave empty to disable.")]
    [SerializeField] private LayerMask noSpawnMask;
    [Tooltip("Overlap radius checked at each candidate against the no-spawn zones.")]
    [SerializeField] private float noSpawnCheckRadius = 0.5f;

    private Transform _player;
    private DayNightController _dayNight;
    private int _alive;
    private float _nextSpawn;

    private void Start()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) _player = go.transform;
        _dayNight = FindAnyObjectByType<DayNightController>();

        _nextSpawn = Time.time + CurrentInterval();
    }

    private void Update()
    {
        if (_player == null || enemyTypes == null || enemyTypes.Length == 0) return;
        if (Time.time < _nextSpawn || _alive >= EffectiveMaxAlive()) return;

        GameObject prefab = PickEnemy();
        if (prefab != null && TryGetSpawnPoint(out Vector3 pos))
            SpawnAt(prefab, pos);

        _nextSpawn = Time.time + CurrentInterval();
    }

    private int CurrentDay => _dayNight != null ? _dayNight.Day : 1;

    private float CurrentInterval()
    {
        // Spawn rate multiplies by nightRateMultiplier each night (Day 1 = base, Day 2 = 1.5x, ...).
        float rate = Mathf.Pow(nightRateMultiplier, Mathf.Max(0, CurrentDay - 1));
        return Mathf.Max(minInterval, baseInterval / rate);
    }

    private int EffectiveMaxAlive()
    {
        return Mathf.Max(1, Mathf.RoundToInt(maxAlive * Mathf.Pow(maxAlivePerNight, Mathf.Max(0, CurrentDay - 1))));
    }

    // A type's weight for the current night (base weight * weightPerNight^(night-1)).
    private float EffectiveWeight(EnemyType e, int day)
    {
        if (e == null || e.prefab == null) return 0f;
        return Mathf.Max(0f, e.weight) * Mathf.Pow(Mathf.Max(0f, e.weightPerNight), Mathf.Max(0, day - 1));
    }

    // Weighted random pick across the enemy types (weights scale per night).
    private GameObject PickEnemy()
    {
        int day = CurrentDay;

        float total = 0f;
        for (int i = 0; i < enemyTypes.Length; i++)
            total += EffectiveWeight(enemyTypes[i], day);
        if (total <= 0f) return null;

        float roll = Random.value * total;
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            float w = EffectiveWeight(enemyTypes[i], day);
            if (w <= 0f) continue;
            if (roll < w) return enemyTypes[i].prefab;
            roll -= w;
        }
        return null; // shouldn't happen, but safe
    }

    private bool TryGetSpawnPoint(out Vector3 result)
    {
        for (int i = 0; i < placementAttempts; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 candidate = _player.position + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
            {
                // Reject points inside a keep-out zone (base / barn / hut); try another spot.
                if (noSpawnMask.value != 0 &&
                    Physics.CheckSphere(hit.position, noSpawnCheckRadius, noSpawnMask, QueryTriggerInteraction.Collide))
                    continue;

                result = hit.position;
                return true;
            }
        }
        result = default;
        return false;
    }

    private void SpawnAt(GameObject prefab, Vector3 pos)
    {
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        _alive++;

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null) health.Died += OnEnemyDied; // free the slot the moment it dies
        else _alive--;                                   // no health to track -> don't count it
    }

    // Frees a slot at death (during the death animation), so the horde keeps flowing.
    private void OnEnemyDied() => _alive = Mathf.Max(0, _alive - 1);

    // Show the spawn ring around the player in the editor: red = min, blue = max.
    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        if (!string.IsNullOrEmpty(playerTag))
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) center = p.transform.position;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, minRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center, maxRadius);
    }
}
