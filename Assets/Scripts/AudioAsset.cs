using UnityEngine;

[CreateAssetMenu(fileName = "AudioAsset", menuName = "Audio/Create Asset")]
public class AudioAsset : ScriptableObject
{
    public AudioClip clip;

    public void Play(){
        AudioPlayer.Instance.Play(this);
    }
}
