using System;
using System.Collections.Generic;
using UnityEngine;
using SharedGraph;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.UI;
using TMPro;  // 确保引入 TextMeshPro 命名空间

public class GraphLoader : MonoBehaviour
{
    public static GraphLoader Instance { get; private set; }

    public Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> links = new Dictionary<string, GameObject>();
    public GraphData graph; // 假设 GraphData 包含节点数据

    private void Awake()
    {
        // 实现Singleton模式
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

    public GameObject nodePrefab;  // 在Unity Inspector中分配这个Prefab
    public GameObject linePrefab;  // 在Unity Inspector中分配这个Line Prefab
    public Color fixedNodeColor = Color.red;  // 你可以设置为你想要的颜色
    public TextAsset jsonData;  // Assign this in the Unity Editor's Inspector
    public float scale = 10.0f;  // Scale factor to increase node position magnitudes
    public float sizeScale = 1.0f;  // Additional scale factor for node sizes

    public List<string> fixedNodes = new List<string> { "动画", "动物圈", "搞笑", "鬼畜", "科技",
                                                 "美食", "汽车", "生活", "时尚", "舞蹈",
                                                 "音乐", "影视", "游戏", "娱乐", "运动",
                                                 "知识", "资讯" };

    private GameObject nodesParent;  // 节点的父对象
    private GameObject linksParent;  // 连线的父对象

    // 用于存储节点和连线的引用
    //public Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
    //public Dictionary<string, GameObject> links = new Dictionary<string, GameObject>();

    //public GraphData graph;
    void Start()
    {
        // 创建节点和连线的父对象
        nodesParent = new GameObject("NodesParent");
        linksParent = new GameObject("LinksParent");
        graph = JsonUtility.FromJson<GraphData>(jsonData.text);
        CreateGraph(graph);
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
        HideNonFixedNodes(graph.nodes);
        HideLinksForInactiveNodes(graph.links);
    }

    /*    void CreateNode(NodeData node)
        {
            GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 position = new Vector3(node.position[0], node.position[1], node.position[2]) * scale;
            nodeObject.transform.position = position;

            // Apply logarithmic scaling to node size
            float adjustedSize = Mathf.Log10(node.size + 1) * sizeScale;  // +1 to avoid log(0)
            nodeObject.transform.localScale = Vector3.one * adjustedSize;

            nodeObject.name = node.id;
        }*/
    void CreateNode(NodeData node)
    {
        // 使用自定义Prefab实例化节点
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

        nodes[node.id] = nodeObject;
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

    void Update()
    {
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

    void CreateLink(LinkData link)
    {
        GameObject source = GameObject.Find(link.source);
        GameObject target = GameObject.Find(link.target);
        if (source != null && target != null)
        {
            /*            LineRenderer line = new GameObject("Link").AddComponent<LineRenderer>();
                        line.SetPositions(new Vector3[] { source.transform.position, target.transform.position });
                        line.material = new Material(Shader.Find("Sprites/Default"));
                        line.startWidth = line.endWidth = 0.05f;*/
            // 使用Prefab实例化连线

            /*GameObject lineObject = Instantiate(linePrefab);*/
            GameObject lineObject = Instantiate(linePrefab, linksParent.transform);  // 设置Parent为linksParent
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

            lineRenderer.SetPositions(new Vector3[] { source.transform.position, target.transform.position });

            string linkName = $"{link.source}_{link.target}";
            lineObject.name = linkName;
            links[linkName] = lineObject;
        }
    }
    void HideNonFixedNodes(List<NodeData> nodes)
    {

        foreach (NodeData node in nodes)
        {
            if (!fixedNodes.Contains(node.id))
            {
                GameObject nodeObject = GameObject.Find(node.id);
                nodeObject.SetActive(true);

            }
        }
    }
    void HideLinksForInactiveNodes(List<LinkData> links)
    {
        foreach (LinkData link in links)
        {
            GameObject linkObject = GameObject.Find($"{link.source}_{link.target}");
            if (linkObject != null)
            {
                linkObject.SetActive(false);
            }
        }
    }



}
