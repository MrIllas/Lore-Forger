using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NovaDot.LoreForger.Windows
{
    using Settings;
    using Utilities;

    public class LFEditorWindow : EditorWindow
    {
        private LFGraphView _graphView;
        private LFSettings _settings;

        private readonly string defaultFilename = "DialogueFilename";
        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;

        [MenuItem("Nova Dot/Lore Forger/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<LFEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            _settings = LFSettings.Load();

            AddGraphView();
            AddToolbar();

            rootVisualElement.AddStyleSheets("LoreForger/LFVariables.uss");
        }

        #region Elements Addition

        private void AddGraphView()
        {
            _graphView = new LFGraphView(this);
            _graphView.StretchToParentSize();

            rootVisualElement.Add(_graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = LFElementUtility.CreateTextField(defaultFilename, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = LFElementUtility.CreateButton("Save", () => Save());

            Button loadButton = LFElementUtility.CreateButton("Load", () => Load());
            Button clearButton = LFElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = LFElementUtility.CreateButton("Reset", () => ResetGraph());

            miniMapButton = LFElementUtility.CreateButton("Minimap", () => ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            toolbar.AddStyleSheets("LoreForger/LFToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        #endregion

        #region Toolbar Actions

        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name",
                    "Please ensure the file name you have typed-in is valid.",
                    "Okay"
                    );

                return;
            }

            LFIOUtility.Initialize(_graphView, _settings, fileNameTextField.value);
            LFIOUtility.Save();
        }

        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", _settings.GraphsFolderPath, "asset");

            if (string.IsNullOrEmpty(filePath)) return;

            Clear();

            LFIOUtility.Initialize(_graphView, _settings, Path.GetFileNameWithoutExtension(filePath));
            LFIOUtility.Load();
        }

        private void Clear()
        {
            _graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFilename);
        }

        private void ToggleMiniMap()
        {
            _graphView.ToggleMiniMap();
            miniMapButton.ToggleInClassList("lf-toolbar__button__selected");
        }

        #endregion

        #region Utility Methods

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
        #endregion
    }
}