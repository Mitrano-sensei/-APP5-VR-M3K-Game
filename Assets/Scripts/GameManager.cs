using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [Header("Player")]
    [SerializeField] private int _maxHealth = 20;
    private int currentHealth;

    [SerializeField] private Transform _player;
    public Transform Player { get => _player; }

    [Header("Events")]
    [SerializeField] private OnHealthChange _onHealthChange = new OnHealthChange();
    [SerializeField] private OnHullDamageTaken _onHullDamageTaken = new OnHullDamageTaken();

    protected LogManager _logger;

    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public int MaxHealth { get => _maxHealth; }
    public OnHealthChange OnHealthChange { get => _onHealthChange; set => _onHealthChange = value; }
    public OnHullDamageTaken OnHullDamageTaken { get => _onHullDamageTaken; set => _onHullDamageTaken = value; }


    // Start is called before the first frame update
    void Start()
    {
        _logger = LogManager.Instance;
        CurrentHealth = MaxHealth;

        OnHealthChange.AddListener(OnHealthChangeHandler);
    }

    private void OnHealthChangeHandler(OnHealthChangeEvent onHealthChangeEvent)
    {
        _logger.Trace("Health remaining: " + CurrentHealth);
        if (CurrentHealth <= 0)
        {
            _logger.Trace("You are DED. GAME OVER.");
        }
    }

    public void GainHealth(int amount=1)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        OnHealthChange?.Invoke(new OnHealthChangeEvent(amount));
        if (amount <= 0) OnHullDamageTaken.Invoke(amount);
    }
}

#region Events

[Serializable] public class OnHealthChange : UnityEvent<OnHealthChangeEvent> { }

public class OnHealthChangeEvent
{
    private int _amount;

    public OnHealthChangeEvent(int amount)
    {
        Amount = amount;
    }

    public int Amount { get => _amount; set => _amount = value; }
}

[Serializable] public class OnHullDamageTaken : UnityEvent<int> { }
#endregion

