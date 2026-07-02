using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Экран выбора перка при левелапе.
/// При показе останавливает время (timeScale = 0), при выборе — возобновляет.
/// Содержит 3 кнопки, каждая динамически заполняется данными из PerkData.
/// </summary>
public class LevelUpScreen : MonoBehaviour
{
    [Header("Панель и заголовок")]
    [SerializeField] private GameObject panel;       // корневой объект экрана (включаем/выключаем)
    [SerializeField] private TMP_Text titleText;     // "Выберите перк!"

    [Header("Три кнопки перков")]
    // Три кнопки — по одной на каждый вариант.
    [SerializeField] private Button[] perkButtons;           // сами кнопки
    [SerializeField] private TMP_Text[] perkNameTexts;       // тексты имён перков
    [SerializeField] private TMP_Text[] perkDescTexts;       // тексты описаний

    // Список перков, которые сейчас отображаются (нужен чтобы знать какой выбрали).
    private List<PerkData> _currentOptions;

    private void Awake()
    {
        // Экран начинает скрытым.
        panel.SetActive(false);
    }

    /// <summary>
    /// Показывает экран с переданными вариантами перков. Останавливает игру.
    /// </summary>
    public void Show(List<PerkData> options)
    {
        _currentOptions = options;
        panel.SetActive(true);
        Time.timeScale = 0f; // пауза — враги перестают двигаться пока выбираем

        // Заполняем кнопки данными из ScriptableObject-ассетов.
        for (int i = 0; i < perkButtons.Length; i++)
        {
            if (i < options.Count)
            {
                perkButtons[i].gameObject.SetActive(true);
                perkNameTexts[i].text = options[i].perkName;
                perkDescTexts[i].text = options[i].description;

                // Захватываем индекс в локальную переменную для лямбды —
                // иначе все кнопки будут использовать одно и то же значение i.
                int index = i;
                perkButtons[i].onClick.RemoveAllListeners();
                perkButtons[i].onClick.AddListener(() => OnPerkSelected(index));
            }
            else
            {
                // Если перков меньше 3 — скрываем лишние кнопки.
                perkButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnPerkSelected(int index)
    {
        // Применяем перк через ExperienceManager.
        ExperienceManager.Instance.ApplyPerk(_currentOptions[index]);

        // Скрываем экран и возобновляем игру.
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
