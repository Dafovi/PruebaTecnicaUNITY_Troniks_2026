using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Audio Library")]
public class AudioLibrarySO : ScriptableObject
{
    public List<AudioClip> ShootSounds;
    public List<AudioClip> HitSounds;
    public List<AudioClip> EnemySounds;
}