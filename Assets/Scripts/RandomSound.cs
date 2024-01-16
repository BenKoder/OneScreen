using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    [SerializeField] private List<AudioClip> clipList;
    
    // Start is called before the first frame update
    void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        AudioClip clip = clipList[Random.Range(0, clipList.Count)];
        source.clip = clip;
        source.Play();
    }
}
