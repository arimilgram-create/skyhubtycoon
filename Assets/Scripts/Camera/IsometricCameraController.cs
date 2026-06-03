// Orthographic 2/3 isometric camera controller with smooth panning, zooming, and 90-degree rotation.
using UnityEngine;

namespace SkyHubTycoon.CameraControls
{
    [RequireComponent(typeof(Camera))]
    public class IsometricCameraController : MonoBehaviour
    {
        public float panSpeed = 14f;
        public float panSmoothing = 12f;
        public float zoomSpeed = 5f;
        public float zoomSmoothing = 14f;
        public float rotationSmoothing = 10f;
        public float minZoom = 7f;
        public float maxZoom = 26f;
        public float pitch = 55f;
        public float yaw = 45f;

        private Camera attachedCamera;
        private Vector3 targetPosition;
        private float targetZoom;
        private float targetYaw;

        private void Awake()
        {
            attachedCamera = GetComponent<Camera>();
            attachedCamera.orthographic = true;
            targetPosition = transform.position;
            targetZoom = attachedCamera.orthographicSize;
            targetYaw = yaw;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void Update()
        {
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;
            Vector3 input = right * Input.GetAxisRaw("Horizontal") + forward * Input.GetAxisRaw("Vertical");
            if (input.sqrMagnitude > 1f) input.Normalize();
            targetPosition += input * panSpeed * Time.unscaledDeltaTime;

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed, minZoom, maxZoom);
            }

            if (Input.GetKeyDown(KeyCode.Q)) Rotate90(-1);
            if (Input.GetKeyDown(KeyCode.E)) Rotate90(1);

            float panBlend = 1f - Mathf.Exp(-panSmoothing * Time.unscaledDeltaTime);
            float zoomBlend = 1f - Mathf.Exp(-zoomSmoothing * Time.unscaledDeltaTime);
            float rotateBlend = 1f - Mathf.Exp(-rotationSmoothing * Time.unscaledDeltaTime);

            transform.position = Vector3.Lerp(transform.position, targetPosition, panBlend);
            attachedCamera.orthographicSize = Mathf.Lerp(attachedCamera.orthographicSize, targetZoom, zoomBlend);
            yaw = Mathf.LerpAngle(yaw, targetYaw, rotateBlend);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        public void Rotate90(int direction)
        {
            targetYaw += 90f * direction;
        }
    }
}
