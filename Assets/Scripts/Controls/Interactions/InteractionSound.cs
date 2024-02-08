using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class InteractionSound : MonoBehaviour
{
    private AudioSystem _audioSystem;
    private Interactable _interactable;

    // Start is called before the first frame update
    void Start()
    {
        _audioSystem = AudioSystem.Instance;
        _interactable = GetComponent<Interactable>();
        _interactable.OnInteraction.AddListener(e => _audioSystem.PlayInteractionSound());
        _interactable.OnInteractionFailed.AddListener(e => _audioSystem.PlayInteractionFailedSound());
    }
}
