using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip music;
    private bool _musicPlaying;

    private void Awake()
    {
        music.LoadAudioData();
    }

    private void Start()
    {
        Invoke(nameof(PlayMusicOne), 25f);
    }

    private void PlayMusicOne()
    {
        if (_musicPlaying) return;

        source.clip = music;
        source.Play();

        _musicPlaying = true;
    }
}
