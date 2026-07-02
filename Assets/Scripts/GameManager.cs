using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Синглтон-менеджер игры. Следит за состоянием:
/// - Playing: обычный геймплей
/// - GameOver: игрок умер → экран поражения
/// - Victory: игрок выжил нужное время → экран победы
///
/// Подписывается на PlayerController.OnDied и сам считает таймер победы.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Время для победы (секунды)")]
    [SerializeField] private float victoryTime = 180f; // 3 минуты по умолчанию

    [Header("Экран Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverKillsText;  // "Убито врагов: 42"
    [SerializeField] private TMP_Text gameOverTimeText;   // "Время: 01:23"
    [SerializeField] private Button restartButton;        // кнопка "Заново"
    [SerializeField] private Button menuButton;           // кнопка "В меню"

    [Header("Экран победы")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryKillsText;
    [SerializeField] private TMP_Text victoryTimeText;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryMenuButton;

    [Header("Ссылки")]
    [SerializeField] private PlayerController player;
    [SerializeField] private HUDController hud;
    [SerializeField] private EnemySpawner spawner;

    private bool _gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Скрываем оба экрана при старте.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void Start()
    {
        // Подписываемся на смерть игрока.
        if (player != null)
            player.OnDied += OnPlayerDied;

        // Назначаем кнопки.
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
        if (victoryRestartButton != null) victoryRestartButton.onClick.AddListener(RestartGame);
        if (victoryMenuButton != null) victoryMenuButton.onClick.AddListener(GoToMenu);
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnDied -= OnPlayerDied;
    }

    private void Update()
    {
        if (_gameEnded) return;

        // Проверяем победу по таймеру HUD.
        if (hud != null && hud.SurvivalTime >= victoryTime)
            OnVictory();
    }

    // Вызывается из HUDController.RegisterKill() —
    // оставляем публичным на случай если захочешь добавить доп. логику при убийстве.
    public void OnEnemyKilled()
    {
        if (hud != null) hud.RegisterKill();
    }

    private void OnPlayerDied()
    {
        if (_gameEnded) return;
        EndGame(false);
    }

    private void OnVictory()
    {
        if (_gameEnded) return;
        EndGame(true);
    }

    private void EndGame(bool isVictory)
    {
        _gameEnded = true;

        // Останавливаем игру: таймер, спавнер.
        if (hud != null) hud.StopTimer();
        if (spawner != null) spawner.enabled = false;

        // Замедляем время — враги замирают на экране финала.
        Time.timeScale = 0f;

        // Форматируем итоговую статистику.
        string killsStr = hud != null ? $"Убито врагов: {hud.KillCount}" : "";
        string timeStr = "";
        if (hud != null)
        {
            int min = (int)(hud.SurvivalTime / 60f);
            int sec = (int)(hud.SurvivalTime % 60f);
            timeStr = $"Время: {min:00}:{sec:00}";
        }

        if (isVictory)
        {
            // Победа.
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (victoryKillsText != null) victoryKillsText.text = killsStr;
            if (victoryTimeText != null) victoryTimeText.text = timeStr;
        }
        else
        {
            // Поражение.
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverKillsText != null) gameOverKillsText.text = killsStr;
            if (gameOverTimeText != null) gameOverTimeText.text = timeStr;
        }
    }

    private void RestartGame()
    {
        // Сбрасываем timeScale перед перезагрузкой сцены — иначе следующая
        // сцена тоже стартует на паузе.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // сцена с главным меню всегда под индексом 0
    }
}
