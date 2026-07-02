using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


/// Контроллер главного меню. Содержит три кнопки:
/// - Играть: загружает игровую сцену (индекс 1 в Build Settings)
/// - Выход: закрывает приложение
///


public class MainMenuController : MonoBehaviour
{
    [Header("Кнопки")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Название игры")]
    [SerializeField] private TMP_Text titleText;

    private void Start()
    {
        // Убеждаемся что время не заморожено — если вернулись из игры
        // через GoToMenu() где стоял Time.timeScale = 0, сбрасываем его.
        Time.timeScale = 1f;

        if (titleText != null)
            titleText.text = "TOP-DOWN\nAUTOBATTLER";

        // Назначаем обработчики кнопок.
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void PlayGame()
    {
        // Загружаем игровую сцену (индекс 1 в Build Settings).
        SceneManager.LoadScene(1);
    }

    private void QuitGame()
    {
        // В редакторе Unity этот вызов ничего не делает визуально —
        // это нормально. В собранной игре (.exe) — закроет приложение.
        Application.Quit();

#if UNITY_EDITOR
        // В редакторе принудительно останавливаем Play Mode для удобства тестирования.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
