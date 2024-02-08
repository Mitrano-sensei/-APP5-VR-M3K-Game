using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Dockable))]
public class DockSound : MonoBehaviour
{
    private AudioSystem _audioSystem;
    private Dockable _dockable;

    // Start is called before the first frame update
    void Start()
    {
        _audioSystem = AudioSystem.Instance;
        _dockable = GetComponent<Dockable>();
        _dockable.OnDock.AddListener(e => _audioSystem.PlayDockSound());
    }
}
