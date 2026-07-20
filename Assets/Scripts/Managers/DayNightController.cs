using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Self-driving day/night cycle with an internal clock.
///   - Starts at 6:00 on Day 1. Day = 06:00–20:00, Night = 20:00–06:00.
///   - Each new morning (06:00) advances the day counter, up to Days To Survive (3).
///   - Surviving past the final night fires onSurvived (hook your game-over/win screen).
///   - The directional light eases (dawn/dusk) over the Ramp Hours BEFORE each boundary,
///     so the change starts a little early like real twilight — not abruptly at 6/20.
///   - Night enables the spawn manager; day disables it and purges monsters.
/// Day + time are exposed for a TextMeshPro UI (Day, Hour, TimeString).
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

    [Header("Clock")]
    [Tooltip("Real seconds for a full 24-hour in-game day.")]
    [SerializeField] private float dayLengthSeconds = 120f;
    [SerializeField] private float startHour = 6f;
    [SerializeField] private float dayStartHour = 6f;
    [SerializeField] private float nightStartHour = 20f;
    [Tooltip("Hours over which dawn/dusk eases the light, ending at each boundary (real twilight).")]
    [SerializeField] private float rampHours = 2f;
    [Tooltip("Survive this many days; morning after the final night fires onSurvived.")]
    [SerializeField] private int daysToSurvive = 3;

    [Header("Spawning")]
    [Tooltip("Enabled at night, disabled during day (assign your EnemySpawner).")]
    [SerializeField] private Behaviour spawnManager;

    [Header("Events")]
    [SerializeField] private UnityEvent onBecameDay;
    [SerializeField] private UnityEvent onBecameNight;
    [Tooltip("Fired the morning after the final night — the player survived. Hook your game-over/win screen.")]
    [SerializeField] private UnityEvent onSurvived;

    [Header("Music")]
    [Tooltip("AudioSource that plays the day/night tracks (set to loop).")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip dayMusic;
    [SerializeField] private AudioClip nightMusic;
    [Tooltip("Seconds of silence at the START and END of each phase (music only plays in between).")]
    [SerializeField] private float musicSilenceSeconds = 10f;

    /// <summary>Universal flag — read from anywhere: DayNightController.IsNight.</summary>
    public static bool IsNight { get; private set; }
    /// <summary>Fired on every phase boundary; argument is the new isNight value.</summary>
    public static event Action<bool> PhaseChanged;

    // --- Exposed for UI ---
    public int Day { get; private set; } = 1;
    public float Hour => _hour;
    public bool IsNightNow => _isNight;
    /// <summary>Formatted clock, e.g. "06:00" — bind to a TextMeshPro text.</summary>
    public string TimeString
    {
        get { int h = (int)_hour; int m = (int)((_hour - h) * 60f); return $"{h:00}:{m:00}"; }
    }

    private float _hour;
    private bool _isNight;
    private bool _ended;
    private float _phaseStartTime;     // real time the current phase began
    private float _phaseDurationReal;  // real seconds the current phase lasts

    private float RealSecondsPerHour => dayLengthSeconds / 24f;
    private float DayHours => nightStartHour - dayStartHour;            // e.g. 14
    private float NightHours => 24f - (nightStartHour - dayStartHour);  // e.g. 10

    private void Start()
    {
        if (directionalLight != null) directionalLight.useColorTemperature = true;

        _hour = startHour;
        Day = 1;
        _isNight = IsNightAt(_hour);
        IsNight = _isNight;
        if (spawnManager != null) spawnManager.enabled = _isNight;

        ApplyLight(DayFactor(_hour));

        // Music phase timing — account for starting partway through a phase.
        float hoursIn = _isNight
            ? (_hour >= nightStartHour ? _hour - nightStartHour : _hour + (24f - nightStartHour))
            : (_hour - dayStartHour);
        _phaseDurationReal = (_isNight ? NightHours : DayHours) * RealSecondsPerHour;
        _phaseStartTime = Time.time - hoursIn * RealSecondsPerHour;
        if (musicSource != null) musicSource.loop = true;
    }

    private void Update()
    {
        if (_ended) return;

        // Advance the clock.
        _hour += (24f / Mathf.Max(1f, dayLengthSeconds)) * Time.deltaTime;
        if (_hour >= 24f) _hour -= 24f;

        // Ease the light by time of day (dawn/dusk ramps handle the early transition).
        ApplyLight(DayFactor(_hour));

        // Handle phase boundary crossings.
        bool night = IsNightAt(_hour);
        if (night != _isNight)
        {
            _isNight = night;
            IsNight = night;
            if (night) EnterNight(); else EnterDay();
        }

        UpdateMusic();
    }

    // --- Optional manual control (jumps the clock to that phase) -------------
    public void SetDay() => _hour = dayStartHour;
    public void SetNight() => _hour = nightStartHour;

    // ------------------------------------------------------------------------

    private void EnterNight()
    {
        if (spawnManager != null) spawnManager.enabled = true;
        onBecameNight?.Invoke();
        PhaseChanged?.Invoke(true);

        _phaseStartTime = Time.time;
        _phaseDurationReal = NightHours * RealSecondsPerHour;
    }

    private void EnterDay()
    {
        // New morning: advance the day, or end the run if the final night is survived.
        if (Day >= daysToSurvive)
        {
            _ended = true;
            if (spawnManager != null) spawnManager.enabled = false;
            KillAllMonsters();
            SetMusic(null); // silence on the win screen
            onSurvived?.Invoke(); // game-over / win screen
            return;
        }

        Day++;
        if (spawnManager != null) spawnManager.enabled = false;
        KillAllMonsters();
        onBecameDay?.Invoke();
        PhaseChanged?.Invoke(false);

        _phaseStartTime = Time.time;
        _phaseDurationReal = DayHours * RealSecondsPerHour;
    }

    // Plays the phase track, but stays silent for the first/last musicSilenceSeconds of the phase.
    private void UpdateMusic()
    {
        if (musicSource == null) return;

        float elapsed = Time.time - _phaseStartTime;
        float remaining = _phaseDurationReal - elapsed;
        bool silent = elapsed < musicSilenceSeconds || remaining < musicSilenceSeconds;

        SetMusic(silent ? null : (_isNight ? nightMusic : dayMusic));
    }

    private void SetMusic(AudioClip clip)
    {
        if (musicSource == null) return;

        if (clip == null)
        {
            if (musicSource.isPlaying) musicSource.Stop();
            return;
        }
        if (musicSource.clip != clip || !musicSource.isPlaying)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    private bool IsNightAt(float h) => h >= nightStartHour || h < dayStartHour;

    // 1 = full day, 0 = full night, eased over rampHours before each boundary.
    private float DayFactor(float h)
    {
        float dawnStart = dayStartHour - rampHours;   // e.g. 04:00
        float duskStart = nightStartHour - rampHours; // e.g. 18:00

        if (h < dawnStart) return 0f;                                        // deep night
        if (h < dayStartHour) return Mathf.Clamp01((h - dawnStart) / rampHours);   // dawn
        if (h < duskStart) return 1f;                                        // full day
        if (h < nightStartHour) return Mathf.Clamp01(1f - (h - duskStart) / rampHours); // dusk
        return 0f;                                                           // night
    }

    private void ApplyLight(float dayFactor)
    {
        if (directionalLight == null) return;
        directionalLight.transform.eulerAngles = Vector3.Lerp(night.rotation, day.rotation, dayFactor);
        directionalLight.colorTemperature = Mathf.Lerp(night.temperature, day.temperature, dayFactor);
        directionalLight.intensity = Mathf.Lerp(night.intensity, day.intensity, dayFactor);
    }

    private void KillAllMonsters()
    {
        var monsters = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < monsters.Length; i++)
        {
            var hp = monsters[i].GetComponent<EnemyHealth>();
            if (hp != null) hp.Kill();
        }
    }
}
