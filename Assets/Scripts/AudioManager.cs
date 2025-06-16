using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] sfxClips;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static void PlaySfx(int sfxIdx, float volume = 1f, float pitch = 1f)
    {
        Instance.sfxSource.volume = volume;
        Instance.sfxSource.pitch = pitch;
        Instance.sfxSource.PlayOneShot(Instance.sfxClips[sfxIdx]);
    }
}
