using UnityEngine;

[CreateAssetMenu(fileName = "AudioAsset", menuName = "Audio/Create Asset")]
public class AudioAsset : ScriptableObject
{
    public AudioClip Clip;
    public float PitchBase;
    public float PitchOffset;
    public float VolumeBase;
    public float VolumeOffset;

    public void Play(){
        AudioPlayer.Instance.Play(this);
    }
}
