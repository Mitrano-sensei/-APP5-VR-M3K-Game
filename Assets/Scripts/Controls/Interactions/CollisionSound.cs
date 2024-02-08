using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CollisionEventHandler))]
public class CollisionSound : MonoBehaviour
{
    private AudioSystem _audioSystem;
    private CollisionEventHandler _ceHandler;

    // Start is called before the first frame update
    void Start()
    {
        _audioSystem = AudioSystem.Instance;
        _ceHandler = GetComponent<CollisionEventHandler>();

        _ceHandler.OnCollision.AddListener(e => _audioSystem.PlayCollisionSound(e));
    }
}
