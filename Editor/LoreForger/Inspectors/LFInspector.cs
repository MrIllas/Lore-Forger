using System.Collections.Generic;
using UnityEditor;

namespace NovaDot.LoreForger.Inspectors
{
    using Settings;
    using SO;
    using Utilities;

    [CustomEditor(typeof(LFDialogue))]
    public class LFInspector : Editor
    {
        private LFSettings _settings;

        /* Dialogue Scriptable Objects */
        private SerializedProperty dialogueContainerProperty;
        private SerializedProperty dialogueGroupProperty;
        private SerializedProperty dialogueProperty;

        /* Filters */
        private SerializedProperty groupedDialoguesProperty;
        private SerializedProperty startingDialoguesOnlyProperty;

        /* Indexes */
        private SerializedProperty selectedDialogueGroupIndexProperty;
        private SerializedProperty selectedDialogueIndexProperty;

        private void OnEnable()
        {
            _settings = LFSettings.Load();

            dialogueContainerProperty = serializedObject.FindProperty("dialogueContainer");
            dialogueGroupProperty = serializedObject.FindProperty("dialogueGroup");
            dialogueProperty = serializedObject.FindProperty("dialogue");

            groupedDialoguesProperty = serializedObject.FindProperty("groupedDialogues");
            startingDialoguesOnlyProperty = serializedObject.FindProperty("startingDialoguesOnly");

            selectedDialogueGroupIndexProperty = serializedObject.FindProperty("selectedDialogueGroupIndex");
            selectedDialogueIndexProperty = serializedObject.FindProperty("selectedDialogueIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDialogueContainerArea();

            LFDialogueContainerSO dialogueContainer = (LFDialogueContainerSO)dialogueContainerProperty.objectReferenceValue;

            if (dialogueContainer == null)
            {
                StopDrawing("Select a Dialogue Container to see the rest of the Inspector");

                return;
            }

            DrawFiltersArea();

            bool currentStartingDialoguesOnlyFilter = startingDialoguesOnlyProperty.boolValue;

            List<string> dialogueNames;

            string dialogueFolderPath = _settings.DialoguesFolderPath + dialogueContainer.FileName;
            string dialogueInfoMessage;


            if (groupedDialoguesProperty.boolValue)
            {
                List<string> dialogueGroupNames = dialogueContainer.GetDialogueGroupNames();

                if (dialogueGroupNames.Count == 0)
                {
                    StopDrawing("There are no Dialogue Groups in this Dialogue Container.");

                    return;
                }

                DrawDialogueGroupArea(dialogueContainer, dialogueGroupNames);

                LFDialogueGroupSO dialogueGroup = (LFDialogueGroupSO) dialogueGroupProperty.objectReferenceValue;

                dialogueNames = dialogueContainer.GetGroupedDialogueNames(dialogueGroup, currentStartingDialoguesOnlyFilter);
                dialogueFolderPath += "/Groups/" + dialogueGroup.GroupName + "/Dialogues";
                dialogueInfoMessage = "There are no " + (currentStartingDialoguesOnlyFilter ? "Starting " : "") + "Dialogues in this Dialogue Group.";
            }
            else
            {
                dialogueNames = dialogueContainer.GetUngroupedDialogueNames(currentStartingDialoguesOnlyFilter);
                dialogueFolderPath += "/Global/Dialogues";
                dialogueInfoMessage = "There are no " + (currentStartingDialoguesOnlyFilter ? "Starting " : "") + "Ungrouped Dialogues in this Dialogue Container.";
            }

            if (dialogueNames.Count == 0)
            {
                StopDrawing(dialogueInfoMessage);

                return;
            }

            DrawDialogueArea(dialogueNames, dialogueFolderPath);

            serializedObject.ApplyModifiedProperties();
        }

        #region Draw Methods

        private void DrawDialogueContainerArea()
        {
            EditorGUILayout.LabelField("Dialogue Container", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dialogueContainerProperty);

            EditorGUILayout.Space(4);
        }

        private void DrawFiltersArea()
        {
            EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(groupedDialoguesProperty);
            EditorGUILayout.PropertyField(startingDialoguesOnlyProperty);

            EditorGUILayout.Space(4);
        }

        private void DrawDialogueGroupArea(LFDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            EditorGUILayout.LabelField("Dialogue Group", EditorStyles.boldLabel);

            int oldSelectedDialogueGroupIndex = selectedDialogueGroupIndexProperty.intValue;
            LFDialogueGroupSO oldDialogueGroup = (LFDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;
            bool isOldDialogueGroupNull = oldDialogueGroup == null;
            string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;

            UpdateIndexOnNamesListUpdate(dialogueGroupNames, selectedDialogueGroupIndexProperty, oldSelectedDialogueGroupIndex, oldDialogueGroupName, isOldDialogueGroupNull);
            selectedDialogueGroupIndexProperty.intValue = EditorGUILayout.Popup("Dialogue Group", selectedDialogueGroupIndexProperty.intValue, dialogueGroupNames.ToArray());

            string selectedDialogueGroupName = dialogueGroupNames[selectedDialogueGroupIndexProperty.intValue];
            LFDialogueGroupSO selectedDialogueGroup = LFIOUtility.LoadAsset<LFDialogueGroupSO>(_settings.DialoguesFolderPath + dialogueContainer.FileName + "/Groups/" + selectedDialogueGroupName, selectedDialogueGroupName);

            dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(dialogueGroupProperty);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(4);
        }

        private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            EditorGUILayout.LabelField("Dialogue", EditorStyles.boldLabel);

            int oldSelectedDialogueIndex = selectedDialogueIndexProperty.intValue;
            LFDialogueSO oldDialogue = (LFDialogueSO) dialogueProperty.objectReferenceValue;
            bool isOldDialogueNull = oldDialogue == null;
            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;

            UpdateIndexOnNamesListUpdate(dialogueNames, selectedDialogueIndexProperty, oldSelectedDialogueIndex, oldDialogueName, isOldDialogueNull);
            selectedDialogueIndexProperty.intValue = EditorGUILayout.Popup("Dialogue", selectedDialogueIndexProperty.intValue, dialogueNames.ToArray());

            string selectedDialogueName = dialogueNames[selectedDialogueIndexProperty.intValue];
            LFDialogueSO selectedDialogue = LFIOUtility.LoadAsset<LFDialogueSO>(dialogueFolderPath, selectedDialogueName);

            dialogueProperty.objectReferenceValue = selectedDialogue;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(dialogueProperty);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(4);
        }

        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            EditorGUILayout.HelpBox(reason, messageType);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("You need to select a Dialogue for this component to work properly at Runtime!", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Index Mehods

        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldSelectedPropertyIndex,string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;

                return;
            }

            bool oldIndexIsOutOfBoundsOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBoundsOfNamesListCount || oldPropertyName != optionNames[oldSelectedPropertyIndex];

            if (oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);
                }
                else
                {
                    indexProperty.intValue = 0;
                }
            }
        }

        #endregion
    }
}

