using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RocketScript))]
public class RocketSound : MonoBehaviour
{
    private AudioSystem _audioSystem;
    private RocketScript _rocketScript;

    // Start is called before the first frame update
    void Start()
    {
        _audioSystem = AudioSystem.Instance;
        _rocketScript = GetComponent<RocketScript>();
        _rocketScript.OnLaunch.AddListener(() => _audioSystem.PlayRocketLaunchSound());
        _rocketScript.OnExplode.AddListener(() => _audioSystem.PlayRocketExplosionSound());
    }
}
