using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Синглтон-менеджер системы опыта и уровней.
/// Следит за текущим опытом, вычисляет пороги уровней и
/// вызывает экран выбора перка при левелапе.
/// Подписывайся на события OnExperienceChanged и OnLevelUp из UI-скриптов.
/// </summary>
public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("Настройки уровней")]
    // Базовое количество опыта до первого уровня.
    [SerializeField] private int baseExpToLevel = 20;
    // Множитель: каждый следующий уровень требует в N раз больше опыта.
    [SerializeField] private float levelMultiplier = 1.3f;

    [Header("Все возможные перки (ScriptableObject-ассеты)")]
    // Перетащи сюда все PerkData-ассеты, которые создашь в Project-окне.
    [SerializeField] private List<PerkData> allPerks;

    [Header("Ссылки")]
    [SerializeField] private LevelUpScreen levelUpScreen; // экран выбора перка
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerAttack playerAttack;

    // Текущие значения — публичны только для чтения, чтобы HUD мог их отображать.
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentExp { get; private set; } = 0;
    public int ExpToNextLevel { get; private set; }

    // События: UI-скрипты подпишутся на них, чтобы обновлять полосы/текст.
    public System.Action<int, int> OnExperienceChanged; // (текущий опыт, нужно до уровня)
    public System.Action<int> OnLevelUp;                // (новый уровень)

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Считаем, сколько опыта нужно до первого уровня.
        ExpToNextLevel = baseExpToLevel;
    }

    /// <summary>
    /// Добавляет опыт и проверяет левелап. Вызывается из ExpOrb.OnTriggerEnter2D.
    /// </summary>
    public void AddExperience(int amount)
    {
        CurrentExp += amount;
        OnExperienceChanged?.Invoke(CurrentExp, ExpToNextLevel);

        // Пока опыта накоплено больше порога — повышаем уровень.
        // Цикл нужен на случай если за один раз набрали больше одного уровня.
        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;

            // Следующий порог растёт с каждым уровнем.
            ExpToNextLevel = Mathf.RoundToInt(ExpToNextLevel * levelMultiplier);

            CurrentLevel++;
            OnLevelUp?.Invoke(CurrentLevel);

            // Показываем экран выбора перка — он сам поставит Time.timeScale = 0.
            ShowLevelUpScreen();
        }
    }

    private void ShowLevelUpScreen()
    {
        if (levelUpScreen == null || allPerks == null || allPerks.Count == 0) return;

        // Выбираем 3 случайных уникальных перка из общего списка.
        List<PerkData> shuffled = new List<PerkData>(allPerks);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int count = Mathf.Min(3, shuffled.Count);
        List<PerkData> options = shuffled.GetRange(0, count);

        levelUpScreen.Show(options);
    }

    /// <summary>
    /// Применяет выбранный перк к игроку и оружию. Вызывается из LevelUpScreen
    /// когда игрок нажимает кнопку.
    /// </summary>
    public void ApplyPerk(PerkData perk)
    {
        // Применяем бонусы к игроку.
        if (player != null)
            player.ApplyPerkBonus(perk.bonusHealth, perk.bonusSpeed);

        // Применяем бонусы к оружию.
        if (playerAttack != null)
            playerAttack.ApplyPerkBonus(perk.bonusDamage, perk.bonusAttackSpeed, perk.bonusRadius);

        Debug.Log($"Применён перк: {perk.perkName}");
    }
}
