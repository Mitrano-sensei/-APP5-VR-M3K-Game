using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnemyHealth), typeof(FaceTarget), typeof(MechRocketLauncher))]
public class SimpleEnemy : MonoBehaviour
{
    private MechRocketLauncher _rocketLauncher;
    private EnemyState _state = EnemyState.Wandering;
    private EnemyHealth _health;
    private FaceTarget _faceTarget;

    private GameManager _gameManager;
    private LogManager _logger;

    [Header("Events")]
    public UnityEvent<EnemyState> OnStateChanged = new UnityEvent<EnemyState>();

    [Header("Wandering Stats")]

    [Tooltip("The speed at which the enemy will rotate towards their target")]
    [SerializeField] private float _wanderRotationSpeed = 3f;
    [Tooltip("The angle at which the enemy will start moving towards their target")]
    [SerializeField] private float _wanderAngleToMove = 5f;
    [Tooltip("The speed at which the enemy will move towards their target")]
    [SerializeField] private float _wanderMoveSpeed = 1f;
    [Tooltip("The distance at which the enemy will consider it has reached their target")]
    [SerializeField] private float _wanderDistanceToTarget = .7f;
    [Tooltip("The distance at which the enemy will start to aggro the player and start locking")]
    [SerializeField] private float _wanderAggroRange = 10f;

    [Header("Wandering Paths")]
    [SerializeField] private Transform[] _path;

    [Header("Locking Stats")]
    [Tooltip("The speed at which the enemy will rotate towards the player")]
    [SerializeField] private float _lockRotationSpeed = 3f;
    [Tooltip("The angle at which the enemy will start charging")]
    [SerializeField] private float _lockAngle = 5f;

    [Header("Charge Stats")]
    [SerializeField] private float _requiredChargeTime = 1f;

    [Header("Attack Stats")]
    [Tooltip("The Time the enemy will wait after attacking")]
    [SerializeField] private float _cooldownTime = 1f;
    [Space(10)]
    [SerializeField] private float _rocketSpeed = 10f;
    [SerializeField] private float _rocketLifeTime = 5f;
    [Tooltip("Damage dealt by the rocket to enemies, note that it does not apply to player mech")]
    [SerializeField] private int _rocketDamage = 10;
    [SerializeField] private float _rocketExplosionRadius = 5f;
    [SerializeField] private float _rocketExplosionForce = 10f;

    private Color _baseColor;

    public void Start()
    {
        _rocketLauncher = GetComponent<MechRocketLauncher>();
        _health = GetComponent<EnemyHealth>();
        _faceTarget = GetComponent<FaceTarget>();

        _gameManager = GameManager.Instance;
        _logger = LogManager.Instance;

        _baseColor = GetComponent<Renderer>().material.color;

        _health.OnDeath.AddListener(deathEvent =>
        {
            ChangeState(EnemyState.Dead);
        });
    }

    public void Update()
    {
        switch (_state)
        {
            case EnemyState.Wandering:
                Wander();
                if (WatchForPlayer()) ChangeState(EnemyState.Locking);
                break;
            case EnemyState.Locking:
                Lock();
                break;
            case EnemyState.Charging:
                Charge();
                break;
            case EnemyState.Attacking:
                Fire();
                ChangeState(EnemyState.Cooldown);
                break;
            case EnemyState.Cooldown:
                Cooldown();
                break;
            case EnemyState.Dead:
                break;
            default:
                Debug.LogError("Unknown State :/");
                break;
        }
    }


    #region Behaviors

    private int _currentPathIndex = 0;
    /**
     * Wanders in his path (in circle).
     * The enemy will rotate towards the next point in the path, and then move towards it.
     * The movement is really simple, won't take into account obstacles.
     */
    private void Wander()
    {
        _faceTarget.enabled = true;

        if (_path == null || _path.Length == 0)
            return;

        // Verify if we're close enough to the next point in the path
        if (Vector3.Distance(transform.position, _path[_currentPathIndex].position.With(y:transform.position.y)) < _wanderDistanceToTarget)
        {
            // _logger.Trace("Wander - Reached !");
            // If we are, increment the index
            _currentPathIndex++;
            // If we've reached the end of the path, loop back to the beginning
            if (_currentPathIndex >= _path.Length)
                _currentPathIndex = 0;
        }

        var nextPosition = _path[_currentPathIndex].position.With(y:transform.position.y);

        // Verify if the enemy is facing the target
        var direction = nextPosition - transform.position;
        direction.y = 0;
        var angle = Vector3.Angle(transform.forward, direction);
        
        if (angle < _wanderAngleToMove)
        {
            // _logger.Trace("Wander - Walking");
            // Walks towards the target
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, _wanderMoveSpeed * Time.deltaTime);
        }
        else
        {
            // Set the target to the next point in the path
            _faceTarget.RotationSpeed = _wanderRotationSpeed;
            _faceTarget.Target = nextPosition;
            // _logger.Trace("Wander - Rotating");
        }
    }

    private Vector3? _lockTargetPosition;
    /**
     * Rotates to the player until it's facing him, then starts charging.
     */
    private void Lock()
    {
        _faceTarget.enabled = true;

        if (_lockTargetPosition == null)
            _lockTargetPosition = _gameManager.Player.transform.position;

        var lockTargetPosition = _lockTargetPosition ?? _gameManager.Player.transform.position; // So Vector3 is not nullable

        if (Vector3.Angle(transform.forward, lockTargetPosition - transform.position) < _lockAngle)
        {
            // If the enemy is facing the player, start charging
            ChangeState(EnemyState.Charging);

            // Prepare next lock
            _lockTargetPosition = null;
        }
        else
        {
            _logger.Trace("Lock - Turning to player");
            // Rotates to the player until it's facing him
            _faceTarget.RotationSpeed = _lockRotationSpeed;
            _faceTarget.Target = lockTargetPosition;
        }
    }

    public bool WatchForPlayer()
    {
        if (_state == EnemyState.Wandering)
        {
              // Verify if the player is within the aggro range
            var player = _gameManager.Player;
            if (player == null)
            {
                Debug.LogError("Player is null");
                return false;
            }

            var distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < _wanderAggroRange)
            {
                ChangeState(EnemyState.Locking);
                return true;
            }
        }

        return false;
    }

    private float _currentChargeTime = 0f;
    /**
     * Charges for a certain amount of time, then fires.
     * TODO : Should add an animation
     */
    private void Charge()
    {
        _faceTarget.enabled = false;
        // Tweeks colors from red to white to red until charging is complete
        var color = Color.Lerp(_baseColor, Color.white, _currentChargeTime / _requiredChargeTime);
        var renderer = GetComponent<Renderer>();
        
        if (_currentChargeTime >= _requiredChargeTime)
        {
            color = _baseColor;
            ChangeState(EnemyState.Attacking);
            _currentChargeTime = 0f;
        }
        else
        {
            _currentChargeTime += Time.deltaTime;
        }

        renderer.material.color = color;
    }
    
    public void Fire()
    {
        // Sets Rocket Launcher's stats
        _rocketLauncher.RocketDamage = _rocketDamage;
        _rocketLauncher.RocketSpeed = _rocketSpeed;
        _rocketLauncher.RocketLifeTime = _rocketLifeTime;
        _rocketLauncher.RocketExplosionRadius = _rocketExplosionRadius;
        _rocketLauncher.RocketExplosionForce = _rocketExplosionForce;

        _rocketLauncher.Fire();
    }

    private float _currentCooldownTime = 0f;

    private void Cooldown()
    {
        if (_cooldownTime - _currentCooldownTime <= 0)
        {
            _currentCooldownTime = 0f;
            ChangeState(EnemyState.Wandering);
            return;
        }

        _currentCooldownTime += Time.deltaTime;
    }

    #endregion

    public void OnDrawGizmosSelected()
    {
        if (_state == EnemyState.Wandering)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _wanderAggroRange);
        }
    }

    #region State
    public void ChangeState(EnemyState newState)
    {
        _state = newState;
        OnStateChanged.Invoke(_state);
    }

    public enum EnemyState
    {
        Wandering,
        Locking,
        Charging,
        Attacking,
        Cooldown,
        Dead
    }
    #endregion
}


#region Events
[Serializable] public class OnAttack : UnityEvent<OnAttackEvent> { }
public class OnAttackEvent
{

}

[Serializable] public class OnCharge : UnityEvent<OnChargeEvent> { }
public class OnChargeEvent
{

}

#endregion