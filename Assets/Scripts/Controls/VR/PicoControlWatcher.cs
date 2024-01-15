using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PicoControlWatcher : AbstractControlWatcher
{
    [Header("Player")]
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _hand;

    [Header("Grab")]
    [SerializeField] private float _grabOffset = 0.5f;

    [Header("Teleportation")]
    [SerializeField] private float _teleportationCooldown = 1f;
    private float _lastTeleportationTime = 0f;

    [Header("Interaction")]
    [SerializeField] private float _interactionCooldown = 0.5f;
    private float _lastInteractionTime = 0f;

    private List<InputDevice> _devices;

    protected override void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
        if (_player == null) Debug.LogError("Player is null");
        if (_hand == null) Debug.LogError("Hand is null");

        // Pico XR
        _devices = new List<InputDevice>();
        RegisterDevices();

        // Events
        OnTeleportEvent.AddListener((Vector3 newPos) =>
        {
            _player.transform.position = newPos;
        });

        Sequence mover = null;
        OnGrabEvent.AddListener((Pickable pickable) =>
        {
            var dockable = pickable.GetComponent<Dockable>();

            pickable.transform.SetParent(_hand.transform);
            mover = DOTween.Sequence();
            mover = mover.Append(pickable.transform.DOLocalMove(Vector3.forward * _grabOffset + (dockable != null ? dockable.CenterPosition * 0.15f : Vector3.zero), .5f).SetEase(Ease.InOutQuad));

            if (dockable != null)
            {
                var rotationOffset = new Vector3(-90, 0, 0); // Because Correct rotation is for the item to dock, not to be picked. 
                mover.Join(pickable.transform.DOLocalRotate(-dockable.CorrectRotation + rotationOffset, .5f).SetEase(Ease.InOutQuad));
            }

            Helpers.MoveUntilDie(pickable.transform, _hand, mover);
        });

        OnReleaseEvent.AddListener((ReleasedEvent releasedEvent) =>
        {
            mover?.Kill();
            mover = null;       // Double check x)

            if (releasedEvent.GetDocker() != null && releasedEvent.GetDocker().IsActive && releasedEvent.GetDocker().IsAvailable) return;     // Here we let base OnDock event handle the docking.

            var releasedObject = releasedEvent.ReleasedObject;
            var rb = releasedObject.GetComponent<Rigidbody>();
            releasedObject.transform.SetParent(releasedObject.OriginParent != null ? releasedObject.OriginParent : _cockpitEnvironment.transform);
        });
    }

    protected void Update()
    {
        bool triggerButtonState = false;
        bool menuButtonState = false;
        bool arrowButtonState = false;
        
        foreach (var device in _devices)
        {
            device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonState);
            device.TryGetFeatureValue(CommonUsages.menuButton, out menuButtonState);
            device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out arrowButtonState);
        }

        if (menuButtonState && Time.time - _lastTeleportationTime > _teleportationCooldown)
        {
            // Raycast from hand, if hit, teleport to hit point.
            RaycastHit hit;
            if (Helpers.HitBehindGrabbedObjectFromHand(_hand, GrabbedObject?.gameObject, out hit))
            {
                // If the object is a teleporter, invoke the event.
                if (hit.collider.gameObject.GetComponent<TeleportationArea>())
                {
                    OnTeleportEvent.Invoke(hit.point);
                    _lastTeleportationTime = Time.time;
                }
            }
        }

        if (arrowButtonState && Time.time - _lastInteractionTime > _interactionCooldown)
        {
            _logger.Trace("ArrowButton Pressed !");

            RaycastHit hit;
            if (Helpers.HitBehindGrabbedObjectFromHand(_hand, GrabbedObject?.gameObject, out hit))
            {
                var interactable = hit.collider.gameObject.GetComponent<Interactable>();
                if (interactable != null)
                {
                    _lastInteractionTime = Time.time;
                    // Here we touched an interactable
                    OnInteractEvent.Invoke(interactable);
                    return;
                }

                // Here we touched an item that is not an interactable.
                // Let's see if we are holding a usable item.
                var usableItem = GrabbedObject?.GetComponent<UsableItem>();
                if (usableItem != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    usableItem.OnUse.Invoke(new UseEvent(hitObject));
                    return;
                }
            }
        }

        if (triggerButtonState && GrabbedObject == null)
        {
            // Here we are not holding anything, and we are pressing the trigger button.
            // We should try to grab something.
            RaycastHit hit;
            if (Helpers.HitBehindGrabbedObjectFromHand(_hand, GrabbedObject?.gameObject, out hit))
            {
                // If the object is a pickable, invoke the event.
                if (hit.collider.gameObject.GetComponent<Pickable>())
                {
                    OnGrabEvent.Invoke(hit.collider.gameObject.GetComponent<Pickable>());
                }
            }
        }
        else if (!triggerButtonState && GrabbedObject != null)
        {
            // Here we are holding something, and we are not pressing the trigger button.
            // We should release the object.
            _logger.Trace("Releasing Button");

            // Raycast from main camera to next object.
            RaycastHit hit;
            Helpers.HitBehindGrabbedObjectFromHand(_hand, GrabbedObject?.gameObject, out hit);
            var docker = hit.collider?.gameObject?.GetComponent<Docker>();
            OnReleaseEvent.Invoke(docker == null ? new ReleasedEvent(GrabbedObject) : new ReleasedEvent(GrabbedObject, docker));
        }

    }


    private void DeviceConnected(InputDevice device)
    {
        bool discardedValue;

        if (device.TryGetFeatureValue(CommonUsages.menuButton, out discardedValue))
        {
            _devices.Add(device);
        }
    }

    private void DeviceDisconnected(InputDevice device)
    {
        if (_devices.Contains(device))
        {
            _devices.Remove(device);
        }
    }

    private void RegisterDevices()
    {
        var allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        foreach (var device in allDevices)
        {
            DeviceConnected(device);
        }

        InputDevices.deviceDisconnected += DeviceDisconnected;
        InputDevices.deviceConnected += DeviceConnected;
    }

}
