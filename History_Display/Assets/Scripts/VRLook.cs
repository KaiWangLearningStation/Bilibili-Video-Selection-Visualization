using UnityEngine;
using UnityEngine.XR;

public class VRLook : MonoBehaviour
{
    public float rotationSpeed = 45.0f;  // 旋转速度
    public XRNode rightHandControllerNode = XRNode.RightHand;

    private Vector2 inputAxis;

    void Update()
    {
        // 获取右手柄的输入
        InputDevice rightHandDevice = InputDevices.GetDeviceAtXRNode(rightHandControllerNode);
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis))
        {
            RotatePlayer(inputAxis.x);  // 只传递左右方向的输入
        }
    }

    void RotatePlayer(float horizontalInput)
    {
        // 水平旋转 (左右)
        float horizontalRotation = horizontalInput * rotationSpeed * Time.deltaTime;

        // 应用左右旋转
        transform.Rotate(0, horizontalRotation, 0);
    }
}
