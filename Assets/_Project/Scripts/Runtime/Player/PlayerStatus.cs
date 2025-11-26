// Purpose: Handles player health using balance config (SRP: player vitals)
using UnityEngine;

public sealed class PlayerStatus : MonoBehaviour
{
    [SerializeField] private GameBalanceConfig balanceConfig;

    public event System.Action<float, float> OnHealthChanged; // current, max
    public event System.Action OnDeath;
    public event System.Action OnRespawn;

    private float _currentHealth;
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
        ReportVitals();
        Debug.Log("[PlayerStatus] Initialized");
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
        ReportVitals();
        Debug.Log("[PlayerStatus] Player respawned");
        OnRespawn?.Invoke();
    }

    private void ReportVitals()
    {
        OnHealthChanged?.Invoke(_currentHealth, balanceConfig.playerMaxHealth);
    }
}
