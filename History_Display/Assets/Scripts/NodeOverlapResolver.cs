using UnityEngine;

public class NodeOverlapResolver : MonoBehaviour
{
    public float repulsionForce = 10f;  // 排斥力的大小
    public float repulsionRange = 1.5f; // 排斥力的作用范围

    void Start()
    {
        // 获取所有节点
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

        // 添加Sphere Collider和Rigidbody
        foreach (GameObject node in nodes)
        {
            SphereCollider collider = node.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.5f;  // 根据节点大小调整碰撞体的半径

            Rigidbody rb = node.AddComponent<Rigidbody>();
            rb.isKinematic = true;  // 使物体不受物理引擎控制，仅用于检测碰撞
        }
    }

    void FixedUpdate()
    {
        // 对所有节点施加排斥力
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach (GameObject nodeA in nodes)
        {
            foreach (GameObject nodeB in nodes)
            {
                if (nodeA != nodeB)
                {
                    Vector3 direction = nodeA.transform.position - nodeB.transform.position;
                    float distance = direction.magnitude;
                    if (distance < repulsionRange)
                    {
                        // 根据距离施加排斥力
                        float repulsionStrength = repulsionForce / (distance * distance);
                        nodeA.transform.position += direction.normalized * repulsionStrength * Time.deltaTime;
                    }
                }
            }
        }
    }
}
