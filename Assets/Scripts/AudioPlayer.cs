using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }
    private SourcePool pool;
    private List<Voice> activeAudio;

    void Awake(){
        Instance = this;
        if(Application.isPlaying)
            DontDestroyOnLoad(this);
        // setup runtime data structures
        pool = new SourcePool(0, 16);
        activeAudio = new List<Voice>();
    }

    private void OnApplicationQuit() {}

    public void Quit(){
        foreach(Voice voice in activeAudio)
            pool.Put(voice.Source);
        activeAudio.Clear();
        pool.Quit();
    }

    void LateUpdate(){
        // update state of all Voices
        for(int i = activeAudio.Count - 1; i >= 0; i--){
            // clean up list of playing audio
            if(activeAudio[i].Source.isPlaying == false) {
                pool.Put(activeAudio[i].Source);
                activeAudio.RemoveAt(i);
                continue;
            }

            // TODO: deal with fading, account for differences between fade ins and fade outs
            // Sample implementation
            float time = activeAudio[i].Source.time;
            float t = Mathf.Max(time, activeAudio[i].Asset.FadeInPointMs) / activeAudio[i].Asset.FadeOutPointMs;
            float fadeFactor = FadeBehaviour.GetFadeFactor(activeAudio[i].Asset.FadeType, t);
            //Debug.Log($"t = {t}, fade factor = {fadeFactor}");

            //TODO: the fadeFactor currently overrides any randomized volume, need to mix that
            //value with the fadeFactor here
            activeAudio[i].Source.volume = fadeFactor;
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
        // played the AudioSource
        s.Play();

        activeAudio.Add(new Voice() { Source = s, Asset = sound });
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
