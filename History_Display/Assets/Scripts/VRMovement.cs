using UnityEngine;
using UnityEngine.XR;

public class VRMovement : MonoBehaviour
{
    public float speed = 3.0f;  // 移动速度
    public float verticalSpeed = 2.0f;  // 垂直移动速度
    public XRNode leftHandControllerNode = XRNode.LeftHand;

    private Vector2 inputAxis;

    void Update()
    {
        // 获取左手柄的输入
        InputDevice leftHandDevice = InputDevices.GetDeviceAtXRNode(leftHandControllerNode);

        // 检查左摇杆的输入
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis))
        {
            MovePlayer(inputAxis.y);  // 只使用上下输入
        }

        // 检查 Y 键输入
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isYPressed) && isYPressed)
        {
            MoveVertically(verticalSpeed);  // 向上移动
        }

        // 检查 X 键输入
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool isXPressed) && isXPressed)
        {
            MoveVertically(-verticalSpeed);  // 向下移动
        }
    }

    void MovePlayer(float verticalInput)
    {
        // 获取摄像机的前方向
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;

        // 只在水平面上移动
        forward.y = 0;
        forward.Normalize();

        // 计算移动方向
        Vector3 moveDirection = forward * verticalInput;

        // 应用移动
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    void MoveVertically(float verticalSpeed)
    {
        // 应用垂直移动
        transform.Translate(0, verticalSpeed * Time.deltaTime, 0, Space.World);
    }
}
