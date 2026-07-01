using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


/// Singleton-менеджер пулов объектов.
/// Оборачивает встроенный в Unity UnityEngine.Pool.ObjectPool<GameObject>.
/// Для каждого уникального префаба создаётся свой отдельный пул.
/// Используй Get/Release ВМЕСТО Instantiate/Destroy везде в игровом цикле
/// (враги, снаряды, орбы опыта, частицы) — это и есть весь смысл паттерна.

public class PoolManager : MonoBehaviour
{
    // Статическая ссылка на единственный экземпляр (паттерн Singleton).
    // Благодаря этому любой скрипт может вызвать PoolManager.Instance.Get(...)
    // без необходимости искать объект на сцене.
    public static PoolManager Instance { get; private set; }

    [SerializeField] private int defaultCapacity = 20; // сколько объектов держим "наготове" без лишних Instantiate
    [SerializeField] private int maxSize = 200;         // верхний предел объектов в одном пуле (защита от утечки памяти)

    // префаб -> пул для этого префаба.
    // У каждого типа врага/снаряда свой собственный пул, чтобы не перепутать типы.
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();

    // конкретный экземпляр на сцене -> префаб, из которого он создан.
    // Нужен, чтобы при Release() понять, в какой именно пул вернуть объект.
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

    private void Awake()
    {
        // Защита от дублей: если на сцене случайно оказалось два PoolManager,
        // оставляем только первый, второй удаляем.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// Возвращает активный экземпляр указанного префаба, уже поставленный
    /// в нужную позицию и поворот. При первом обращении к этому префабу
    /// создаёт для него новый пул.

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Если пула для этого префаба ещё нет — создаём его один раз.
        if (!_pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool = CreatePool(prefab);
            _pools[prefab] = pool;
        }

        // pool.Get() сам решает: взять "спящий" объект из запаса
        // или создать новый, если все заняты.
        GameObject instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }


    /// Возвращает объект обратно в пул (деактивирует его) вместо Destroy().
    /// Вызывается, например, когда враг умирает.

    public void Release(GameObject instance)
    {
        // Ищем, из какого префаба был создан этот конкретный объект,
        // и находим соответствующий пул.
        if (_instanceToPrefab.TryGetValue(instance, out GameObject prefab) &&
            _pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool.Release(instance);
        }
        else
        {
            // Если объект не из пула (например, его создали вручную через
            // Instantiate, забыв про пул) — на всякий случай удаляем по-старому,
            // чтобы игра не зависла, но предупреждаем в консоли об ошибке логики.
            Debug.LogWarning($"PoolManager: объект '{instance.name}' не был создан через пул. Удаляю напрямую (Destroy).");
            Destroy(instance);
        }
    }

    // Создаёт новый ObjectPool для конкретного префаба.
    // ObjectPool принимает 4 функции-обработчика на каждое действие с объектом:
    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            // createFunc — вызывается только тогда, когда в пуле совсем нет
            // свободных объектов и нужен НОВЫЙ. Здесь, и только здесь,
            // происходит настоящий Instantiate.
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab, transform);
                _instanceToPrefab[obj] = prefab; // запоминаем, чей это объект
                return obj;
            },
            // actionOnGet — вызывается каждый раз при pool.Get(), независимо
            // от того, новый объект или взят из запаса. Включаем его обратно.
            actionOnGet: obj => obj.SetActive(true),
            // actionOnRelease — вызывается при pool.Release(). Прячем объект,
            // но НЕ удаляем — он останется в памяти, готовый к повторному использованию.
            actionOnRelease: obj => obj.SetActive(false),
            // actionOnDestroy — вызывается только если пул переполнен (превышен
            // maxSize) и старый объект пора окончательно удалить.
            actionOnDestroy: Destroy,
            collectionCheck: false, // отключаем проверку на повторный Release одного объекта (чуть быстрее)
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }
}
