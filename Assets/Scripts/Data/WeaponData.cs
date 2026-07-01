using UnityEngine;


/// Данные оружия игрока в виде ScriptableObject-ассета.
/// Создаётся через ПКМ в Project → Create → AutoBattler → Weapon Data.
/// Каждый ассет = одно оружие. На день 2 добавим выбор оружия при левелапе.

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "AutoBattler/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Идентификация")]
    public string weaponName = "Оружие";

    [Header("Боевые параметры")]
    public int damage = 1;               // урон за удар
    public float attackRadius = 5f;      // радиус автоатаки (будет виден как Gizmo в сцене)
    public float attacksPerSecond = 1f;  // сколько ударов в секунду

    [Header("Снаряд (пригодится на день 2)")]
    public GameObject projectilePrefab;  // префаб снаряда (пока оставь пустым)
    public float projectileSpeed = 10f;  // скорость снаряда
}
