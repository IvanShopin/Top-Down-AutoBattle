using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Контроллер HUD. Подписывается на события PlayerController и ExperienceManager
/// и обновляет UI-элементы. Никакого опроса в Update — только реакция на события.
///
/// Почему через события, а не через Update?
/// Update вызывается 60 раз в секунду. HP меняется раз в несколько секунд.
/// Обновлять полосу 60 раз в секунду когда ничего не изменилось — лишняя работа.
/// Событие срабатывает только тогда, когда значение реально изменилось.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Полоска HP")]
    [SerializeField] private Slider healthBar;        // сам слайдер
    [SerializeField] private TMP_Text healthText;     // текст "75 / 100"

    [Header("Полоска опыта")]
    [SerializeField] private Slider expBar;           // слайдер опыта
    [SerializeField] private TMP_Text levelText;      // текст "Уровень 3"

    [Header("Информация")]
    [SerializeField] private TMP_Text timerText;      // таймер выживания "01:23"
    [SerializeField] private TMP_Text killCountText;  // счётчик убийств "Убито: 42"

    [Header("Ссылки")]
    [SerializeField] private PlayerController player;

    private int _killCount = 0;
    private float _survivalTime = 0f;
    private bool _isGameRunning = true; // пауза при левелапе не ломает таймер

    private void Start()
    {
        // Подписываемся на события — HUD будет реагировать только при изменениях.
        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthBar;
            // Инициализируем полоску начальными значениями сразу при старте.
            UpdateHealthBar(player.CurrentHealth, player.MaxHealth);
        }

        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnExperienceChanged += UpdateExpBar;
            ExperienceManager.Instance.OnLevelUp += UpdateLevel;
            // Инициализируем начальные значения.
            UpdateExpBar(0, ExperienceManager.Instance.ExpToNextLevel);
            UpdateLevel(ExperienceManager.Instance.CurrentLevel);
        }

        _killCount = 0;
        UpdateKillCount();
    }

    private void OnDestroy()
    {
        // Отписываемся при уничтожении объекта — иначе при перезапуске сцены
        // старые подписки могут вызвать NullReferenceException.
        if (player != null)
            player.OnHealthChanged -= UpdateHealthBar;

        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnExperienceChanged -= UpdateExpBar;
            ExperienceManager.Instance.OnLevelUp -= UpdateLevel;
        }
    }

    private void Update()
    {
        // Таймер — единственное что обновляем в Update, потому что он идёт каждую секунду.
        // Time.timeScale = 0 на экране левелапа — таймер автоматически встаёт на паузу.
        if (!_isGameRunning) return;

        _survivalTime += Time.deltaTime;
        UpdateTimer();
    }

    // Вызывается из GameManager когда игра заканчивается — останавливаем таймер.
    public void StopTimer() => _isGameRunning = false;

    // Вызывается из Enemy.Die() через GameManager или напрямую.
    public void RegisterKill()
    {
        _killCount++;
        UpdateKillCount();
    }

    // Публичное свойство для GameManager (чтобы показать финальное время на экране победы).
    public float SurvivalTime => _survivalTime;
    public int KillCount => _killCount;

    // ---- Методы обновления UI ----

    private void UpdateHealthBar(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
        if (healthText != null)
            healthText.text = $"{current} / {max}";
    }

    private void UpdateExpBar(int current, int toNext)
    {
        if (expBar != null)
        {
            expBar.maxValue = toNext;
            expBar.value = current;
        }
    }

    private void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Уровень {level}";
    }

    private void UpdateTimer()
    {
        if (timerText == null) return;
        int minutes = (int)(_survivalTime / 60f);
        int seconds = (int)(_survivalTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateKillCount()
    {
        if (killCountText != null)
            killCountText.text = $"Убито: {_killCount}";
    }
}
