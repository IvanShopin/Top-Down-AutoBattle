using UnityEngine;


/// Движение и здоровье игрока.
/// ApplyPerkBonus() вызывается из ExperienceManager при выборе перка.
/// События OnHealthChanged и OnDied используются HUD и GameManager.

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int maxHealth = 100;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;

    // HUD подписывается на это событие, чтобы обновлять HP-бар.
    public System.Action<int, int> OnHealthChanged; // (текущее HP, максимальное HP)
    // GameManager подписывается, чтобы показать Game Over.
    public System.Action OnDied;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = maxHealth;
    }

    private void Update()
    {
        // Читаем ввод каждый кадр. GetAxisRaw возвращает -1, 0 или 1 без сглаживания.
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
        _moveInput = _moveInput.normalized; // нормализация убирает ускорение по диагонали
    }

    private void FixedUpdate()
    {
        // Двигаем через физику, а не через transform.position —
        // так корректно работают столкновения с коллайдерами.
        _rb.MovePosition(_rb.position + _moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0) return;

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth == 0)
            OnDied?.Invoke();
    }


    /// Применяет бонусы перка к игроку. Вызывается из ExperienceManager.ApplyPerk().
  
    public void ApplyPerkBonus(int bonusHealth, float bonusSpeed)
    {
        moveSpeed += bonusSpeed;

        // Увеличиваем максимальное HP и сразу лечим на прибавленное количество.
        maxHealth += bonusHealth;
        _currentHealth = Mathf.Min(_currentHealth + bonusHealth, maxHealth);

        // Уведомляем HUD об изменении HP.
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }
}
