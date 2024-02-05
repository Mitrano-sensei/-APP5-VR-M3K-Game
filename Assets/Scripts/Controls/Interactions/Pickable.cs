using System;
using UnityEngine;
using UnityEngine.Events;

/**
 * Objects that can be picked up.
 */
public class Pickable : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private OnPickEvent _onPick = new OnPickEvent();
    [SerializeField] private OnUnPickEvent _onUnPick = new OnUnPickEvent();

    [Header("Misc")]
    [SerializeField] private bool _useRotationCorrecter = false;
    [SerializeField] private Vector3 _correctPickRotation = new Vector3();

    private Transform _originParent;
    private VelocityCalculator _velocityCalculator;

    public OnPickEvent OnPick { get => _onPick; }
    public OnUnPickEvent OnUnPick { get => _onUnPick; }
    public Transform OriginParent { get => _originParent; set => _originParent = value; }
    public Vector3 CorrectRotation { get => _correctPickRotation; set => _correctPickRotation = value; }
    public bool UseRotationCorrecter { get => _useRotationCorrecter; set => _useRotationCorrecter = value; }

    protected LogManager _logger;

    void Awake()
    {
        _originParent = transform.parent;
    }

    protected virtual void Start()
    {
        _logger = LogManager.Instance;

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        OnPick.AddListener(() =>
        {
            rb.isKinematic = true;
        });

        OnUnPick.AddListener(() => {
            rb.isKinematic = false;
            rb.velocity = _velocityCalculator.Velocity;
        });


        if (GetComponent<VelocityCalculator>() == null)
        {
            gameObject.AddComponent<VelocityCalculator>();
        }
        _velocityCalculator = GetComponent<VelocityCalculator>();
    }
}

#region Events
/**
 * Event invoked when a pickable object is picked up.
 */
[Serializable] public class OnPickEvent : UnityEvent { }
/**
 * Event invoked when a pickable object is released.
 */
[Serializable] public class OnUnPickEvent : UnityEvent { }
#endregion
