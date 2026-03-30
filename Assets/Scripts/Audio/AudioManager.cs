using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioLibrarySO _library;
    [SerializeField] private AudioSource _source;

    public void PlayRandomShoot()
    {
        var clip = GetRandom(_library.ShootSounds);
        _source.PlayOneShot(clip);
    }

    private AudioClip GetRandom(List<AudioClip> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}