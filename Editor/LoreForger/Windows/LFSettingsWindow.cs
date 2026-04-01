using UnityEditor;

namespace NovaDot.LoreForger.Windows
{
    using Settings;

    public class LFSettingsWindow : EditorWindow
    {
        private static LFSettings _settings;
        private static SerializedObject _serializedConfig;

        [MenuItem("Nova Dot/Lore Forger/Lore Forger Settings")]
        public static void Open()
        {
            _settings = LFSettings.Load();
            _serializedConfig = new SerializedObject(_settings);

            GetWindow<LFSettingsWindow>("Lore Forger Settings");
        }

        private void OnGUI()
        {
            AddGeneralSettingsView();
        }

        private void AddGeneralSettingsView()
        {
            _serializedConfig.Update();

            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("GraphsFolderPath"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("DialoguesFolderPath"));

            _serializedConfig.ApplyModifiedProperties();
        }
    }
}