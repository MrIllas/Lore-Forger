using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovaDot.LoreForger.Windows
{
    using Elements;
    using Enumerations;
    using Data.Error;
    using Data.Save;
    using Utilities;

    public class LFGraphView : GraphView
    {
        private LFEditorWindow _editorWindow;
        private LFSearchWindow _searchWindow;

        private MiniMap _miniMap;

        private SerializableDictionary<string, LFNodeErrorData> ungroupedNodes;
        private SerializableDictionary<Group, SerializableDictionary<string, LFNodeErrorData>> groupedNodes;
        private SerializableDictionary<string, LFGroupErrorData> groups;

        private int nameErrosAmount;

        public int NameErrorsAmount
        {
            get { return nameErrosAmount; }
            set 
            { 
                nameErrosAmount = value; 
                
                if (nameErrosAmount == 0)
                {
                    _editorWindow.EnableSaving();
                }
                if (nameErrosAmount == 1)
                {
                    _editorWindow.DisableSaving();
                }
            }
        }


        public LFGraphView(LFEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

            ungroupedNodes = new SerializableDictionary<string, LFNodeErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, LFNodeErrorData>>();
            groups = new SerializableDictionary<string, LFGroupErrorData>();

            AddManipulators();
            AddSearchWindow();
            AddMiniMap();
            AddGridBackground();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
        }

        #region Overrided Methods

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port) return;

                if (startPort.node == port.node) return;

                if (startPort.direction == port.direction) return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        #endregion

        #region Manipulators

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // SELECTORS MUST BE IN THESE ORDER, OTHERWISE THERE MIGHT BE ISSUES
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", LFDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", LFDialogueType.MultipleChoice));
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                );

            return contextualMenuManipulator;
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, LFDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("DialogueName", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );

            return contextualMenuManipulator;
        }

        #endregion

        #region Element Creation

        public LFGroup CreateGroup(string title, Vector2 position)
        {
            LFGroup group = new LFGroup(title, position);

            AddGroup(group);
            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is LFNode)) continue;

                LFNode node = (LFNode) selectedElement;
                group.AddElement(node);
            }

            return group;
        }

        public LFNode CreateNode(string nodeName, LFDialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"NovaDot.LoreForger.Elements.LF{dialogueType}Node");
            LFNode node = (LFNode) Activator.CreateInstance(nodeType);

            node.Initialize(nodeName, this, position);

            if (shouldDraw) node.Draw();

            AddUngroupedNode(node);

            return node;
        }

        #endregion

        #region Callbacks

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(LFGroup);
                Type edgeType = typeof(Edge);

                List<LFNode> nodesToDelete = new List<LFNode>();
                List<Edge> edgesToDelete = new List<Edge>();
                List<LFGroup> groupsToDelete = new List<LFGroup>();

                foreach (GraphElement element in selection)
                {
                    if (element is LFNode node)
                    {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if (element.GetType() == edgeType)
                    {
                        //Edge edge = (Edge) element;

                        edgesToDelete.Add((Edge)element);

                        continue;
                    }

                    if (element.GetType() != groupType) continue;

                    //LFGroup group = (LFGroup) element;

                    groupsToDelete.Add((LFGroup) element);
                }

                // Groups must be deleted before Nodes
                //Groups
                foreach (LFGroup group in groupsToDelete)
                {
                    List<LFNode> groupNodes = new List<LFNode>();

                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if (!(groupElement is LFNode)) continue;

                        //LFNode groupNode = (LFNode) groupElement;

                        groupNodes.Add((LFNode) groupElement);
                    }

                    group.RemoveElements(groupNodes);
                    RemoveGroup(group);
                    RemoveElement(group);
                }

                //Edges
                DeleteElements(edgesToDelete);

                //Nodes
                foreach (LFNode node in nodesToDelete)
                {
                    if (node.Group != null) node.Group.RemoveElement(node);

                    RemoveUngroupedNodes(node);
                    node.DisconnectAllPorts();
                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is LFNode)) continue;

                    //LFGroup nodeGroup = (LFGroup)group;
                    //LFNode node = (LFNode) element;

                    RemoveUngroupedNodes((LFNode) element);
                    AddGroupedNode((LFNode) element, (LFGroup) group);
                }
            };
        }

        private void OnGroupElementsRemoved() 
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is LFNode)) continue;

                    //LFNode node = (LFNode) element;

                    RemoveGroupedNode((LFNode) element, group);
                    AddUngroupedNode((LFNode) element);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                LFGroup lfGroup = (LFGroup) group;

                lfGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(lfGroup.title))
                {
                    if (!string.IsNullOrEmpty(lfGroup.OldTitle)) ++NameErrorsAmount;
                }
                else
                {
                    if (string.IsNullOrEmpty(lfGroup.OldTitle)) --NameErrorsAmount;
                }

                RemoveGroup(lfGroup);
                lfGroup.OldTitle = lfGroup.title;
                AddGroup(lfGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        LFNode nextNode = (LFNode) edge.input.node;
                        LFChoiceSaveData choiceData = (LFChoiceSaveData) edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType) continue;

                        Edge edge = (Edge) element;
                        LFChoiceSaveData choiceData = (LFChoiceSaveData)(edge.output.userData);

                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }

        #endregion

        #region Repeated Elements

        public void AddUngroupedNode(LFNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                LFNodeErrorData nodeErrorData = new LFNodeErrorData();
                nodeErrorData.Nodes.Add(node);

                ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<LFNode> ungroupedNodeList = ungroupedNodes[nodeName].Nodes;
            ungroupedNodeList.Add(node);

            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (ungroupedNodeList.Count == 2)
            {
                ++NameErrorsAmount;

                ungroupedNodeList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNodes(LFNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            List<LFNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);
            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --NameErrorsAmount;

                ungroupedNodesList[0].ResetStyle();

                return;
            }

            if (ungroupedNodesList.Count == 0) ungroupedNodes.Remove(nodeName);
        }

        private void AddGroup(LFGroup group)
        {
            string groupName = group.title.ToLower();

            if (!groups.ContainsKey(groupName))
            {
                LFGroupErrorData groupErrorData = new LFGroupErrorData();

                groupErrorData.Groups.Add(group);
                groups.Add(groupName, groupErrorData);

                return;
            }

            List<LFGroup> groupList = groups[groupName].Groups;
            groupList.Add(group);

            Color errorColor = groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            if (groupList.Count == 2)
            {
                ++NameErrorsAmount;

                groupList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(LFGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();
            List<LFGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);
            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --NameErrorsAmount;

                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0) groups.Remove(oldGroupName);
        }

        public void AddGroupedNode(LFNode node, LFGroup group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
                groupedNodes.Add(group, new SerializableDictionary<string, LFNodeErrorData>());

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                LFNodeErrorData nodeErrorData = new LFNodeErrorData();
                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<LFNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;
            groupedNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;

                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(LFNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();
            List<LFNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            node.Group = null;
            groupedNodesList.Remove(node);
            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --NameErrorsAmount;

                groupedNodesList[0].ResetStyle();

                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0) groupedNodes.Remove(group);
            }
        }

        #endregion

        #region Element Addition

        private void AddSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<LFSearchWindow>();

                _searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddMiniMap()
        {
            _miniMap = new MiniMap() { anchored = true };
            _miniMap.SetPosition(new Rect(15.0f, 50.0f, 200.0f, 100.0f));
            Add(_miniMap);
            _miniMap.visible = false;
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "LoreForger/LFGraphViewStyles.uss",
                "LoreForger/LFNodeStyles.uss"
                );
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            _miniMap.style.backgroundColor = backgroundColor;
            _miniMap.style.borderTopColor = borderColor;
            _miniMap.style.borderRightColor = borderColor;
            _miniMap.style.borderBottomColor = borderColor;
            _miniMap.style.borderLeftColor = borderColor;
        }

        #endregion

        #region Utility

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;
            if (isSearchWindow) worldMousePosition -= _editorWindow.position.position;

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;

        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElements => RemoveElement(graphElements));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            NameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            _miniMap.visible = !_miniMap.visible;
        }

        #endregion
    }
}