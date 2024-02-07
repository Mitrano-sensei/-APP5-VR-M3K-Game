using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [Header("Player")]
    [SerializeField] private int _maxHealth = 20;
    private int currentHealth;

    [SerializeField] private Transform _playerMech;
    public Transform PlayerMech { get => _playerMech; }

    [Header("Events")]
    [SerializeField] private OnHealthChange _onHealthChange = new OnHealthChange();
    [SerializeField] private OnHealthChange _onDamageTaken = new OnHealthChange();
    private OnHealthChangeDone _onHealthChangeDone = new OnHealthChangeDone();
    public OnHealthChange OnDamageTaken { get => _onDamageTaken; set => _onDamageTaken = value; }

    [Header("Misc")]
    [SerializeField] private Transform _buttonParent;

    protected LogManager _logger;
    [SerializeField] private float _intensity = 1.4f;

    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public int MaxHealth { get => _maxHealth; }
    public OnHealthChange OnHealthChange { get => _onHealthChange; set => _onHealthChange = value; }
    public OnHealthChangeDone OnHealthChangeDone { get => _onHealthChangeDone; set => _onHealthChangeDone = value; }

    // Start is called before the first frame update
    void Start()
    {
        _logger = LogManager.Instance;
        CurrentHealth = MaxHealth;

        OnHealthChange.AddListener(OnHealthChangeHandler);
        OnHealthChange.AddListener(e => StartMenuScene());
    }

    private void OnHealthChangeHandler(OnHealthChangeEvent onHealthChangeEvent)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + onHealthChangeEvent.Amount, 0, MaxHealth);
        _logger.Trace("Health remaining: " + CurrentHealth);
        if (CurrentHealth <= 0)
        {
            _logger.Trace("You are DED. GAME OVER.");
        }

        if (onHealthChangeEvent.Amount < 0)
        {
            OnDamageTaken?.Invoke(onHealthChangeEvent);
        }
    }

    public void GainHealth(int amount=1)
    {
        OnHealthChange?.Invoke(new OnHealthChangeEvent(amount));
        OnHealthChangeDone?.Invoke(new OnHealthChangeDoneEvent());
    }

    public void ShakeAllButtons()
    {
        var dock = DockManager.Instance.GetRandomUsedDocker();

        while (dock != null)
        {
            dock.Eject();
            dock = DockManager.Instance.GetRandomUsedDocker();
        }

        foreach (Transform button in _buttonParent)
        {
            button.GetComponent<Rigidbody>().AddForce(UnityEngine.Random.insideUnitSphere * 10 * _intensity, ForceMode.Impulse);
        }
    }

    public void ExitApp()
    {
        _logger.Log("Exiting application");
        Application.Quit();
    }

    public void StartMainScene()
    {
        _logger.Log("Starting main scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    public void StartMenuScene()
    {
        _logger.Log("Starting menu scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu Scene");
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

[Serializable] public class OnHealthChangeDone : UnityEvent<OnHealthChangeDoneEvent> { }

public class OnHealthChangeDoneEvent
{

}
#endregion

