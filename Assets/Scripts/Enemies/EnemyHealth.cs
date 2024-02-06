using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Damageable))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;

    [Header("Events")]
    [SerializeField] private bool _destroyOnDeath = true;
    [SerializeField] private OnDeath _onDeath = new OnDeath();

    [Header("Loot")]
    [Tooltip("Parent to spawn loot (usables) under")]
    [SerializeField] private Transform _usableParent;
    [SerializeField] private List<GameObject> _loot;
    public OnDeath OnDeath { get { return _onDeath; } }

    private Damageable _damageable;
    private LogManager _logger;

    public void Start()
    {
        _damageable = GetComponent<Damageable>();
        _currentHealth = _maxHealth;
        _logger = LogManager.Instance;

        OnDeath.AddListener(deathEvent =>
        {
            _logger.Trace(gameObject.name + " has died.");
        });

        _damageable.OnDamageTaken.AddListener(damageEvent => {
            _currentHealth -= damageEvent.Damage;
            if ( _currentHealth <= 0)
            {
                Die();
            }
        });
    }

    public void Die()
    {
        OnDeath.Invoke(new DeathEvent());

        if (_loot.Count > 0)
        {
            foreach (var item in _loot)
            {
                Instantiate(item, _usableParent.position, Quaternion.identity, _usableParent);
            }
        }

        if (_destroyOnDeath) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}

#region Events

[Serializable] public class OnDeath : UnityEvent<DeathEvent> { }

public class DeathEvent
{
    public DeathEvent()
    {
    }
}

#endregion
