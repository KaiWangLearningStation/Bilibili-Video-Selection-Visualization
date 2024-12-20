using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 10.0f;
    public float sensitivity = 5.0f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update()
    {
        // 只有在鼠标右键按住时才允许旋转视角
        if (Input.GetMouseButton(1))
        {
            yaw += sensitivity * Input.GetAxis("Mouse X");
            pitch -= sensitivity * Input.GetAxis("Mouse Y");

            // 限制俯仰角度
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            // 应用旋转
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // 键盘移动
        float translationX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float translationZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float translationY = 0.0f;

        if (Input.GetKey(KeyCode.Space))
        {
            translationY = speed * Time.deltaTime;  // 向上移动
        }
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            translationY = -speed * Time.deltaTime; // 向下移动
        }

        transform.Translate(translationX, translationY, translationZ);
    }
}
