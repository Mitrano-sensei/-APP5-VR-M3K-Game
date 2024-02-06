using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * Controls watcher for the FPS controls.
 **/
public class FPSControlsWatcher : AbstractControlWatcher
{
    [Header("Player")]
    [SerializeField] private GameObject _player;

    private Hoverable _oldTarget;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        _logger = _logger ?? LogManager.Instance;

        OnTeleportEvent.AddListener((Vector3 newPos) =>
        {
            _player.transform.position = newPos;
        });


        // Specific for the FPS controls
        Sequence mover = null;
        OnGrabEvent.AddListener((Pickable pickable) =>
        {
            var dockable = pickable.GetComponent<Dockable>();

            pickable.transform.SetParent(Camera.main.transform);
            mover = DOTween.Sequence();
            mover = mover.Append(pickable.transform.DOLocalMove(Vector3.forward + (dockable != null ? dockable.CenterPosition * 0.15f : Vector3.zero), .5f).SetEase(Ease.InOutQuad));
            
            if (pickable.UseRotationCorrecter)
            {
                pickable.transform.DOLocalRotate(pickable.CorrectRotation, .5f).SetEase(Ease.InOutQuad);
            }

            if (dockable != null)
            {
                var rotationOffset = new Vector3(-90, 0, 0); // Because Correct rotation is for the item to dock, not to be picked. 
                mover.Join(pickable.transform.DOLocalRotate(-dockable.CorrectRotation + rotationOffset, .5f).SetEase(Ease.InOutQuad));
            }

            Helpers.MoveUntilDie(pickable.transform, Camera.main.gameObject, mover);
        });

        OnReleaseEvent.AddListener((ReleasedEvent releasedEvent) =>
        {
            mover?.Kill();
            mover = null;

            if (releasedEvent.GetDocker() != null && releasedEvent.GetDocker().IsActive && releasedEvent.GetDocker().IsAvailable) return;     // Here we let base OnDock event handle the docking.

            var releasedObject = releasedEvent.ReleasedObject;
            var rb = releasedObject.GetComponent<Rigidbody>();
            releasedObject.transform.SetParent(releasedObject.OriginParent != null ?  releasedObject.OriginParent : _cockpitEnvironment.transform);
        });
    }

    private void Update()
    {
        // Calls Teleport event if the player presses the right mouse button.
        if (Input.GetMouseButtonDown(1))
        {
            // Raycast from main camera to next object.
            RaycastHit hit;
            if (Helpers.HitBehindGrabbedObject(GrabbedObject?.gameObject, out hit))
            {
                // If the object is a teleporter, invoke the event.
                if (hit.collider.gameObject.GetComponent<TeleportationArea>())
                {
                    OnTeleportEvent.Invoke(hit.point);
                }
            }
        }

        // Calls Grab Event when the player presses E and is looking at a pickable object.
        if (Input.GetKeyDown(KeyCode.E) && this.GrabbedObject == null)
        {
            _logger.Trace("E Button");

            // Raycast from main camera to next object.
            RaycastHit hit;
            if (Helpers.HitBehindGrabbedObject(GrabbedObject?.gameObject, out hit))
            {
                // If the object is a pickable, invoke the event.
                if (hit.collider.gameObject.GetComponent<Pickable>())
                {
                    OnGrabEvent.Invoke(hit.collider.gameObject.GetComponent<Pickable>());
                }
            }
        }
        else if (this.GrabbedObject != null && !Input.GetKey(KeyCode.E))
        {
            _logger.Trace("Releasing Button");

            // Raycast from main camera to next object.
            RaycastHit hit;
            Helpers.HitBehindGrabbedObject(GrabbedObject?.gameObject, out hit);
            var docker = hit.collider?.gameObject?.GetComponent<Docker>(); 
            OnReleaseEvent.Invoke(docker == null ? new ReleasedEvent(GrabbedObject) : new ReleasedEvent(GrabbedObject, docker));
        }

        // Calls Interact Event when the player presses left click and is looking at an interactable object.
        if (Input.GetMouseButtonDown(0))
        {
            _logger.Trace("Left Mouse Button");

            // Raycast from main camera to next object.
            RaycastHit hit;
            if (Helpers.HitBehindGrabbedObject(GrabbedObject?.gameObject, out hit))
            {
                Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();
                // If the object is an interactable, invoke the event.
                if (interactable != null && interactable.enabled)
                {
                    OnInteractEvent.Invoke(interactable);
                    return;
                }

                // Here we touched an item that is not an interactable.
                var usableItem = GrabbedObject?.GetComponent<UsableItem>();
                if (usableItem != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    usableItem.OnUse.Invoke(new UseEvent(hitObject));
                    return;
                }

            }

            // Here we check if the object is a usable item.
            // In that case we are using it on nothing (null). So items with no target can be used.
            var usableItem_ = GrabbedObject?.GetComponent<UsableItem>();
            if (usableItem_ != null)
            {             
                usableItem_.OnUse.Invoke(new UseEvent());
                return;
            }
        }
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            if (Helpers.HitBehindGrabbedObject(GrabbedObject?.gameObject, out hit))
            {
                Holdable holdable = hit.collider.gameObject.GetComponent<Holdable>();
                if (holdable != null)
                {
                    holdable.Hold();
                }
            }
        }

        HoverCheck();
    }

    private void HoverCheck()
    {
        RaycastHit hit;
        if (Helpers.HitBehindGrabbedObject(GrabbedObject?.gameObject, out hit))
        {
            var newTarget = hit.collider?.gameObject?.GetComponent<Hoverable>();
            // From no target to a target
            if (_oldTarget == null && newTarget != null) {
                if (GrabbedObject == null)
                    newTarget.OnHoverEnter.Invoke(new OnHoverEnterEvent(hit.point));
                else
                    newTarget.OnHoverEnter.Invoke(new OnHoverEnterEvent(hit.point, GrabbedObject.gameObject));
            }
            // From a target to another
            else if (_oldTarget != null && _oldTarget != newTarget && newTarget != null)
            {
                _oldTarget.OnHoverExit.Invoke(new OnHoverExitEvent());
                newTarget.OnHoverEnter.Invoke(new OnHoverEnterEvent(hit.point));

                if (GrabbedObject == null)
                    _oldTarget.OnHoverExit.Invoke(new OnHoverExitEvent());
                else
                    _oldTarget.OnHoverExit.Invoke(new OnHoverExitEvent(GrabbedObject.gameObject));

                if (GrabbedObject == null)
                    newTarget.OnHoverEnter.Invoke(new OnHoverEnterEvent(hit.point));
                else
                    newTarget.OnHoverEnter.Invoke(new OnHoverEnterEvent(hit.point, GrabbedObject.gameObject));
            }
            // From target to same target (no change)
            else if (_oldTarget != null && _oldTarget == newTarget) {
                _oldTarget.OnHover.Invoke(new OnHoverEvent(hit.point));
            }
            // From a target to no target
            else if (_oldTarget != null && newTarget == null) {
                if (GrabbedObject == null)
                    _oldTarget.OnHoverExit.Invoke(new OnHoverExitEvent());
                else
                    _oldTarget.OnHoverExit.Invoke(new OnHoverExitEvent(GrabbedObject.gameObject));
            }

            _oldTarget = newTarget;
        }
        else
        {
            // From a target to none
            _oldTarget.OnHoverExit.Invoke(new OnHoverExitEvent());
            _oldTarget = null;
        }
    }

}
