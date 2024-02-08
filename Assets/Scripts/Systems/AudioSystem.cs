using System;
using UnityEngine;

/// <summary>
/// Insanely basic audio system which supports 3D sound.
/// Ensure you change the 'Sounds' audio source to use 3D spatial blend if you intend to use 3D sounds.
/// </summary>
public class AudioSystem : StaticInstance<AudioSystem> {
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _soundsSource;
    [SerializeField] private AudioClip _collisionClip;
    [SerializeField] private AudioClip _pickupClip;
    [SerializeField] private AudioClip _interactionClip;
    [SerializeField] private AudioClip _interactionFailedClip;
    [SerializeField] private AudioClip _dockClip;
    [SerializeField] private AudioClip _rocketLaunchClip;
    [SerializeField] private AudioClip _rocketExplosionClip;
    [SerializeField] private AudioClip _teleportClip;

    public void PlayMusic(AudioClip clip) {
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void PlaySound(AudioClip clip, Vector3 pos, float vol = 1) {
        _soundsSource.transform.position = pos;
        PlaySound(clip, vol);
    }

    public void PlaySound(AudioClip clip, float vol = 1) {
        _soundsSource.PlayOneShot(clip, vol);
    }

    public void PlayCollisionSound(CollisionEvent collisionEvent)
    {
        if (collisionEvent.Collision.relativeVelocity.magnitude > 1)
        {
            PlaySound(_collisionClip,
                collisionEvent.Collision.transform.position,
                Math.Min(
                    (float)Math.Sqrt(collisionEvent.Collision.relativeVelocity.magnitude / 4),
                    10)
            );
        }
    }

    public void PlayPickupSound()
    {
        PlaySound(_pickupClip, 2);
    }

    public void PlayInteractionSound()
    {
        PlaySound(_interactionClip, 2);
    }

    public void PlayInteractionFailedSound()
    {
        PlaySound(_interactionFailedClip, 2);
    }

    public void PlayDockSound()
    {
        PlaySound(_dockClip, 2);
    }

    public void PlayRocketLaunchSound()
    {
        PlaySound(_rocketLaunchClip, 3);
    }

    public void PlayRocketExplosionSound()
    {
        PlaySound(_rocketExplosionClip, 2);
    }

    public void PlayTeleportSound()
    {
        PlaySound(_teleportClip, 0.25f);
    }
}