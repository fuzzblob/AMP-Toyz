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
        else if(created < maxCount)
            return Create();
        return null;
    }

    public void Put(AudioSource source){
        data.Push(source);
    }

    private AudioSource Create(){
        created++;
        var go = new GameObject("Source");
        var s = go.AddComponent<AudioSource>();
        s.spatialBlend = 0f;
        return s;
    }
}
