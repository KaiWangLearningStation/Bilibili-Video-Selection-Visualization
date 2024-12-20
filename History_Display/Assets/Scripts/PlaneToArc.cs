using UnityEngine;

public class PlaneToHalfCylinder : MonoBehaviour
{
    public float curveStrength = 1f;  // 控制弯曲的强度（半径）
    public float curveHeight = 5f;    // 控制弯曲的高度
    public int segments = 10;         // 控制弯曲网格的分段数（更高分段数会有更平滑的效果）

    void Start()
    {
        // 获取 Plane 的网格
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // 计算每个顶点的新坐标
        for (int i = 0; i < vertices.Length; i++)
        {
            // 获取当前顶点的坐标
            float x = vertices[i].x;
            float z = vertices[i].z;

            // 计算当前顶点在圆柱面上的角度
            float angle = Mathf.Atan2(z, x); // 通过 arctan 计算出角度

            // 按照角度计算新的 x 和 z 坐标
            float radius = curveStrength;  // 半径（弯曲的强度）
            float newX = radius * Mathf.Sin(angle);  // 通过角度和半径计算 x 坐标
            float newZ = radius * Mathf.Cos(angle);  // 通过角度和半径计算 z 坐标

            // 计算 y 坐标（根据 Plane 的位置）
            float newY = vertices[i].y * curveHeight;  // 沿 Y 轴的变形，控制弯曲的高度

            // 更新顶点位置
            vertices[i] = new Vector3(newX, newY, newZ);
        }

        // 更新网格的顶点数据
        mesh.vertices = vertices;

        // 重新计算法线，确保物体正确照明
        mesh.RecalculateNormals();

        // 重新计算网格的边界，确保渲染正常
        mesh.RecalculateBounds();
    }
}
