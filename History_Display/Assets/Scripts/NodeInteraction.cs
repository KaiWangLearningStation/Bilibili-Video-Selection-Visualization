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
        List<NodeData> relatedNodesData = GetRelatedNodes(this.gameObject.name);
        List<GameObject> relatedNodes = new List<GameObject>();

        // 获取GraphLoader实例中的fixedNodes列表
        List<string> fixedNodes = GraphLoader.Instance.fixedNodes;

        // 遍历所有相关节点数据
        foreach (var nodeData in relatedNodesData)
        {
            if (graphLoader.nodes.ContainsKey(nodeData.id))
            {
                var node = graphLoader.nodes[nodeData.id];

                // 排除已经激活的节点，且排除固定节点
                if (node.activeSelf && !fixedNodes.Contains(node.name))  // 检查节点名字是否在fixedNodes中
                {
                    relatedNodes.Add(node);
                    if (relatedNodes.Count >= count)
                        break;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"NodeInteraction: NodeData for '{nodeData.id}' not found in graphLoader.nodes.");
            }
        }

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
