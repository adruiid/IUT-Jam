using System;
using TMPro;
using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    [SerializeField] private float dayLength = 480f; // 8 minutes
    private float gameMinutes;
    [SerializeField]private TextMeshProUGUI timerText;

    public event Action nightFall;
    public event Action dayChange;
    private void Start()
    {
        gameMinutes = 6 * 60;
    }


    private void Update()
    {
        gameMinutes += Time.deltaTime * (1440f / dayLength);

        gameMinutes %= 1440f;

        int hours = Mathf.FloorToInt(gameMinutes / 60);

        if (hours == 20) nightFall?.Invoke();

        if (hours == 0 & gameMinutes != 0) dayChange.Invoke();



        timerText.text = $"{hours:00}:00";
    }
}
