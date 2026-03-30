using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private int _currentHealth;

    [Header("Damage")]
    [SerializeField] private bool _canTakeDamage = true;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;

    private void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(int damage)
    {
        if (!_canTakeDamage || IsDead)
            return;

        if (damage <= 0)
            return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        NotifyHealthChanged();

        if (_currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead)
            return;

        if (amount <= 0)
            return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

        NotifyHealthChanged();
    }

    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
        NotifyHealthChanged();
    }

    public void SetCanTakeDamage(bool canTakeDamage)
    {
        _canTakeDamage = canTakeDamage;
    }

    private void HandleDeath()
    {
        GameManager.Instance?.TriggerGameOver();
    }

    private void NotifyHealthChanged()
    {
        GameEvents.OnPlayerHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}