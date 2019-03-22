using UnityEngine;
public class PriorityStealing_Test : MonoBehaviour {
    public AudioClip low, high;
    public bool fillWithLowPriority = true;
    public bool playNonFilling = false;

    //Use all available sources in the pool by filling them with either high or low priority sounds
    //Then use the playNonFilling to attempt to add the other type
    //This tests that high-priority voices can appropriately steal sources from low priorities
    //and that low-priority voices cannot steal from a source with high priority
    //(Note that 'high priority' refers to lower priority numbers)

    private void Awake() {
        new GameObject("sound").AddComponent<AudioPlayer>();
        
        AudioAsset asset = CreateAsset(fillWithLowPriority ? low : high);
        asset.Looping = true;
        asset.VolumeBase = AudioUtil.DecibelToVolumeFactor(-50);
        
        for(int i = 0; i < AudioSettings.GetConfiguration().numRealVoices; i++){
            asset.Play();
        }
    }

    private void LateUpdate() {
        if(playNonFilling == false)
            return;
        playNonFilling = false;
        
        AudioAsset asset = CreateAsset(fillWithLowPriority ? high : low);
        asset.Play();

        Debug.LogWarning("calling Debug.Break() to evaluate profiler manually");
    }

    private AudioAsset CreateAsset(AudioClip clip){
        AudioAsset asset = ScriptableObject.CreateInstance<AudioAsset>();
        asset.Attenuation = AttenuationMode.None;

        if(clip.name == low.name){
            asset.name = "low";
            asset.Priority = 100;
        }
        else {
            asset.name = "high";
            asset.Priority = 0;

        }
        asset.Clips = new AudioClip[1] { clip };
        return asset;
    }
}