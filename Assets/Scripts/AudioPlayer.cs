using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioAsset sound;

    void Start()
    {
        sound.Play();
    }
}
