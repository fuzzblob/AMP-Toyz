using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }
    private SourcePool pool;
    private List<AudioSource> activeAudio;

    [Header("Debug")]
    public AudioAsset Sound;

    void Awake(){
        Instance = this;
        DontDestroyOnLoad(this);
        // setup runtime data structures
        pool = new SourcePool(0, 16);
        activeAudio = new List<AudioSource>();
    }

    private void OnApplicationQuit() {}

    public void Quit(){
        foreach(AudioSource s in activeAudio)
            pool.Put(s);
        pool.Quit();
    }

    void LateUpdate(){
        // clean up list of playing audio
        for(int i = activeAudio.Count - 1; i >= 0; i--){
            if(activeAudio[i].isPlaying)
                continue;
            pool.Put(activeAudio[i]);
            activeAudio.RemoveAt(i);
        }
    }

    public void Play(AudioAsset sound){
        var s = pool.Get();
        if(s == null)
            return;
        // set source properties
        s.clip = GetNextClip(sound);
        // apply pitch and volume randomization
        float pitchSemitones = sound.PitchBase + Random.Range(-sound.PitchOffset / 2f, sound.PitchOffset / 2f);
        float volumeDecibels = sound.VolumeBase + Random.Range(-sound.VolumeOffset / 2f, sound.VolumeOffset / 2f);
        s.pitch = AudioUtil.SemitoneToPitchFactor(pitchSemitones);
        s.volume = AudioUtil.DecibelToVolumeFactor(volumeDecibels);
        // playe the AudioSource
        s.Play();
        activeAudio.Add(s);
    }

    private AudioClip GetNextClip(AudioAsset sound){
        int index = sound.LastIndex;
        int length = sound.Clips.Length;

        Debug.LogWarning(Random.Range(0, 0));

        switch(sound.PlayMode){
        case PlaylistMode.Sequence:
            if(++index >= length)
                index = 0;
            break;
        case PlaylistMode.Random:
            do{
                index = Random.Range(0, length);
            }
            // prevent repeating index unless only two options
            while(index == sound.LastIndex && length > 2);
            break;
        case PlaylistMode.Shuffle:
            index = GetNextShuffled(sound, index, length);
            break;
        case PlaylistMode.WeightedRandom:
            index = GetNextWeightedRandom(sound);
            break;
        default:
            Debug.LogError(sound.PlayMode + " is not implemented yet!");
            return null;
        }
        sound.LastIndex = index;
        Debug.Log(index);
        return sound.Clips[index];
    }

    private int GetNextShuffled(AudioAsset sound, int index, int length){
        if(sound.ShuffleQueue == null)
                sound.ShuffleQueue = new Queue<int>(length);
            if(sound.ShuffleQueue.Count ==0){
                // TODO: infinyte loop if playlist empty?
                while(index == sound.LastIndex)
                    index = Random.Range(0, length);
                sound.ShuffleQueue.Enqueue(index);
                // reshuffle
                while(sound.ShuffleQueue.Count < length){
                    int num = Random.Range(0, length);
                    if(sound.ShuffleQueue.Contains(num) == false)
                        sound.ShuffleQueue.Enqueue(num);
                }
            }
        
        return index;
    }

    private int GetNextWeightedRandom(AudioAsset sound){
        //TODO: UX issue here that is maybe unintuitive: total values don't always add to 100%,
        //they can be more or less!
        //We account for that here by getting a random number using the sum of all weights,
        //then using each weight value as a threshold for "landing on" the associated clip

        float sum = 0f;
        for (int i = 0; i < sound.Weights.Count; i++){
            sum += sound.Weights[i];
        }

        float random = Random.Range(0f, sum);
        float nextThreshold = 0f;

        int index = 0;
        for (int i = 0; i < sound.Weights.Count; i++) {
            nextThreshold += sound.Weights[i];
            if (random < nextThreshold) {
                index = i;
                break;
            }
        }
        
        return index;
    }
}
