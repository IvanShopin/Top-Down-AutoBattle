using System.Collections.Generic;
using UnityEngine;


/// Спавнер врагов. Через равные промежутки времени запрашивает
/// объект из пула (PoolManager.Get) и инициализирует его данными
/// из случайно выбранного EnemyData-ассета.
/// НИКАКИХ Instantiate — только Get из пула.

public class EnemySpawner : MonoBehaviour
{
    [Header("Типы врагов (ScriptableObject-ассеты)")]

    [SerializeField] private List<EnemyData> enemyTypes;

    [Header("Ссылки")]
    [SerializeField] private Transform player; // цель, к которой будут идти враги

    [Header("Параметры спавна")]
    [SerializeField] private float spawnInterval = 1.5f; // секунд между спавнами
    [SerializeField] private float spawnRadius = 10f;    // радиус от игрока, на котором спавним

    private float _timer; // внутренний таймер между спавнами

    private void Update()
    {
        // Накапливаем время. Когда накопилось достаточно — спавним врага и сбрасываем.
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        // Защита: если забыли назначить данные или игрока в Inspector — ничего не делаем.
        if (enemyTypes == null || enemyTypes.Count == 0 || player == null) return;

        // Выбираем случайный тип врага из списка.
        EnemyData data = enemyTypes[Random.Range(0, enemyTypes.Count)];

        // Выбираем случайную точку на окружности вокруг игрока.
        // insideUnitCircle.normalized — точка строго на краю круга (не внутри).
        Vector2 spawnOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = player.position + (Vector3)spawnOffset;

        // Ключевое место: НЕ Instantiate, а Get из пула.
        // Пул либо вернёт "спящий" объект, либо создаст новый — нам без разницы.
        GameObject instance = PoolManager.Instance.Get(data.prefab, spawnPosition, Quaternion.identity);

        // Инициализируем врага: передаём ему данные и цель.
        // Это обязательно делать после Get, чтобы сбросить HP и цель.
        if (instance.TryGetComponent(out Enemy enemy))
        {
            enemy.Initialize(data, player);
        }
    }
}
