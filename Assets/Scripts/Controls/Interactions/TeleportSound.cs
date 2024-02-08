using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AbstractControlWatcher))]
public class TeleportSound : MonoBehaviour
{
    private AbstractControlWatcher _abstractControlWatcher;
    private AudioSystem _audioSystem;

    // Start is called before the first frame update
    void Start()
    {
        _audioSystem = AudioSystem.Instance;
        _abstractControlWatcher = GetComponent<AbstractControlWatcher>();
        _abstractControlWatcher.OnTeleportEvent.AddListener(e => _audioSystem.PlayTeleportSound());
    }
}
