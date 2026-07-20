using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Switches between day and night when called (SetDay / SetNight / Toggle from your game
/// manager). The directional light's rotation, color temperature and intensity ease
/// smoothly from the current values to the target profile over Transition Seconds.
/// Night enables the spawn manager; day disables it. Exposes a universal IsNight flag +
/// events so other mechanics can react (e.g. monsters die at day).
/// </summary>
[DisallowMultipleComponent]
public class DayNightController : MonoBehaviour
{
    [System.Serializable]
    public class LightProfile
    {
        public Vector3 rotation;
        public float temperature;
        public float intensity;
    }

    [Header("Directional light")]
    [SerializeField] private Light directionalLight;

    [Header("Profiles")]
    [SerializeField]
    private LightProfile day = new LightProfile { rotation = new Vector3(36f, 380f, 1f), temperature = 4000f, intensity = 3f };
    [SerializeField]
    private LightProfile night = new LightProfile { rotation = new Vector3(36f, 585f, 1f), temperature = 10000f, intensity = 1f };

    [Header("Transition")]
    [Tooltip("Seconds to ease the light from its current values to the target profile.")]
    [SerializeField] private float transitionSeconds = 3f;
    [SerializeField] private bool startAtNight = false;

    [Header("Spawning")]
    [Tooltip("Enabled at night, disabled during day (assign your EnemySpawner).")]
    [SerializeField] private Behaviour spawnManager;

    [Header("Events")]
    [SerializeField] private UnityEvent onBecameDay;
    [SerializeField] private UnityEvent onBecameNight;

    /// <summary>Universal flag — read from anywhere: DayNightController.IsNight.</summary>
    public static bool IsNight { get; private set; }
    /// <summary>Fired on every phase change; argument is the new isNight value.</summary>
    public static event Action<bool> PhaseChanged;

    // Current (raw) light values we ease from — kept raw so the given >360 rotation eases predictably.
    private Vector3 _curRot;
    private float _curTemp;
    private float _curInt;
    private Coroutine _transition;

    private void Start()
    {
        if (directionalLight != null) directionalLight.useColorTemperature = true;

        LightProfile p = startAtNight ? night : day;
        _curRot = p.rotation; _curTemp = p.temperature; _curInt = p.intensity;
        ApplyCurrent();

        IsNight = startAtNight;
        if (spawnManager != null) spawnManager.enabled = startAtNight;
    }

    // --- Public API (call from your game manager) ---------------------------

    public void SetNight() => SetPhase(true);
    public void SetDay() => SetPhase(false);
    public void Toggle() => SetPhase(!IsNight);

    // ------------------------------------------------------------------------

    private void SetPhase(bool toNight)
    {
        IsNight = toNight;

        // Spawning on at night, off during day.
        if (spawnManager != null) spawnManager.enabled = toNight;

        // Fire hooks so other mechanics can react.
        if (toNight) onBecameNight?.Invoke(); else onBecameDay?.Invoke();
        PhaseChanged?.Invoke(toNight);

        // Built-in mechanic: daybreak purges all monsters.
        if (!toNight) KillAllMonsters();

        // Ease the light to the target profile (visual only — the phase already changed).
        if (_transition != null) StopCoroutine(_transition);
        _transition = StartCoroutine(LightTransition(toNight ? night : day));
    }

    private IEnumerator LightTransition(LightProfile target)
    {
        Vector3 startRot = _curRot;
        float startTemp = _curTemp;
        float startInt = _curInt;

        float t = 0f, dur = Mathf.Max(0.01f, transitionSeconds);
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            _curRot = Vector3.Lerp(startRot, target.rotation, k);
            _curTemp = Mathf.Lerp(startTemp, target.temperature, k);
            _curInt = Mathf.Lerp(startInt, target.intensity, k);
            ApplyCurrent();
            yield return null;
        }

        _curRot = target.rotation; _curTemp = target.temperature; _curInt = target.intensity;
        ApplyCurrent();
        _transition = null;
    }

    private void ApplyCurrent()
    {
        if (directionalLight == null) return;
        directionalLight.transform.eulerAngles = _curRot;
        directionalLight.colorTemperature = _curTemp;
        directionalLight.intensity = _curInt;
    }

    private void KillAllMonsters()
    {
        // Monsters = things with EnemyAI (not passive animals, which use AnimalAI).
        var monsters = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < monsters.Length; i++)
        {
            var hp = monsters[i].GetComponent<EnemyHealth>();
            if (hp != null) hp.Kill();
        }
    }
}
