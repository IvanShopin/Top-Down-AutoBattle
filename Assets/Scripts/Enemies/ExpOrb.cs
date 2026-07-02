using UnityEngine;


/// Орб опыта. Автоматически летит к игроку
/// Это решает проблему орбов, которые остаются за экраном когда враги
/// умирают далеко (при большом радиусе атаки).
///
/// Логика притяжения двухфазная:
/// 1. Орб медленно дрейфует к игроку всегда (базовое притяжение).
/// 2. Когда игрок входит в радиус подбора — орб резко ускоряется к нему.
/// При касании — опыт засчитывается и орб уходит в пул.

public class ExpOrb : MonoBehaviour
{
    [Header("Движение")]
    // Скорость дрейфа орба к игроку пока он далеко.
    [SerializeField] private float driftSpeed = 1.5f;
    // Скорость орба когда игрок вошёл в радиус подбора — резкое ускорение.
    [SerializeField] private float attractSpeed = 12f;
    // Расстояние при котором орб начинает лететь к игроку быстро.
    [SerializeField] private float attractRadius = 4f;

    private int _expAmount;
    private Transform _playerTransform;


    /// Инициализируется при Get() из пула. Находим игрока по тегу.

    public void Initialize(int expAmount)
    {
        _expAmount = expAmount;

        // Ищем игрока по тегу "Player" — он единственный на сцене.
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _playerTransform = playerObj.transform;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        // Выбираем скорость в зависимости от дистанции до игрока.
        float speed = distToPlayer <= attractRadius ? attractSpeed : driftSpeed;

        // Двигаемся в сторону игрока с нужной скоростью.
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // При касании с игроком — засчитываем опыт и уходим в пул.
        if (other.CompareTag("Player"))
        {
            ExperienceManager.Instance.AddExperience(_expAmount);
            PoolManager.Instance.Release(gameObject);
        }
    }

    // Вспомогательный Gizmo: рисует радиус притяжения в окне Scene.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}
