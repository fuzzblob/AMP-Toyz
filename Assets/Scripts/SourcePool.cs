using System.Collections.Generic;
using UnityEngine;

public class SourcePool
{
    private Stack<AudioSource> data;
    private int maxCount;
    private int created;

    public SourcePool(int max = 0){
        maxCount = max;
        data = new Stack<AudioSource>(maxCount);
    }

    public AudioSource Get(){
        if(data.Count > 0)
            return data.Pop();
        else if(maxCount == 0 || created < maxCount)
            return Create();
        Debug.LogWarning("coudn't get AudioSource from pool!");
        return null;
    }

    public void Put(AudioSource source){
        data.Push(source);
    }

    private AudioSource Create(){
        created++;
        GameObject go = new GameObject("Source " + created.ToString("00"));
        AudioSource s = go.AddComponent<AudioSource>();
        s.spatialBlend = 0f;
        return s;
    }
}
