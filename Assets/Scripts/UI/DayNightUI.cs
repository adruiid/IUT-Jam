using UnityEngine;
using TMPro;

/// <summary>
/// Drives TextMeshPro texts from the DayNightController — one for the clock ("13:00")
/// and one for the day ("Day 1"). Only rewrites the text when the value actually
/// changes, so it doesn't allocate a string every frame.
/// </summary>
[DisallowMultipleComponent]
public class DayNightUI : MonoBehaviour
{
    [SerializeField] private DayNightController dayNight;

    [Header("Texts")]
    [Tooltip("Shows the clock, e.g. 13:00.")]
    [SerializeField] private TMP_Text timeText;
    [Tooltip("Shows the day, e.g. Day 1.")]
    [SerializeField] private TMP_Text dayText;

    [Tooltip("{0} is replaced by the day number.")]
    [SerializeField] private string dayFormat = "Day {0}";

    private int _lastHour = -1;
    private int _lastDay = -1;

    private void Awake()
    {
        if (dayNight == null) dayNight = FindAnyObjectByType<DayNightController>();
    }

    private void Update()
    {
        if (dayNight == null) return;

        if (timeText != null)
        {
            // Show whole hours only — update when the hour changes, not every minute.
            int hour = Mathf.FloorToInt(dayNight.Hour);
            if (hour != _lastHour)
            {
                _lastHour = hour;
                timeText.text = $"{hour:00}:00";
            }
        }

        if (dayText != null && dayNight.Day != _lastDay)
        {
            _lastDay = dayNight.Day;
            dayText.text = string.Format(dayFormat, dayNight.Day);
        }
    }
}
