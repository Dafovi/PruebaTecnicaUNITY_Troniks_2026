using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapons/Weapon Audio")]
public class WeaponAudioSO : ScriptableObject
{
    public List<AudioClip> ShootClips;

    public AudioClip GetRandomShootClip()
    {
        if (ShootClips == null || ShootClips.Count == 0)
            return null;

        return ShootClips[Random.Range(0, ShootClips.Count)];
    }
}