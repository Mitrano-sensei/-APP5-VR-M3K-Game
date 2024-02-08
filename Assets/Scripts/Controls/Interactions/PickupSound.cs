using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Pickable))]

public class PickupSound : MonoBehaviour
{
    private AudioSystem _audioSystem;
    private Pickable _pickable;

    // Start is called before the first frame update
    void Start()
    {
        _audioSystem = AudioSystem.Instance;
        _pickable = GetComponent<Pickable>();
        _pickable.OnPick.AddListener(() => _audioSystem.PlayPickupSound());
    }
}
