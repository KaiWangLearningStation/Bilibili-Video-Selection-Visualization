using UnityEngine;

public class NodeBreathing : MonoBehaviour
{
    public float breathingSpeed = 1.0f;  // 控制呼吸的速度
    public float breathingScale = 0.1f;  // 控制呼吸的幅度
    private Vector3 initialScale;

    void Start()
    {
        // 记录初始缩放大小
        initialScale = transform.localScale;
    }

    void Update()
    {
        // 计算新的缩放比例
        float scale = 1.0f + Mathf.Sin(Time.time * breathingSpeed) * breathingScale;
        transform.localScale = initialScale * scale;
    }
}
