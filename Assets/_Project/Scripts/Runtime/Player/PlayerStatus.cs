// Purpose: Handles player health and hunger using balance config (SRP: player vitals)
using UnityEngine;

public sealed class PlayerStatus : MonoBehaviour
{
    [SerializeField] private GameBalanceConfig balanceConfig;

    public event System.Action<float, float> OnHealthChanged; // current, max
    public event System.Action<float, float> OnHungerChanged; // current, max
    public event System.Action OnDeath;
    public event System.Action OnRespawn;

    private float _currentHealth;
    private float _currentHunger;
    private bool _isDead;

    private void Awake()
    {
        if (balanceConfig == null)
        {
            Debug.LogWarning("[PlayerStatus] Missing GameBalanceConfig");
            enabled = false;
            return;
        }

        _currentHealth = balanceConfig.playerStartHealth;
        _currentHunger = balanceConfig.playerMaxHunger;
        ReportVitals();
        Debug.Log("[PlayerStatus] Initialized");
    }

    private void Update()
    {
        if (_isDead) return;
        TickHunger(Time.deltaTime);
    }

    public void ApplyDamage(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        Debug.Log($"[PlayerStatus] Damage {amount}, health {_currentHealth}/{balanceConfig.playerMaxHealth}");
        OnHealthChanged?.Invoke(_currentHealth, balanceConfig.playerMaxHealth);
        if (_currentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(balanceConfig.playerMaxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, balanceConfig.playerMaxHealth);
    }

    private void TickHunger(float deltaTime)
    {
        _currentHunger = Mathf.Max(0f, _currentHunger - balanceConfig.hungerDecayPerSecond * deltaTime);
        OnHungerChanged?.Invoke(_currentHunger, balanceConfig.playerMaxHunger);

        if (_currentHunger <= 0f)
        {
            ApplyDamage(balanceConfig.hungerDamagePerSecond * deltaTime);
        }
    }

    private void HandleDeath()
    {
        _isDead = true;
        Debug.Log("[PlayerStatus] Player died");
        OnDeath?.Invoke();
        Respawn();
    }

    private void Respawn()
    {
        _isDead = false;
        _currentHealth = balanceConfig.playerMaxHealth;
        _currentHunger = balanceConfig.playerMaxHunger * balanceConfig.hungerRevivePercent;
        ReportVitals();
        Debug.Log("[PlayerStatus] Player respawned");
        OnRespawn?.Invoke();
    }

    private void ReportVitals()
    {
        OnHealthChanged?.Invoke(_currentHealth, balanceConfig.playerMaxHealth);
        OnHungerChanged?.Invoke(_currentHunger, balanceConfig.playerMaxHunger);
    }
}
