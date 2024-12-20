using System;
using System.Collections.Generic;
using UnityEngine;
using SharedGraph;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.UI;
using TMPro;  // 确保引入 TextMeshPro 命名空间

[Serializable]
public class HighlightNodeData
{
    public string id;
    public int size;
}
[Serializable]
public class HighlightNodeDataList
{
    public List<HighlightNodeData> nodes;
}


public class GraphLoader : MonoBehaviour
{
    public static GraphLoader Instance { get; private set; }

    public Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> links = new Dictionary<string, GameObject>();
    public GraphData graph; // 假设 GraphData 包含节点数据

    private GameObject nodesParent;  // 节点的父对象
    private GameObject linksParent;  // 连线的父对象
    private List<HighlightNodeData> highlightNodes;  // 用于存储高亮节点的数据
    private int currentNodeIndex = 0;  // 当前高亮节点的索引
    private Dictionary<string, Color> originalColors;  // 用于存储节点的原始颜色
    private bool isForwardButtonHeld = false;  // 判断手柄 X 键是否被按住
    private bool isBackwardButtonHeld = false; // 判断手柄 Y 键是否被按住

    public GameObject nodePrefab;  // 在Unity Inspector中分配这个Prefab
    public GameObject linePrefab;  // 在Unity Inspector中分配这个Line Prefab
    public Color fixedNodeColor = Color.red;  // 你可以设置为你想要的颜色
    public TextAsset jsonData;  // Assign this in the Unity Editor's Inspector
    public float scale = 10.0f;  // Scale factor to increase node position magnitudes
    public float sizeScale = 1.0f;  // Additional scale factor for node sizes
    public TextAsset highlightJsonData;  // Assign the new JSON file in the Unity Editor
    public Color highlightColor = Color.yellow;  // The color to highlight the nodes

    // 将 fixedNodes 移到类级别
    public List<string> fixedNodes = new List<string> {
        "动画", "动物圈", "搞笑", "鬼畜", "科技",
        "美食", "汽车", "生活", "时尚", "舞蹈",
        "音乐", "影视", "游戏", "娱乐", "运动",
        "知识", "资讯"
    };


    void Awake()
    {
        // Implement Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            UnityEngine.Debug.LogWarning("GraphLoader: Duplicate instance destroyed.");
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        UnityEngine.Debug.Log("GraphLoader: Singleton instance initialized.");
    }

    void Start()
    {
        // 初始化字典
        originalColors = new Dictionary<string, Color>();

        // 创建节点和连线的父对象
        nodesParent = new GameObject("NodesParent");
        linksParent = new GameObject("LinksParent");
        graph = JsonUtility.FromJson<GraphData>(jsonData.text);
        CreateGraph(graph);

        // 记录每个节点的原始颜色
        foreach (Transform node in nodesParent.transform)
        {
            Renderer renderer = node.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalColors[node.name] = renderer.material.color;
            }
        }

        // 解析高亮节点的JSON文件
        HighlightNodeDataList highlightNodeDataList = JsonUtility.FromJson<HighlightNodeDataList>(highlightJsonData.text);
        highlightNodes = highlightNodeDataList.nodes;

        // 初始化节点的高亮状态
        UpdateHighlightedNodes();
    }

    void Update()
    {
        // 检测手柄的 X 键 (前进) 和 Y 键 (后退)
        if (OVRInput.GetDown(OVRInput.Button.One))  // X 键按下
        {
            isForwardButtonHeld = true;
            InvokeRepeating("MoveHighlightForward", 0f, 0.5f);  // 开始重复调用
        }
        else if (OVRInput.GetUp(OVRInput.Button.One))  // X 键松开
        {
            isForwardButtonHeld = false;
            CancelInvoke("MoveHighlightForward");  // 停止重复调用
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))  // Y 键按下
        {
            isBackwardButtonHeld = true;
            InvokeRepeating("MoveHighlightBackward", 0f, 0.5f);  // 开始重复调用
        }
        else if (OVRInput.GetUp(OVRInput.Button.Two))  // Y 键松开
        {
            isBackwardButtonHeld = false;
            CancelInvoke("MoveHighlightBackward");  // 停止重复调用
        }


        // 让所有文本标签始终面向摄像机，更新每一帧
        if (mainCamera != null)
        {
            foreach (var label in nodeLabels.Values)
            {
                //方法 1：直接使用 LookAt
                label.transform.LookAt(mainCamera.transform);
                label.transform.Rotate(0f, 180f, 0f);

                //// 方法 2：只旋转 Y 轴（如果你希望文本保持竖直）
                //Vector3 direction = mainCamera.transform.position - label.transform.position;
                //direction.y = 0;  // 只在水平方向旋转，保持竖直
                //label.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    void CreateGraph(GraphData graph)
    {
        foreach (NodeData node in graph.nodes)
        {
            CreateNode(node);
        }
        foreach (LinkData link in graph.links)
        {
            CreateLink(link);
        }
    }
    public TMP_FontAsset customFont;  // 通过 Inspector 分配你的 Font Asset
    public Camera mainCamera;
    private Dictionary<GameObject, GameObject> nodeLabels = new Dictionary<GameObject, GameObject>();

    void CreateLabelForNode(GameObject nodeObject, string nodeName)
    {
        // 创建一个Canvas，如果没有的话
        Canvas canvas = nodeObject.GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            // 创建一个新的Canvas并设置为世界空间
            canvas = new GameObject("NodeLabelCanvas").AddComponent<Canvas>();
            canvas.transform.SetParent(nodeObject.transform);
            canvas.renderMode = RenderMode.WorldSpace;  // 设置为世界空间
            canvas.worldCamera = Camera.main;  // 设置主相机
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 100); // 设置Canvas大小
        }

        // 创建一个TextMeshPro对象来显示节点名称
        GameObject textObject = new GameObject("NodeLabel");
        textObject.transform.SetParent(canvas.transform);

        // 使用 TextMeshPro 来避免与其他 Text 类型冲突
        TextMeshPro textComponent = textObject.AddComponent<TextMeshPro>();
        textComponent.text = nodeName;
        textComponent.fontSize = 200;
        textComponent.color = Color.black; // 设置标签颜色
        //textComponent.color = new Color(203f / 255f, 43f / 255f, 43f / 255f);  // 设置标签颜色为 #CB2B2B
        textComponent.alignment = TextAlignmentOptions.Center; // 设置文本对齐方式

        // 设置字体
        textComponent.font = customFont;  // 这里指定了自定义字体

        // Adjust RectTransform to properly position the label
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);  // 设置文本大小
        rectTransform.anchoredPosition = new Vector2(0, 1.5f);  // 将文本放置在节点上方

        // 将标签的世界空间位置设置为节点的位置上方
        rectTransform.position = nodeObject.transform.position + new Vector3(0, 2f, 0);  // 调整标签位置，使其位于节点上方

        // 将标签保存到字典中
        nodeLabels[nodeObject] = textObject;
    }


    void CreateNode(NodeData node)
    {
        GameObject nodeObject = Instantiate(nodePrefab, nodesParent.transform);  // 设置Parent为nodesParent

        Vector3 position = new Vector3(node.position[0], node.position[1], node.position[2]) * scale;
        nodeObject.transform.position = position;

        // Apply logarithmic scaling to node size
        float adjustedSize = Mathf.Log10(node.size + 1) * sizeScale;  // +1 to avoid log(0)
        nodeObject.transform.localScale = Vector3.one * adjustedSize;

        nodeObject.name = node.id;

        // 仅为fixedNodes中的节点创建名称标签
        if (fixedNodes.Contains(node.id))
        {
            // 设置颜色
            nodeObject.GetComponent<Renderer>().material.color = fixedNodeColor;

            // 为固定节点添加显示名称的标签
            CreateLabelForNode(nodeObject, node.id);
        }

        if (fixedNodes.Contains(node.id))
        {
            // 设置颜色
            nodeObject.GetComponent<Renderer>().material.color = fixedNodeColor;
        }
    }

    void CreateLink(LinkData link)
    {
        GameObject source = GameObject.Find(link.source);
        GameObject target = GameObject.Find(link.target);
        if (source != null && target != null)
        {
            GameObject lineObject = Instantiate(linePrefab, linksParent.transform);  // 设置Parent为linksParent
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

            lineRenderer.SetPositions(new Vector3[] { source.transform.position, target.transform.position });

            // 默认隐藏整个 GameObject
            lineObject.SetActive(false);  // 禁用整个 GameObject
        }
    }


    // Forward key functionality (X Key)
    void MoveHighlightForward()
    {
        if (currentNodeIndex < highlightNodes.Count - 1)
        {
            currentNodeIndex++;
            UpdateHighlightedNodes();
        }
    }

    // Backward key functionality (Y Key)
    void MoveHighlightBackward()
    {
        if (currentNodeIndex > 0)
        {
            GameObject nodeObject = GameObject.Find(highlightNodes[currentNodeIndex].id);
            if (nodeObject != null)
            {
                // 恢复到原始颜色
                Color originalColor = originalColors[highlightNodes[currentNodeIndex].id];
                nodeObject.GetComponent<Renderer>().material.color = originalColor;
            }
            currentNodeIndex--;
            UpdateHighlightedNodes();
        }
    }

    // Update the highlighted nodes
    void UpdateHighlightedNodes()
    {
        // Reset all nodes to original colors
        foreach (var node in nodes)
        {
            if (node.Value != null)
            {
                node.Value.GetComponent<Renderer>().material.color = originalColors[node.Key];
            }
        }

        // Apply highlight to the nodes up to the current index
        for (int i = 0; i <= currentNodeIndex; i++)
        {
            GameObject nodeObject = GameObject.Find(highlightNodes[i].id);
            if (nodeObject != null)
            {
                // 将节点设置为高亮颜色
                nodeObject.GetComponent<Renderer>().material.color = highlightColor;
            }
        }
    }
}
