using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip flipClip;
    public AudioClip matchClip;
    public AudioClip mismatchClip;
    public AudioClip gameOverClip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFlip() => PlaySound(flipClip);
    public void PlayMatch() => PlaySound(matchClip);
    public void PlayMismatch() => PlaySound(mismatchClip);
    public void PlayGameOver() => PlaySound(gameOverClip);

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
