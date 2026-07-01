using UnityEngine;


/// Данные врага в виде ScriptableObject-ассета.
/// Создаётся через ПКМ в Project → Create → AutoBattler → Enemy Data.
/// Каждый ассет = один тип врага (слайм, скелет, гоблин и т.д.).
/// Никаких цифр в коде — все числа меняются прямо в Inspector.

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "AutoBattler/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Идентификация")]
    public string enemyName = "Враг";   // имя для отладки и UI
    public GameObject prefab;            // какой GameObject спавнить через пул

    [Header("Характеристики")]
    public int maxHealth = 10;           // очки здоровья
    public float moveSpeed = 2f;         // скорость движения к игроку
    public int damage = 1;               // урон при касании игрока

    [Header("Награда")]
    public int experienceReward = 5;     // сколько опыта падает при смерти
}
