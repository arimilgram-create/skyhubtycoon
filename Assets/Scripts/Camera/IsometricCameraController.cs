// Orthographic 2/3 isometric camera controller using WebGL-safe keyboard and mouse input.
using UnityEngine;

namespace SkyHubTycoon.CameraControls
{
    [RequireComponent(typeof(Camera))]
    public class IsometricCameraController : MonoBehaviour
    {
        public float panSpeed = 12f;
        public float zoomSpeed = 4f;
        public float minZoom = 8f;
        public float maxZoom = 28f;
        public float pitch = 55f;
        public float yaw = 45f;

        private Camera attachedCamera;

        private void Awake()
        {
            attachedCamera = GetComponent<Camera>();
            attachedCamera.orthographic = true;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void Update()
        {
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;
            Vector3 movement = (right * Input.GetAxisRaw("Horizontal") + forward * Input.GetAxisRaw("Vertical")) * panSpeed * Time.deltaTime;
            transform.position += movement;

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                attachedCamera.orthographicSize = Mathf.Clamp(attachedCamera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }

            if (Input.GetKeyDown(KeyCode.Q)) Rotate90(-1);
            if (Input.GetKeyDown(KeyCode.E)) Rotate90(1);
        }

        public void Rotate90(int direction)
        {
            yaw += 90f * direction;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
