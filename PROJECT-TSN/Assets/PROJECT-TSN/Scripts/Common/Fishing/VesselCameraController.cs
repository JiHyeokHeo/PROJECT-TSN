using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// Camera controller for rendering fishing view into LeftFrame via RenderTexture.
    /// </summary>
    public class VesselCameraController : MonoBehaviour
    {
        public static VesselCameraController Singleton { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private VesselController vessel;

        [Header("Follow")]
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private bool autoFollow = true;

        [Header("View Angle")]
        [SerializeField] private float pitchAngle = 45f;
        [SerializeField] private bool lockPitchAngle = true;

        [Header("Offset")]
        [SerializeField] private float height = 12f;
        [SerializeField] private float depth = 12f;

        [Header("RenderTexture")]
        [SerializeField] private RectTransform viewportFrame;
        [SerializeField] private RawImage displayImage;

        private Camera cameraComponent;
        private RenderTexture renderTexture;

        private Vector3 shakeOffset;
        private Coroutine shakeCoroutine;

        public bool AutoFollow
        {
            get => autoFollow;
            set => autoFollow = value;
        }

        private void Awake()
        {
            Singleton = this;
            cameraComponent = GetComponent<Camera>();

            if (lockPitchAngle)
            {
                transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
            }

            PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
            if (pixelPerfectCamera != null && cameraComponent != null && !cameraComponent.orthographic)
            {
                Debug.LogWarning(
                    "[VesselCameraController] PixelPerfectCamera is on a perspective camera. " +
                    "Keep Pixel Perfect on Main Camera(2D output) and use VesselCamera without it.",
                    this);
            }
        }

        private void Start()
        {
            SetupRenderTexture();
        }

        private void LateUpdate()
        {
            Vector3 basePosition = transform.position - shakeOffset;

            if (autoFollow && vessel != null)
            {
                Vector3 desiredPosition = vessel.transform.position + new Vector3(0f, height, -depth);
                basePosition = Vector3.Lerp(basePosition, desiredPosition, followSpeed * Time.deltaTime);
            }

            transform.position = basePosition + shakeOffset;

            if (lockPitchAngle)
            {
                transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
            }
        }

        private void OnDestroy()
        {
            if (Singleton == this)
            {
                Singleton = null;
            }

            ReleaseRenderTexture();
        }

        [ContextMenu("Enable Auto Follow")]
        private void EnableAutoFollow()
        {
            autoFollow = true;
            SnapToVessel();
        }

        [ContextMenu("Disable Auto Follow (Manual Framing)")]
        private void DisableAutoFollow()
        {
            autoFollow = false;
        }

        [ContextMenu("Snap To Vessel")]
        private void SnapToVessel()
        {
            if (vessel == null)
            {
                return;
            }

            transform.position = vessel.transform.position + new Vector3(0f, height, -depth);

            if (lockPitchAngle)
            {
                transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
            }
        }

        public void ShakeCamera(float duration, float magnitude = 0.3f)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeCameraRoutine(duration, magnitude));
        }

        private IEnumerator ShakeCameraRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                shakeOffset = Random.insideUnitSphere * magnitude;
                elapsed += Time.deltaTime;
                yield return null;
            }

            shakeOffset = Vector3.zero;
            shakeCoroutine = null;
        }

        private void SetupRenderTexture()
        {
            if (cameraComponent == null)
            {
                Debug.LogError("[VesselCameraController] Camera component not found.", this);
                return;
            }

            int rtWidth = 1280;
            int rtHeight = 720;

            if (viewportFrame != null)
            {
                Rect frameRect = viewportFrame.rect;
                int width = Mathf.RoundToInt(Mathf.Abs(frameRect.width));
                int heightValue = Mathf.RoundToInt(Mathf.Abs(frameRect.height));

                if (width > 0 && heightValue > 0)
                {
                    rtWidth = width;
                    rtHeight = heightValue;
                }
            }

            ReleaseRenderTexture();

            renderTexture = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.ARGB32)
            {
                name = "VesselCameraRT",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            renderTexture.Create();

            cameraComponent.targetTexture = renderTexture;

            if (displayImage != null)
            {
                displayImage.texture = renderTexture;
            }
            else
            {
                Debug.LogWarning(
                    "[VesselCameraController] displayImage(RawImage) is not connected. " +
                    "Please assign LeftFrame > VesselView RawImage in Inspector.",
                    this);
            }

            Debug.Log($"[VesselCameraController] RenderTexture created: {rtWidth}x{rtHeight}", this);
        }

        private void ReleaseRenderTexture()
        {
            if (renderTexture == null)
            {
                return;
            }

            if (cameraComponent != null && cameraComponent.targetTexture == renderTexture)
            {
                cameraComponent.targetTexture = null;
            }

            if (displayImage != null && displayImage.texture == renderTexture)
            {
                displayImage.texture = null;
            }

            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            SetupRenderTexture();
        }
#endif
    }
}
