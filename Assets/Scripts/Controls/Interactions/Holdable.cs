using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Dockable))]
public class Holdable : MonoBehaviour
{
    [Tooltip("Time in seconds before the hold is considered as a stop hold")]
    [SerializeField][Range(.05f, 1f)] private float _stopHoldTimeThreshHold = .2f;

    [SerializeField] private OnHold _onHold = new OnHold();
    [SerializeField] private OnStopHold _onStopHold = new OnStopHold();

    public OnHold OnHold { get => _onHold; private set => _onHold = value; }
    public OnStopHold OnStopHold { get => _onStopHold; private set => _onStopHold = value; }
    private State _state = State.IDLE;
    private float _lastHold = -1f;
    private float _startHold = -1f;

    private void Update()
    {
        switch (_state)
        {
            case State.IDLE:
                _lastHold = -1f;
                break;
            case State.HOLDING:
                LogManager.Instance.Trace("HOLDING");
                if (Time.time - _lastHold > _stopHoldTimeThreshHold)
                    _state = State.STOP_HOLDING;

                if (CheckConditions())
                    _state = State.HOLDING;

                OnHold.Invoke(new OnHoldEvent() { StartHoldTime = _startHold });
                break;
            case State.STOP_HOLDING:
                LogManager.Instance.Trace("STOP HOLDING");
                _onStopHold.Invoke(new OnHoldEvent() { StartHoldTime = _startHold });

                _startHold = -1f;
                _state = State.IDLE;
                break;
        }
    }

    /**
     * Manage the holding of the object
     */
    public void Hold()
    {
        _lastHold = Time.time;
        if (_state == State.IDLE) _startHold = Time.time;

        _state = State.HOLDING;
    }


    private enum State
    {
        IDLE,
        HOLDING,
        STOP_HOLDING,

    }

    #region Condition
    public delegate bool Condition();
    private List<Condition> _conditions = new List<Condition>();

    /**
    * Adds a condition to the interaction.
    * If the condition is not met, the interaction will fail.
    * 
    * TODO : Add more information, like the name of the condition, or the object that failed the condition.
    */
    public void AddCondition(Condition condition)
    {
        _conditions.Add(condition);
    }

    /**
     *  Check if all conditions are met before interacting.
     *  If no conditions are set, the interaction is always possible.
     *  If at least one condition is not met, returns true.
     *  To add a condition use AddCondition(() => { return myCondition; });
     */
    private bool CheckConditions()
    {
        var checkConditionsList = _conditions.TrueForAll(c => c());

        return !checkConditionsList;
    }
    #endregion
}

#region Events
[Serializable] public class OnHold : UnityEvent<OnHoldEvent> {}
[Serializable] public class OnStopHold : UnityEvent<OnHoldEvent> {}

public class OnHoldEvent{
    public float StartHoldTime = -1f;
    public float HoldTime { get { return Time.time - StartHoldTime; } }
}

#endregion