using System;
using UnityEngine;

public class NodeFloating : MonoBehaviour
{
    public float floatingSpeed = 1.0f;  // 控制运动的速度
    public float floatingDistance = 0.1f;  // 控制运动的幅度

    private Vector3 initialPosition;
    private Vector3 randomOffset;  // 每个球体的随机偏移
    private float randomSpeed;     // 每个球体的随机速度因子

    void Start()
    {
        // 记录初始位置
        initialPosition = transform.position;

        // 为每个球体生成一个随机偏移
        randomOffset = new Vector3(
            UnityEngine.Random.Range(0f, 2f * Mathf.PI),
            UnityEngine.Random.Range(0f, 2f * Mathf.PI),
            UnityEngine.Random.Range(0f, 2f * Mathf.PI)
        );

        // 为每个球体生成一个随机的速度因子
        randomSpeed = UnityEngine.Random.Range(0.8f, 1.2f);
    }

    void Update()
    {
        // 计算新的位置
        Vector3 offset = new Vector3(
            Mathf.Sin(Time.time * floatingSpeed * randomSpeed + randomOffset.x) * floatingDistance,
            Mathf.Cos(Time.time * floatingSpeed * randomSpeed * 0.8f + randomOffset.y) * floatingDistance,
            Mathf.Sin(Time.time * floatingSpeed * randomSpeed * 1.2f + randomOffset.z) * floatingDistance
        );

        transform.position = initialPosition + offset;
    }
}
