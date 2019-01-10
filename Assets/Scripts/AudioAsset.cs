using UnityEngine;

[CreateAssetMenu(fileName = "AudioAsset", menuName = "Audio/Create Asset")]
public class AudioAsset : ScriptableObject
{
    public AudioClip Clip;
    [Range(-24, 24)]
    public float PitchBase;
    [Range(0, 24)]
    public float PitchOffset;
    [Range(-80, 0)]
    public float VolumeBase;
    [Range(0, 20)]
    public float VolumeOffset;

    public void Play(){
        AudioPlayer.Instance.Play(this);
    }
}
