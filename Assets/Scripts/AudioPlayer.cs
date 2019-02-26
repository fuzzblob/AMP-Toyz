using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

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
        float deltaTime = Time.deltaTime; // TODO: maybe use Time.unscaledDeltaTime?
        // update state of all Voices
        for(int i = activeAudio.Count - 1; i >= 0; i--){
            Voice voice = activeAudio[i];
            if(voice == null){
                activeAudio.RemoveAt(i);
                continue;
            }
            if(voice.Source == null){
                Debug.LogError("Voice doesn't have an AudioSource!");
                continue;
            }
            // clean up list of playing audio
            if(voice.Source.isPlaying == false) {
                pool.Put(voice.Source);
                activeAudio.RemoveAt(i);
                continue;
            }
            // TODO: fade out at end of file?
            /* if(voice.Source.loop == false
                && voice.Fader.FadeType == FadeType.NONE
                && voice.Asset.FadeTypeOut != FadeType.NONE
                && voice.Source.time + voice.Asset.FadeTimeOut >= voice.Source.clip.length)
            {
                voice.Fader.Fade(
                    voice.Fader.FadeVolume, 0f,
                    voice.Asset.FadeTimeOut, voice.Asset.FadeTypeOut);
            }*/
            // claculate fade volume factor
            if(voice.Fader.FadeType != FadeType.NONE) {
                voice.Fader.UpdateFade(deltaTime);
            }
            //value with the fadeFactor here
            if(voice.Source != null)
                voice.Source.volume = voice.Volume * voice.Fader.Volume;
        }
    }

    public Voice Play(AudioAsset sound) {
        Voice voice = new Voice() {
            Asset = sound
        };
        voice.Fader.Reset();
        // get AudioClip
        voice.Clip = GetNextClip(sound);
        // fading in
        if(sound.FadeTypeIn != FadeType.NONE){
            voice.Fader.Fade(0f, 1f, sound.FadeTimeIn, sound.FadeTypeIn);
        }
        // apply pitch and volume randomization
        float pitchSemitones = sound.PitchBase + Random.Range(-sound.PitchOffset / 2f, sound.PitchOffset / 2f);
        float volumeDecibels = sound.VolumeBase + Random.Range(-sound.VolumeOffset / 2f, sound.VolumeOffset / 2f);
        voice.Pitch = AudioUtil.SemitoneToPitchFactor(pitchSemitones);
        voice.Volume = AudioUtil.DecibelToVolumeFactor(volumeDecibels);
        // set distance attentuation properties
        voice.Spatialized = sound.Spatialized;
        voice.MinDistance = sound.MinimumDistance;
        voice.MaxDistance = sound.MaximumDistance;

        activeAudio.Add(voice);
        // assign AudioSource
        AudioSource s = pool.Get();
        if(s == null){
            // TODO: set voice virtual
            Debug.LogError("can't assign AudioSource for sound asset " + sound);
            return voice;
        }
        voice.Source = s;
        s.clip = voice.Clip;
        s.volume = voice.Volume * voice.Fader.Volume;
        s.pitch = voice.Pitch;
        s.spatialBlend = voice.Spatialized ? 1f : 0f;
        s.minDistance = voice.MinDistance;
        s.maxDistance = voice.MaxDistance;
        s.rolloffMode = AudioRolloffMode.Linear;
        // played the AudioSource
        s.Play();
        return voice;
    }

    public void Stop(Voice voice, float fadeOutTime = -1f){
        if(voice.Asset.FadeTypeOut == FadeType.NONE){
            StopVoice(voice);
            return;
        }
        // calculate fade time
        if(fadeOutTime >= 0f)
            fadeOutTime = Mathf.Min(fadeOutTime, voice.Asset.FadeTimeOut);
        else
            fadeOutTime = voice.Asset.FadeTimeOut;
        // start fading
        voice.Fader.Fade(
            voice.Fader.Volume, 0f,
            fadeOutTime, voice.Asset.FadeTypeIn,
            // make the fader stop the voice when done
            delegate { StopVoice(voice); });
    }

    private void StopVoice(Voice voice)
    {
        voice.Source.Stop();
        pool.Put(voice.Source);
        voice.Source = null;
        activeAudio.Remove(voice);
    }

    private AudioClip GetNextClip(AudioAsset sound){
        int index = sound.LastIndex;
        int length = sound.Clips.Length;

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
