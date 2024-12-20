using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction;
using SharedGraph;  // 引入 SharedGraph 命名空间
using System.IO;
using System;
using UnityEditor;  // 引入 UnityEditor 命名空间

public class NodeInteraction : MonoBehaviour
{
    private GraphLoader graphLoader;

    // 用于记录用户点击过的节点
    private HashSet<string> clickedNodeIds = new HashSet<string>();

    // 点击计数器
    private int clickCount = 0;

    // 最大点击次数
    public const int maxClickCount = 31;

    void Start()
    {
        // 初始化 GraphLoader
        graphLoader = FindObjectOfType<GraphLoader>();
        if (graphLoader == null)
        {
            UnityEngine.Debug.LogError("NodeInteraction: GraphLoader not found in the scene.");
            return;
        }

        // 获取 RayInteractor 组件并订阅事件
        RayInteractor rayInteractor = FindObjectOfType<RayInteractor>();
        if (rayInteractor != null)
        {
            // 避免多次订阅
            rayInteractor.OnObjectSelected -= HandleObjectSelected;
            rayInteractor.OnObjectSelected += HandleObjectSelected;
        }
        else
        {
            UnityEngine.Debug.LogError("NodeInteraction: RayInteractor not found in the scene.");
        }
    }

    private void HandleObjectSelected(GameObject selectedObject)
    {
        // 打印 selectedObject 和 this.gameObject 的名称，确认是否匹配
        UnityEngine.Debug.Log($"HandleObjectSelected: selectedObject = {selectedObject.name}, this.gameObject = {this.gameObject.name}");

        // 记录点击过的节点
        if (selectedObject != null)
        {
            clickedNodeIds.Add(selectedObject.name); // 将选中的节点ID加入到已点击节点的列表
            UnityEngine.Debug.Log($"Node '{selectedObject.name}' added to clicked nodes.");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Selected object '{selectedObject.name}' does not match this node '{this.gameObject.name}'.");
        }

        // 增加点击次数
        clickCount++;

        //// 如果点击次数达到最大次数，自动退出程序
        //if (clickCount >= maxClickCount)
        //{
        //    UnityEngine.Application.Quit();
        //}

        // 如果点击次数达到最大次数，自动退出程序（退出 Play Mode）
        if (clickCount >= maxClickCount)
        {
            // 在编辑器中退出 Play Mode
            UnityEditor.EditorApplication.isPlaying = false;

            UnityEngine.Debug.Log("Max click count reached. Exiting Play Mode.");
        }

        // 高亮当前节点
        if (selectedObject == this.gameObject)
        {
            if (HighlightManager.Instance != null)
            {
                HighlightManager.Instance.HighlightNode(this.gameObject); // 执行高亮
            }
            else
            {
                UnityEngine.Debug.LogError("NodeInteraction: HighlightManager instance not found.");
            }
        }
    }

    // 在应用程序退出时调用
    private void OnApplicationQuit()
    {
        // 调用导出功能，保存用户点击的数据
        string filePath = "Assets/ExportedFiles/clicked_nodes.csv";
        ExportClickedNodesToCSV(filePath);
    }

    /// <summary>
    /// 将已点击的节点ID列表保存为CSV文件
    /// </summary>
    public void ExportClickedNodesToCSV(string filePath)
    {
        try
        {
            // 创建CSV内容
            List<string> clickedNodesList = new List<string>(clickedNodeIds);

            // 如果没有点击节点，直接返回并记录日志
            if (clickedNodesList.Count == 0)
            {
                UnityEngine.Debug.LogWarning("NodeInteraction: No nodes were clicked. CSV export skipped.");
                return;
            }

            string csvContent = "NodeID,PositionX,PositionY,PositionZ,Size,Group\n"; // CSV表头

            // 遍历已点击的节点ID，找到每个节点的详细信息
            foreach (var nodeId in clickedNodesList)
            {
                // 从 GraphLoader 中找到对应的节点数据
                NodeData nodeData = GetNodeDataById(nodeId);
                if (nodeData != null)
                {
                    string position = nodeData.GetPositionAsString(); // 使用 GetPositionAsString 来获取位置
                    string size = nodeData.size.ToString();

                    // 处理 group 信息（假设是 int 类型）
                    string group = nodeData.group.ToString();  // 直接获取 group 字段的值

                    // 将节点的详细信息写入CSV
                    csvContent += $"{nodeId},{position},{size},{group}\n";
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"NodeInteraction: Node data for '{nodeId}' not found.");
                }
            }


            // 写入到文件
            File.WriteAllText(filePath, csvContent);
            UnityEngine.Debug.Log($"NodeInteraction: Exported clicked nodes to '{filePath}' successfully.");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"NodeInteraction: Failed to export clicked nodes to CSV. Error: {ex.Message}");
        }
    }

    // 根据NodeID从GraphLoader中获取对应的NodeData
    private NodeData GetNodeDataById(string nodeId)
    {
        if (graphLoader != null && graphLoader.graph != null)
        {
            return graphLoader.graph.nodes.FirstOrDefault(node => node.id == nodeId); // 查找并返回节点数据
        }
        return null;
    }

    // 获取与当前节点相关的Top节点（排除掉已经激活的节点以及固定节点）
    public List<GameObject> GetTopRelatedNodes(int count)
    {
        List<NodeData> relatedNodesData = GetRelatedNodes(this.gameObject.name);  // 获取所有相关节点数据
        List<GameObject> relatedNodes = new List<GameObject>();

        // 获取当前节点的 NodeData 信息
        NodeData currentNodeData = graphLoader.graph.nodes.FirstOrDefault(n => n.id == this.gameObject.name);
        if (currentNodeData == null)
        {
            UnityEngine.Debug.LogWarning("NodeInteraction: Current node data not found.");
            return relatedNodes;
        }

        int currentNodeGroup = currentNodeData.group;  // 获取当前节点的组信息

        // 记录已访问的节点，避免重复访问
        HashSet<string> visitedNodes = new HashSet<string>();

        // 获取同组节点和不同组节点
        List<NodeData> sameGroupNodes = relatedNodesData.Where(n => n.group == currentNodeGroup).ToList();
        List<NodeData> otherGroupNodes = relatedNodesData.Where(n => n.group != currentNodeGroup).ToList();
        List<NodeData> allNodes = graphLoader.graph.nodes.ToList();  // 获取所有节点，包括没有链接的

        // 随机选择节点
        System.Random rand = new System.Random();

        // 按照给定的数量选择节点
        while (relatedNodes.Count < count)
        {
            NodeData selectedNode = null;

            // 尝试优先选择有连接的节点，直到没有足够的连接节点
            if (visitedNodes.Count < relatedNodesData.Count)  // 还有足够的节点可以选择
            {
                float probability = (float)rand.NextDouble();  // [0, 1)

                // 10% 概率选择不同组的节点
                if (probability <= 0.1f && otherGroupNodes.Count > 0)
                {
                    // 随机从其他组中选择一个节点
                    selectedNode = otherGroupNodes[rand.Next(otherGroupNodes.Count)];
                    UnityEngine.Debug.Log($"Selected from other group: {selectedNode.id}");
                }
                else  // 90% 概率选择同组的节点
                {
                    // 随机从同组中选择一个节点
                    selectedNode = sameGroupNodes[rand.Next(sameGroupNodes.Count)];
                    UnityEngine.Debug.Log($"Selected from same group: {selectedNode.id}");
                }
            }

            // 如果没有足够的连接节点，允许选择其他节点（即使没有连接）
            if (selectedNode == null || visitedNodes.Contains(selectedNode.id))
            {
                // 从所有节点中随机选择一个节点
                selectedNode = allNodes[rand.Next(allNodes.Count)];
                UnityEngine.Debug.Log($"Selected from all nodes (no link or repeated): {selectedNode.id}");
            }

            // 确保选中的节点没有被访问过
            if (!visitedNodes.Contains(selectedNode.id))
            {
                visitedNodes.Add(selectedNode.id);  // 标记为已访问
                GameObject nodeObject = graphLoader.nodes[selectedNode.id];
                relatedNodes.Add(nodeObject);

                UnityEngine.Debug.Log($"Added Node: {selectedNode.id}, Group: {selectedNode.group}, Size: {selectedNode.size}");
            }

            // 如果选中的节点已被访问过，跳过当前循环，重新选择
        }

        UnityEngine.Debug.Log($"Total selected nodes: {relatedNodes.Count}");
        return relatedNodes;
    }

    // 获取与当前节点相关的节点
    private List<NodeData> GetRelatedNodes(string nodeName)
    {
        List<NodeData> relatedNodes = new List<NodeData>();

        foreach (var link in graphLoader.links)
        {
            string[] nodes = link.Key.Split('_');
            if (nodes.Length != 2)
            {
                continue;
            }

            if (nodes[0] == nodeName || nodes[1] == nodeName)
            {
                string relatedNodeName = nodes[0] == nodeName ? nodes[1] : nodes[0];
                NodeData relatedNode = graphLoader.graph.nodes.FirstOrDefault(n => n.id == relatedNodeName);
                if (relatedNode != null)
                {
                    relatedNodes.Add(relatedNode);
                }
            }
        }

        // 按照节点的 size 排序，降序排列
        return relatedNodes.OrderByDescending(n => n.size).ToList();
    }
}
