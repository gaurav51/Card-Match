using UnityEngine;

public class AudioController : IAudioService
{
    private AudioSource audioSource;

    public AudioController(AudioSource source)
    {
        audioSource = source;
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
