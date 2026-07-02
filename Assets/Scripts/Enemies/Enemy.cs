using UnityEngine;


/// Поведение врага на сцене.
/// Получает свои характеристики через Initialize() из EnemyData (ScriptableObject),
/// а не хардкодом. При смерти НЕ удаляется через Destroy() —
/// возвращается в пул через PoolManager.Release(), чтобы быть использованным снова.

public class Enemy : MonoBehaviour
{
    [Header("Префаб орба опыта")]
    [SerializeField] private GameObject expOrbPrefab;

    private EnemyData _data;       // ссылка на ScriptableObject с характеристиками
    private Transform _target;     // цель движения (Transform игрока)
    private int _currentHealth;    // текущее HP (изменяется в ходе боя)

    // Событие: подписчики (например, спавнер или менеджер опыта)
    // будут уведомлены, когда этот враг умрёт.
    public System.Action<Enemy> OnDied;


    /// Вызывается спавнером сразу после pool.Get().
    /// Инициализирует врага свежими данными — важно сбрасывать HP при
    /// каждом "возрождении" из пула, иначе враг будет помнить урон из прошлой жизни.

    public void Initialize(EnemyData data, Transform target)
    {
        _data = data;
        _target = target;
        _currentHealth = data.maxHealth; // сбрасываем HP при каждом спавне из пула!
    }

    private void Update()
    {
        if (_target == null || _data == null) return;

        // Каждый кадр двигаемся в сторону игрока.
        // normalized — нормализует вектор, чтобы скорость была одинаковой
        // независимо от расстояния до цели.
        Vector3 direction = (_target.position - transform.position).normalized;
        transform.position += direction * _data.moveSpeed * Time.deltaTime;
    }


    /// Принимает урон от автоатаки игрока.
    /// Вызывается из PlayerAttack.cs.

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {

        // Уведомляем подписчиков (например, спавнер опыта на день 2).
        OnDied?.Invoke(this);

        // Уведомляем GameManager — он обновит счётчик убийств в HUD.
        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyKilled();

        // Спавним орб опыта в точке смерти врага.
        if (expOrbPrefab != null)
        {
            GameObject orb = PoolManager.Instance.Get(expOrbPrefab, transform.position, Quaternion.identity);
            if (orb.TryGetComponent(out ExpOrb expOrb))
                expOrb.Initialize(_data.experienceReward);
        }
        // НЕ Destroy, а возврат в пул!
        // Объект просто станет SetActive(false) и будет ждать следующего спавна.
        PoolManager.Instance.Release(gameObject);
    }


    /// Когда враг касается коллайдера игрока — наносим урон.
    /// Работает потому что на обоих стоит Is Trigger = true.

    private void OnTriggerEnter2D(Collider2D other)
    {
        // TryGetComponent — безопасный способ проверить наличие компонента
        // без лишнего GetComponent (не создаёт мусор в памяти).
        if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage(_data.damage);
        }
    }
}
