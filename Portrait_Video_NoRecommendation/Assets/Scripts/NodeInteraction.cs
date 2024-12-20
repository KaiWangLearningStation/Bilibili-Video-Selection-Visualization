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
    public const int maxClickCount = 5;

    private static GameObject lastClickedNode; // 记录上次点击的节点

    // 新增：用于保存高亮的节点
    private List<GameObject> highlightedNodes = new List<GameObject>();

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

        // 查找 HighlightManager 挂载的 GameObject（假设它的名称是 "HighlightManagerObject"）
        GameObject highlightManagerObject = GameObject.Find("HighlightManager");

        // 获取 HighlightManager 组件
        HighlightManager highlightManager = highlightManagerObject.GetComponent<HighlightManager>();


        // 访问高亮节点
        highlightedNodes = highlightManager.GetHighlightedNodes(); // 将高亮的节点保存到成员变量中

    }

    private void HandleObjectSelected(GameObject selectedObject)
    {
        UnityEngine.Debug.Log($"HandleObjectSelected: selectedObject = {selectedObject.name}, this.gameObject = {this.gameObject.name}");

        // 记录点击过的节点
        if (selectedObject != null)
        {
            clickedNodeIds.Add(selectedObject.name);
            UnityEngine.Debug.Log($"Node '{selectedObject.name}' added to clicked nodes.");
        }

        clickCount++;

        // 如果点击次数达到最大次数，退出程序
        if (clickCount >= maxClickCount)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            UnityEngine.Debug.Log("Max click count reached. Exiting Play Mode.");
        }

        // 高亮当前节点
        if (selectedObject == this.gameObject)
        {
            if (HighlightManager.Instance != null)
            {
                HighlightManager.Instance.HighlightNodeChain(this.gameObject);  // 调用链式高亮
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
        // 调用导出功能，保存高亮的节点数据
        string filePath = "Assets/ExportedFiles/highlighted_nodes.csv";
        ExportClickedNodesToCSV(filePath);  // 调用导出CSV方法
    }




    /// <summary>
    /// 将已点击的节点ID列表保存为CSV文件
    /// </summary>
    public void ExportClickedNodesToCSV(string filePath)
    {
        try
        {
            // 创建CSV内容
            List<string> highlightedNodesList = new List<string>();

            // 提取高亮节点的ID
            foreach (var node in highlightedNodes)
            {
                if (node != null)
                {
                    highlightedNodesList.Add(node.name);  // 取出高亮节点的名称或ID
                }
            }

            string csvContent = "NodeID,PositionX,PositionY,PositionZ,Size,Group\n"; // CSV表头

            // 遍历已点击的节点ID，找到每个节点的详细信息
            foreach (var nodeId in highlightedNodesList)
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

    public List<GameObject> GetTopRelatedNodes(int count)
    {
        // 获取所有节点数据
        List<NodeData> allNodesData = graphLoader.graph.nodes.ToList();
        List<GameObject> relatedNodes = new List<GameObject>();

        // 记录已访问的节点，避免重复访问
        HashSet<string> visitedNodes = new HashSet<string>();

        // 随机选择节点
        System.Random rand = new System.Random();

        // 按照给定的数量选择节点
        while (relatedNodes.Count < count)
        {
            // 随机从所有节点中选择一个节点
            NodeData selectedNode = allNodesData[rand.Next(allNodesData.Count)];

            // 确保选中的节点没有被访问过
            if (!visitedNodes.Contains(selectedNode.id))
            {
                visitedNodes.Add(selectedNode.id);  // 标记为已访问
                GameObject nodeObject = graphLoader.nodes[selectedNode.id];
                relatedNodes.Add(nodeObject);

                UnityEngine.Debug.Log($"Added Node: {selectedNode.id}, Group: {selectedNode.group}, Size: {selectedNode.size}");
            }
        }

        UnityEngine.Debug.Log($"Total selected nodes: {relatedNodes.Count}");
        return relatedNodes;
    }

    private List<NodeData> GetRelatedNodes(string nodeName)
    {
        // 获取所有节点数据
        List<NodeData> allNodesData = graphLoader.graph.nodes.ToList();
        List<NodeData> relatedNodes = new List<NodeData>();

        // 随机选择节点
        System.Random rand = new System.Random();

        // 遍历所有节点，随机选择相关节点（不考虑链接）
        foreach (var nodeData in allNodesData)
        {
            // 直接添加所有节点到列表
            relatedNodes.Add(nodeData);
        }

        return relatedNodes;
    }




}
