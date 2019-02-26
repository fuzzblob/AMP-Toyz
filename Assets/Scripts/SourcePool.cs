using System.Collections.Generic;
using UnityEngine;

public class SourcePool
{
    private Stack<AudioSource> data;
    private int maxCount;
    private int created;
    private GameObject prefab;

    public SourcePool(int max = 0, int min = 0){
        maxCount = max;
        data = new Stack<AudioSource>(maxCount);
        CreatePrefab();
        for(int i = 0; i < min; i++){
            Put(Create());
        }
    }

    public void Quit(){
#if UNITY_EDITOR
        GameObject.DestroyImmediate(prefab);
        while(data.Count > 0){
            GameObject.DestroyImmediate(data.Pop().gameObject);
        }
#else
        GameObject.Destroy(prefab);
        while(data.Count > 0){
            GameObject.Destroy(data.Pop().gameObject);
        }
#endif
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
        GameObject go = GameObject.Instantiate(prefab, AudioPlayer.Instance.transform);
        go.name = "Source " + created.ToString("00");
        return go.GetComponent<AudioSource>();
    }

    private void CreatePrefab(){
        prefab = new GameObject();
        prefab.transform.parent = AudioPlayer.Instance.transform;
        
        var s = prefab.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.reverbZoneMix = 0f;
        s.dopplerLevel = 0f;
        s.spatialBlend = 0f;
    }
}
