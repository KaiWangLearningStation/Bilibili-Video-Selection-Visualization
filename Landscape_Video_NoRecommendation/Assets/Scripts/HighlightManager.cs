using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class HighlightManager : MonoBehaviour
{
    // Singleton实例
    public static HighlightManager Instance { get; private set; }

    // 跟踪当前和之前激活的节点和连接
    private List<GameObject> lastActivatedNodesAndLinks = new List<GameObject>();
    private List<GameObject> currentActivatedNodesAndLinks = new List<GameObject>();

    // 材质
    public Material defaultNodeMaterial;
    public Material defaultLinkMaterial;
    public Material highlightNodeMaterial;
    public Material highlightLinkMaterial;

    // 缓存原始材质的字典
    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();

    private void Awake()
    {
        // 实现Singleton模式
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            UnityEngine.Debug.LogWarning("HighlightManager: Duplicate instance destroyed.");
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        UnityEngine.Debug.Log("HighlightManager: Singleton instance initialized.");
    }

    private void Start()
    {
        // 在 Start() 中调用初始化方法，确保 GraphLoader 已初始化
        //LogInitialActiveNodesAndLinks();
        CacheOriginalMaterials();
    }
    private void CacheOriginalMaterials()
    {
        // 缓存所有节点的原始材质
        foreach (var node in GraphLoader.Instance.nodes.Values)
        {
            Renderer renderer = node.GetComponent<Renderer>();
            if (renderer != null && !originalMaterials.ContainsKey(node))
            {
                originalMaterials[node] = renderer.material;  // 缓存原始材质
            }
        }
    }
    /// <summary>
    /// 初始化时记录所有已激活的节点和连接的材质。
    /// </summary>
    private void LogInitialActiveNodesAndLinks()
    {
        UnityEngine.Debug.Log("HighlightManager: Logging initial active nodes and links.");

        if (GraphLoader.Instance == null)
        {
            UnityEngine.Debug.LogError("HighlightManager: GraphLoader.Instance is null. Ensure GraphLoader is properly initialized.");
            return;
        }

        // 处理初始激活的节点
        foreach (var node in GraphLoader.Instance.nodes.Values)
        {
            if (node.activeSelf)
            {
                Renderer renderer = node.GetComponent<Renderer>();
                if (renderer != null)
                {
                    UnityEngine.Debug.Log($"HighlightManager: Initial active node '{node.name}' has material '{renderer.material.name}'.");

                    // 缓存原始材质
                    if (!originalMaterials.ContainsKey(node))
                    {
                        originalMaterials[node] = renderer.material;
                        UnityEngine.Debug.Log($"HighlightManager: Cached original material for node '{node.name}': '{renderer.material.name}'.");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"HighlightManager: Initial active node '{node.name}' does not have a Renderer component.");
                }
            }
        }

        // 处理初始激活的连接
        foreach (var link in GraphLoader.Instance.links.Values)
        {
            if (link.activeSelf)
            {
                Renderer renderer = link.GetComponent<Renderer>();
                if (renderer != null)
                {
                    UnityEngine.Debug.Log($"HighlightManager: Initial active link '{link.name}' has material '{renderer.material.name}'.");

                    // 缓存原始材质
                    if (!originalMaterials.ContainsKey(link))
                    {
                        originalMaterials[link] = renderer.material;
                        UnityEngine.Debug.Log($"HighlightManager: Cached original material for link '{link.name}': '{renderer.material.name}'.");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"HighlightManager: Initial active link '{link.name}' does not have a Renderer component.");
                }
            }
        }

        UnityEngine.Debug.Log("HighlightManager: Initial active nodes and links logging completed.");
    }

    /// <summary>
    /// 高亮选中的节点及其相关节点和连接。
    /// </summary>
    /// <param name="selectedNode">被选中的节点</param>
    public void HighlightNode(GameObject selectedNode)
    {
        UnityEngine.Debug.Log($"HighlightManager: Highlighting node '{selectedNode.name}'.");

        // 恢复之前激活的节点和连接
        //RestoreAllNodesAndLinksToDefault();

        // 只有在材质没有缓存过的情况下才缓存
        if (!originalMaterials.ContainsKey(selectedNode))
        {
            Renderer renderer = selectedNode.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterials[selectedNode] = renderer.material;
            }
        }
        // 不恢复之前节点的材质，直接高亮当前节点
        ApplyHighlightEffect(selectedNode, highlightNodeMaterial);

        // 将当前节点添加到激活列表
        if (!currentActivatedNodesAndLinks.Contains(selectedNode))
        {
            currentActivatedNodesAndLinks.Add(selectedNode);
        }

        //// 清空当前激活列表
        //currentActivatedNodesAndLinks.Clear();
        //UnityEngine.Debug.Log("HighlightManager: Cleared currentActivatedNodesAndLinks list.");

        //// 高亮当前选中的节点
        //currentActivatedNodesAndLinks.Add(selectedNode);
        //ApplyHighlightEffect(selectedNode, highlightNodeMaterial);
        //UnityEngine.Debug.Log($"HighlightManager: Highlighted selected node '{selectedNode.name}'.");

        //// 获取相关节点和连接
        //NodeInteraction nodeInteraction = selectedNode.GetComponent<NodeInteraction>();
        //if (nodeInteraction != null)
        //{
        //    List<GameObject> relatedNodes = nodeInteraction.GetTopRelatedNodes(10);
        //    UnityEngine.Debug.Log($"HighlightManager: Found {relatedNodes.Count} related nodes for '{selectedNode.name}'.");
        //    // 只显示选中节点和其相关节点之间的连接
        //    foreach (var relatedNode in relatedNodes)
        //    {
        //        // 确保相关节点已经激活
        //        if (!relatedNode.activeSelf)
        //        {
        //            relatedNode.SetActive(true);
        //            UnityEngine.Debug.Log($"HighlightManager: Set related node '{relatedNode.name}' active.");
        //        }

        //        // 高亮相关节点
        //        ApplyHighlightEffect(relatedNode, highlightNodeMaterial);
        //        currentActivatedNodesAndLinks.Add(relatedNode);
        //        UnityEngine.Debug.Log($"HighlightManager: Highlighted related node '{relatedNode.name}'.");

        //        // 激活并高亮连接
        //        ActivateLink(selectedNode.name, relatedNode.name);
        //    }
        //}
        //else
        //{
        //    UnityEngine.Debug.LogWarning($"HighlightManager: NodeInteraction component not found on '{selectedNode.name}'.");
        //}

        //// 更新lastActivatedNodesAndLinks
        //lastActivatedNodesAndLinks = new List<GameObject>(currentActivatedNodesAndLinks);
        //UnityEngine.Debug.Log($"HighlightManager: Updated lastActivatedNodesAndLinks with {lastActivatedNodesAndLinks.Count} objects.");
    }



    /// <summary>
    /// 恢复所有之前激活的节点和连接到默认材质。
    /// </summary>
    private void RestoreAllNodesAndLinksToDefault()
    {
        UnityEngine.Debug.Log("HighlightManager: Restoring all previously activated nodes and links to default materials.");

        // 先隐藏所有的link
        foreach (var link in GraphLoader.Instance.links.Values)
        {
            if (link != null)
            {
                link.SetActive(false);  // 隐藏所有的 link
            }
        }
        foreach (var obj in lastActivatedNodesAndLinks)
        {
            UnityEngine.Debug.Log($"HighlightManager: Restoring object '{obj.name}'.");
            RestoreObjectToDefault(obj);
        }

        // 清空列表以准备下次激活
        lastActivatedNodesAndLinks.Clear();
        currentActivatedNodesAndLinks.Clear();
        UnityEngine.Debug.Log("HighlightManager: Cleared lastActivatedNodesAndLinks and currentActivatedNodesAndLinks lists.");
    }

    /// <summary>
    /// 恢复单个对象到默认材质。
    /// </summary>
    /// <param name="obj">要恢复的对象</param>
    private void RestoreObjectToDefault(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            UnityEngine.Debug.Log($"HighlightManager: Before restoring, '{obj.name}' has material '{renderer.material.name}'.");

            if (originalMaterials.ContainsKey(obj))
            {
                renderer.material = originalMaterials[obj];
                UnityEngine.Debug.Log($"HighlightManager: Restored '{obj.name}' to original material '{originalMaterials[obj].name}'.");
                // 移除缓存的原始材质
                originalMaterials.Remove(obj);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"HighlightManager: Original material for '{obj.name}' not found. Using default materials.");

                if (obj.CompareTag("Node"))
                {
                    renderer.material = defaultNodeMaterial;
                    UnityEngine.Debug.Log($"HighlightManager: Restored '{obj.name}' to defaultNodeMaterial '{defaultNodeMaterial.name}'.");
                }
                else if (obj.CompareTag("Link"))
                {
                    renderer.material = defaultLinkMaterial;
                    UnityEngine.Debug.Log($"HighlightManager: Restored '{obj.name}' to defaultLinkMaterial '{defaultLinkMaterial.name}'.");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"HighlightManager: Object '{obj.name}' does not have 'Node' or 'Link' tag.");
                }
            }

            UnityEngine.Debug.Log($"HighlightManager: After restoring, '{obj.name}' has material '{renderer.material.name}'.");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"HighlightManager: Object '{obj.name}' does not have a Renderer component.");
        }
    }

    /// <summary>
    /// 激活并高亮连接。
    /// </summary>
    /// <param name="sourceNodeName">源节点名称</param>
    /// <param name="targetNodeName">目标节点名称</param>
    private void ActivateLink(string sourceNodeName, string targetNodeName)
    {
        string linkName1 = $"{sourceNodeName}_{targetNodeName}";
        string linkName2 = $"{targetNodeName}_{sourceNodeName}";
        GameObject linkObject = null;

        // 尝试按正向和反向名称查找链接
        if (GraphLoader.Instance.links.TryGetValue(linkName1, out linkObject))
        {
            UnityEngine.Debug.Log($"HighlightManager: Link found: '{linkName1}'.");
        }
        else if (GraphLoader.Instance.links.TryGetValue(linkName2, out linkObject))
        {
            UnityEngine.Debug.Log($"HighlightManager: Link found with reversed name: '{linkName2}'.");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"HighlightManager: Link between '{sourceNodeName}' and '{targetNodeName}' not found.");
            return;
        }

        if (linkObject != null)
        {
            if (!linkObject.activeSelf)
            {
                linkObject.SetActive(true);
                UnityEngine.Debug.Log($"HighlightManager: Set link '{linkObject.name}' active.");
                ApplyHighlightEffect(linkObject, highlightLinkMaterial);
                currentActivatedNodesAndLinks.Add(linkObject);
                UnityEngine.Debug.Log($"HighlightManager: Activated and highlighted link '{linkObject.name}'.");
            }
            else
            {
                UnityEngine.Debug.Log($"HighlightManager: Link '{linkObject.name}' is already active.");
            }
        }
    }

    /// <summary>
    /// 应用高亮材质到对象。
    /// </summary>
    /// <param name="obj">目标对象</param>
    /// <param name="highlightMaterial">高亮材质</param>
    private void ApplyHighlightEffect(GameObject target, Material highlightMaterial)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material originalMaterial = renderer.material;
            originalMaterials[target] = originalMaterial;  // 缓存原始材质
            renderer.material = highlightMaterial;         // 设置高亮材质
        }
        else
        {
            UnityEngine.Debug.LogWarning($"No Renderer component found on '{target.name}'.");
        }
    }

}
