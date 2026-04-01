using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NovaDot.LoreForger.Utilities
{
    using Data;
    using Data.Save;
    using Elements;
    using Settings;
    using SO;
    using Windows;

    public static class LFIOUtility
    {
        private static LFGraphView _graphView;
        private static LFSettings _settings;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<LFGroup> groups;
        private static List<LFNode> nodes;

        private static Dictionary<string, LFDialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, LFDialogueSO> createdDialogues;

        private static Dictionary<string, LFGroup> loadedGroups;
        private static Dictionary<string, LFNode> loadedNodes;

        public static void Initialize(LFGraphView graphView, LFSettings settings, string graphName)
        {
            _graphView = graphView;
            _settings = settings;

            graphFileName = graphName;
            //containerFolderPath = "Assets/LoreForger/Dialogues/"+graphFileName;
            containerFolderPath = _settings.DialoguesFolderPath + graphFileName;

            groups = new List<LFGroup>();
            nodes = new List<LFNode>();

            createdDialogueGroups = new Dictionary<string, LFDialogueGroupSO>();
            createdDialogues = new Dictionary<string, LFDialogueSO>();

            loadedGroups = new Dictionary<string, LFGroup>();
            loadedNodes = new Dictionary<string, LFNode>();
        }

        #region Save Methods
        public static void Save()
        {
            CreateStaticFolder();
            GetElementsFromGraphView();

            LFGraphSaveDataSO graphData = CreateAsset<LFGraphSaveDataSO>(_settings.GraphsFolderPath, graphFileName+"Graph");
            graphData.Initialize(graphFileName);

            LFDialogueContainerSO dialogueContainer = CreateAsset<LFDialogueContainerSO>(containerFolderPath, graphFileName);
            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        #region Groups

        private static void SaveGroups(LFGraphSaveDataSO graphData, LFDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (LFGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(LFGroup group, LFGraphSaveDataSO graphData)
        {
            LFGroupSaveData groupData = new LFGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position,
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(LFGroup group, LFDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder(containerFolderPath + "/Groups", groupName);
            CreateFolder(containerFolderPath + "/Groups/" + groupName, "Dialogues");

            LFDialogueGroupSO dialogueGroup = CreateAsset<LFDialogueGroupSO>(containerFolderPath + "/Groups/"+groupName, groupName);
            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<LFDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, LFGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach(string groupToRemove in groupsToRemove)
                {
                    RemoveFolder(containerFolderPath+"/Groups/"+groupToRemove);
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        #endregion

        #region Nodes

        private static void SaveNodes(LFGraphSaveDataSO graphData, LFDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (LFNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);

                    continue;
                }
            }

            UpdateDialogueChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
            
        }

        private static void SaveNodeToScriptableObject(LFNode node, LFDialogueContainerSO dialogueContainer)
        {
            LFDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<LFDialogueSO>(containerFolderPath + "/Groups/" + node.Group.title + "/Dialogues", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<LFDialogueSO>(containerFolderPath + "/Global/Dialogues", node.DialogueName);
                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(node.DialogueName, node.Text, ConvertNodeChoicesToDialogueChoices(node.Choices), node.DialogueType, node.IsStartingNode());

            createdDialogues.Add(node.ID, dialogue);

            SaveAsset(dialogue);
        }

        private static List<LFDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<LFChoiceSaveData> nodeChoices)
        {
            List<LFDialogueChoiceData> dialogueChoices = new List<LFDialogueChoiceData>();

            foreach (LFChoiceSaveData nodeChoice in nodeChoices)
            {
                LFDialogueChoiceData choiceData = new LFDialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static void UpdateDialogueChoicesConnections()
        {
            foreach (LFNode node in nodes)
            {
                LFDialogueSO dialogue = createdDialogues[node.ID];

                for (int choiceIndex = 0; choiceIndex < node.Choices.Count; choiceIndex++)
                {
                    LFChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.NodeID)) continue;

                    dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, LFGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach(KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();

                    foreach (string nodeToRemove in nodesToRemove)
                        RemoveAsset(containerFolderPath + "/Groups/" + oldGroupedNode.Key + "/Dialogues", nodeToRemove);
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, LFGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach(string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset(containerFolderPath + "/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        #endregion

        #endregion

        #region Load Methods

        public static void Load()
        {
            LFGraphSaveDataSO graphData = LoadAsset<LFGraphSaveDataSO>(_settings.GraphsFolderPath, graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Couldn't load the graph.",
                    "The graph at the following path could not be found:\n\n" +
                    _settings.GraphsFolderPath + graphFileName+"\n\n"+
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Okay");

                return;
            }

            LFEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<LFGroupSaveData> groups)
        {
            foreach(LFGroupSaveData groupData in groups)
            {
                LFGroup group = _graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<LFNodeSaveData> nodes)
        {
            foreach(LFNodeSaveData nodeData in nodes)
            {
                List<LFChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
                LFNode node = _graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;
                node.Draw();

                _graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID)) continue;

                LFGroup group = loadedGroups[nodeData.GroupID];
                node.Group = group;

                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            foreach(KeyValuePair<string, LFNode> loadedNode in loadedNodes)
            {
                foreach(Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    LFChoiceSaveData choiceData = (LFChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeID)) continue;

                    //LFNode nextNode = loadedNodes[choiceData.NodeID];
                    //Port nextNodeInputPort = (Port) loadedNodes[choiceData.NodeID].inputContainer.Children().First();
                    Edge edge = choicePort.ConnectTo((Port) loadedNodes[choiceData.NodeID].inputContainer.Children().First());

                    _graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        #endregion

        #region Creation Methods

        private static void CreateStaticFolder()
        {
            string graphsPath;
            string graphsFolder;
            LFSettings.GetFolderAndPath(_settings.GraphsFolderPath, out graphsPath, out graphsFolder);

            CreateFolder(graphsPath, graphsFolder);

            string dialoguePath;
            string dialogueFolder;
            LFSettings.GetFolderAndPath(_settings.DialoguesFolderPath, out dialoguePath, out dialogueFolder);

            //CreateFolder("Assets", "LoreForger");
            CreateFolder(dialoguePath, dialogueFolder);

            CreateFolder(_settings.DialoguesFolderPath, graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder(containerFolderPath + "/Global", "Dialogues");
        }

        #endregion

        #region Utility Methods

        public static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder(path + "/" + folderName)) return;

            if (!string.IsNullOrEmpty(path) && path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

            AssetDatabase.CreateFolder(path, folderName);
        }

        public static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory(fullPath+".meta");
            FileUtil.DeleteFileOrDirectory(fullPath + "/");
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = path + '/' + assetName + ".asset";
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = path + '/' + assetName + ".asset";

            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset(path +"/"+assetName+".asset");
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<LFChoiceSaveData> CloneNodeChoices(List<LFChoiceSaveData> nodeChoices)
        {
            List<LFChoiceSaveData> choices = new List<LFChoiceSaveData>();

            foreach (LFChoiceSaveData choice in nodeChoices)
            {
                LFChoiceSaveData choiceData = new LFChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID,
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        public static void SaveNodeToGraph(LFNode node, LFGraphSaveDataSO graphData)
        {
            List<LFChoiceSaveData> choices = CloneNodeChoices(node.Choices);

            LFNodeSaveData nodeData = new LFNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID, // ? if the group is null it wont try to access ID
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }

        #endregion

        #region Fetch Methods

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(LFGroup); 

            _graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is LFNode node)
                {
                    nodes.Add(node);

                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    LFGroup group = (LFGroup) graphElement;
                    groups.Add(group);

                    return;
                }
            });
        }

        #endregion
    }
}
