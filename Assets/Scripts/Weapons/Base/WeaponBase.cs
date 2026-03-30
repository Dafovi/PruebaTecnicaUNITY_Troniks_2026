using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected WeaponDataSO _data;
    [SerializeField] protected WeaponAudioSO _audioData;
    [SerializeField] protected AudioSource _audioSource;

    protected float _lastShootTime;

    public virtual void StartFire() { }
    public virtual void StopFire() { }

    public virtual void Shoot()
    {
        if (Time.time < _lastShootTime + _data.FireRate)
            return;

        _lastShootTime = Time.time;
        ExecuteShoot();
        PlayShootAudio();
    }

    protected abstract void ExecuteShoot();

    protected virtual void PlayShootAudio()
    {
        if (_audioSource == null || _audioData == null)
            return;

        AudioClip clip = _audioData.GetRandomShootClip();

        if (clip == null)
            return;

        _audioSource.PlayOneShot(clip);
    }
}