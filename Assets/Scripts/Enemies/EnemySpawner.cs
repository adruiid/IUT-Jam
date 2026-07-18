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
        [Tooltip("Relative spawn chance. Weight 2 spawns twice as often as weight 1. 0 = never.")]
        [Min(0f)] public float weight = 1f;
    }

    [Header("Enemies (weighted random)")]
    [SerializeField] private EnemyType[] enemyTypes;
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Maximum enemies alive at once.")]
    [SerializeField] private int maxAlive = 30;

    [Header("Rate (ramps from start -> min over Ramp Duration)")]
    [SerializeField] private float startInterval = 2f;
    [SerializeField] private float minInterval = 0.4f;
    [Tooltip("Seconds over which spawning ramps to its fastest. 0 = always fastest.")]
    [SerializeField] private float rampDuration = 120f;

    [Header("Placement (ring around the player)")]
    [SerializeField] private float minRadius = 12f;
    [SerializeField] private float maxRadius = 20f;
    [Tooltip("Search radius for a NavMesh point at each candidate spot.")]
    [SerializeField] private float navSampleRadius = 4f;
    [SerializeField] private int placementAttempts = 8;

    private Transform _player;
    private int _alive;
    private float _nextSpawn;
    private float _startTime;

    private void Start()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) _player = go.transform;

        _startTime = Time.time;
        _nextSpawn = Time.time + startInterval;
    }

    private void Update()
    {
        if (_player == null || enemyTypes == null || enemyTypes.Length == 0) return;
        if (Time.time < _nextSpawn || _alive >= maxAlive) return;

        GameObject prefab = PickEnemy();
        if (prefab != null && TryGetSpawnPoint(out Vector3 pos))
            SpawnAt(prefab, pos);

        _nextSpawn = Time.time + CurrentInterval();
    }

    private float CurrentInterval()
    {
        float t = rampDuration <= 0f ? 1f : Mathf.Clamp01((Time.time - _startTime) / rampDuration);
        return Mathf.Lerp(startInterval, minInterval, t);
    }

    // Weighted random pick across the enemy types.
    private GameObject PickEnemy()
    {
        float total = 0f;
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            var e = enemyTypes[i];
            if (e != null && e.prefab != null) total += Mathf.Max(0f, e.weight);
        }
        if (total <= 0f) return null;

        float roll = Random.value * total;
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            var e = enemyTypes[i];
            if (e == null || e.prefab == null) continue;

            float w = Mathf.Max(0f, e.weight);
            if (roll < w) return e.prefab;
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
}
