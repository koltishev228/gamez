using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zomboid.Core.Localization
{
    public enum GameLanguage
    {
        EN,
        RU,
        DE,
        FR
    }

    /// <summary>
    /// Простой и надежный менеджер локализации на основе CSV файлов.
    /// Читает файл формата: Key;EN;RU;DE;FR
    /// </summary>
    public static class LocalizationManager
    {
        public static GameLanguage CurrentLanguage = GameLanguage.RU; // По умолчанию русский
        
        // Словарь всех загруженных переводов для текущего языка
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();
        
        private static bool _isInitialized = false;

        public static void Initialize(GameLanguage lang)
        {
            CurrentLanguage = lang;
            _translations.Clear();

            // Загружаем наш мастер-файл локализации из Resources (назовем его Translations.csv)
            TextAsset csvFile = Resources.Load<TextAsset>("Localization/Translations");
            if (csvFile == null)
            {
                Debug.LogWarning("[LocalizationManager] Translation file not found at Resources/Localization/Translations.csv!");
                return;
            }

            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return;

            // Читаем заголовки, чтобы понять в какой колонке какой язык (Key;EN;RU;DE;FR)
            string[] headers = lines[0].Split(';');
            int targetColIndex = -1;
            
            string targetLangStr = CurrentLanguage.ToString();
            for (int i = 1; i < headers.Length; i++)
            {
                if (headers[i].Trim().Equals(targetLangStr, StringComparison.OrdinalIgnoreCase))
                {
                    targetColIndex = i;
                    break;
                }
            }

            if (targetColIndex == -1)
            {
                Debug.LogError($"[LocalizationManager] Language {targetLangStr} not found in CSV columns!");
                return;
            }

            // Читаем сами переводы
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cols = lines[i].Split(';');
                if (cols.Length <= targetColIndex) continue;

                string key = cols[0].Trim();
                string translation = cols[targetColIndex].Trim();

                if (!string.IsNullOrEmpty(key))
                {
                    _translations[key] = translation;
                }
            }

            _isInitialized = true;
            Debug.Log($"[LocalizationManager] Loaded {_translations.Count} translations for {CurrentLanguage}.");
        }

        public static string Get(string key)
        {
            if (!_isInitialized) Initialize(CurrentLanguage);

            if (string.IsNullOrEmpty(key)) return "";

            if (_translations.TryGetValue(key, out string translated))
            {
                // Если перевод пустой, пытаемся вернуть сам ключ, чтобы было видно, что нет перевода
                return string.IsNullOrEmpty(translated) ? $"[{key}]" : translated;
            }

            // Если ключа вообще нет в базе
            return $"[{key}]";
        }
    }
}
