using System.Collections.Generic;
using System;

namespace SharedGraph
{
    [Serializable]
    public class NodeData
    {
        public string id;
        public int size;
        public int group;  // 确保这是一个单一的 int 类型
        public List<float> position;

        // 返回节点位置的简化方法
        public string GetPositionAsString()
        {
            if (position == null || position.Count < 3)
            {
                return "0,0,0"; // 默认值
            }
            return $"{position[0]},{position[1]},{position[2]}"; // 返回 "X,Y,Z" 格式
        }
    }

    [Serializable]
    public class LinkData
    {
        public string source;
        public string target;
    }

    [Serializable]
    public class GraphData
    {
        public List<NodeData> nodes;
        public List<LinkData> links;
    }
}
