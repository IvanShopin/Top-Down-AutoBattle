using UnityEngine;

/// <summary>
/// Орб опыта. Спавнится из пула при смерти врага, подбирается при касании игрока.
/// Как и враги — НЕ Destroy, а PoolManager.Release() при подборе.
/// </summary>
public class ExpOrb : MonoBehaviour
{
    // Количество опыта внутри этого орба — задаётся спавнером при инициализации.
    private int _expAmount;

    /// <summary>
    /// Инициализация после Get() из пула. Сбрасывает количество опыта.
    /// </summary>
    public void Initialize(int expAmount)
    {
        _expAmount = expAmount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Если коснулся игрок — отдаём опыт и возвращаемся в пул.
        if (other.CompareTag("Player"))
        {
            // ExperienceManager.Instance — синглтон, который мы создадим на следующем шаге.
            ExperienceManager.Instance.AddExperience(_expAmount);

            // Возврат в пул вместо Destroy — так же как с врагами.
            PoolManager.Instance.Release(gameObject);
        }
    }
}
