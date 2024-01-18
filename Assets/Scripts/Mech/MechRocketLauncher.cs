using UnityEngine;
using UnityEngine.Events;

public class MechRocketLauncher : MonoBehaviour
{
    [Header("Rocket launcher params")]
    [SerializeField] private GameObject _rocketPrefab;
    [SerializeField] private Transform _rocketSpawnPoint;
    [SerializeField] private bool _isAlly = true;

    [Header("Rocket Stats")]
    [SerializeField] private float _rocketSpeed = 10f;
    [SerializeField] private float _rocketLifeTime = 5f;
    [Tooltip("Damage dealt by the rocket to enemies, note that it does not apply to player mech")]
    [SerializeField] private int _rocketDamage = 10;
    [SerializeField] private float _rocketExplosionRadius = 5f;
    [SerializeField] private float _rocketExplosionForce = 10f;

    [Header("Events")]
    [Tooltip("Event called when the rocket is fired")]
    [SerializeField] private UnityEvent _onFire = new UnityEvent();
    [Tooltip("Event called when the rocket explodes")]
    [SerializeField] private UnityEvent _onExplode = new UnityEvent();

    private LogManager _logger;

    public float RocketSpeed { get => _rocketSpeed; set => _rocketSpeed = value; }
    public float RocketLifeTime { get => _rocketLifeTime; set => _rocketLifeTime = value; }
    public int RocketDamage { get => _rocketDamage; set => _rocketDamage = value; }
    public float RocketExplosionRadius { get => _rocketExplosionRadius; set => _rocketExplosionRadius = value; }
    public float RocketExplosionForce { get => _rocketExplosionForce; set => _rocketExplosionForce = value; }

    private void Start()
    {
        
        _logger = LogManager.Instance;
        _onFire.AddListener(() =>  _logger.Trace("Firing rocket") );
    }

    public void Fire()
    {
        _onFire?.Invoke();

        var rocket = Instantiate(_rocketPrefab, _rocketSpawnPoint.position, _rocketSpawnPoint.rotation);
        var rocketScript = rocket.GetComponent<RocketScript>();
        rocketScript.Speed = RocketSpeed;
        rocketScript.LifeTime = RocketLifeTime;
        rocketScript.Damage = RocketDamage;
        rocketScript.ExplosionRadius = RocketExplosionRadius;
        rocketScript.ExplosionForce= RocketExplosionForce;
        rocketScript.OnExplode = _onExplode;
        rocketScript.IsAlly = _isAlly;
        rocketScript.FiredBy = gameObject;
    }   

}
