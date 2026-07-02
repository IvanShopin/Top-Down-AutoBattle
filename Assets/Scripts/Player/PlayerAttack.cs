using UnityEngine;


/// Автоатака игрока. Через равные промежутки времени ищет ближайшего
/// врага в радиусе и наносит ему урон. Все параметры (урон, радиус,
/// скорострельность) берутся из WeaponData (ScriptableObject), не из кода.

public class PlayerAttack : MonoBehaviour
{
    [Header("Данные оружия (ScriptableObject-ассет)")]
  
    [SerializeField] private WeaponData weapon;

    [Header("Слой врагов")]

    [SerializeField] private LayerMask enemyLayer;

    private float _timer; // счётчик времени между атаками

    private void Update()
    {
        if (weapon == null) return;

        _timer += Time.deltaTime;

        // attacksPerSecond = 2 означает атаку каждые 0.5 секунды (1 / 2 = 0.5).
        float attackInterval = 1f / weapon.attacksPerSecond;

        if (_timer >= attackInterval)
        {
            _timer = 0f;
            TryAttack();
        }
    }

    private void TryAttack()
    {
        // OverlapCircleAll — возвращает массив всех коллайдеров в радиусе на слое Enemy.
        // Это физический запрос, поэтому делаем его только раз в attackInterval,
        // а не каждый кадр в Update — это важно для производительности.
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            weapon.attackRadius,
            enemyLayer
        );

        // Если врагов в радиусе нет — выходим, атаковать некого.
        if (hits.Length == 0) return;

        // Находим ближайшего врага вручную — LINQ (OrderBy) создаёт мусор в памяти,
        // поэтому используем простой цикл.
        Collider2D nearest = hits[0];
        float nearestDist = Vector2.Distance(transform.position, nearest.transform.position);

        for (int i = 1; i < hits.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, hits[i].transform.position);
            if (dist < nearestDist)
            {
                nearest = hits[i];
                nearestDist = dist;
            }
        }

        // Бьём ближайшего врага. TryGetComponent — безопасно и без мусора в памяти.
        if (nearest.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(weapon.damage);
        }
    }
    /// Применяет бонусы перка к оружию. Вызывается из ExperienceManager.ApplyPerk().
    public void ApplyPerkBonus(int bonusDamage, float bonusAttackSpeed, float bonusRadius)
    {
        weapon.damage += bonusDamage;
        weapon.attacksPerSecond += bonusAttackSpeed;
        weapon.attackRadius += bonusRadius;
    }
    // OnDrawGizmosSelected — рисует вспомогательный круг радиуса атаки
    // прямо в окне Scene, когда объект Player выделен. Виден только в редакторе,
    // в игре не отображается. Удобно для настройки баланса.
    private void OnDrawGizmosSelected()
    {
        if (weapon == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, weapon.attackRadius);
    }
}
