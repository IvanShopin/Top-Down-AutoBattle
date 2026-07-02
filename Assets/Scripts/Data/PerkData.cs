using UnityEngine;


/// Данные одного перка в виде ScriptableObject-ассета.
/// Создаётся через ПКМ → Create → AutoBattler → Perk Data.
/// Каждый ассет = один перк. Добавить новый перк = создать новый ассет.

[CreateAssetMenu(fileName = "NewPerk", menuName = "AutoBattler/Perk Data")]
public class PerkData : ScriptableObject
{
    [Header("Отображение")]
    public string perkName = "Перк";             // название на экране выбора
    [TextArea] public string description = "";    // описание для игрока
    public Sprite icon;                           // иконка 

    [Header("Бонусы к оружию")]
    public int bonusDamage = 0;                  // +урон к WeaponData.damage
    public float bonusAttackSpeed = 0f;          // +скорость атаки к WeaponData.attacksPerSecond
    public float bonusRadius = 0f;               // +радиус к WeaponData.attackRadius

    [Header("Бонусы к игроку")]
    public int bonusHealth = 0;                  // +максимальное HP игрока
    public float bonusSpeed = 0f;                // +скорость движения игрока
}
