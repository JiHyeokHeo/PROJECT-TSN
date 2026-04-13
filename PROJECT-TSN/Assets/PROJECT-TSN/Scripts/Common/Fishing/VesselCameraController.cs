using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 관측선을 추적하는 쿼터뷰 카메라 컨트롤러.
    /// 45° 기울어진 시점으로 VesselController를 부드럽게 따라갑니다.
    ///
    /// RenderTexture 방식:
    ///   - Start()에서 LeftFrame 크기 기준으로 RenderTexture를 런타임 생성합니다.
    ///   - Camera.targetTexture에 RenderTexture를 할당하여 Main Display 대신 텍스처에 렌더링합니다.
    ///   - displayImage(RawImage)에 같은 RenderTexture를 할당하여 LeftFrame 안에 표시합니다.
    ///
    /// 세팅 방법:
    ///   - 이 컴포넌트를 VesselCamera GameObject에 부착합니다.
    ///   - viewportFrame : Canvas > MainLayOutController > LeftFrame 의 RectTransform을 연결합니다.
    ///   - displayImage  : LeftFrame > VesselView 의 RawImage를 연결합니다.
    /// </summary>
    public class VesselCameraController : MonoBehaviour
    {
        // ── 싱글톤 ───────────────────────────────────────────────────
        public static VesselCameraController Singleton { get; private set; }

        // ── 설정 ─────────────────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private VesselController vessel;

        [Header("Follow")]
        [Tooltip("카메라 추적 보간 속도 (높을수록 빠르게 따라감)")]
        [SerializeField] private float followSpeed = 8f;

        [Header("View Angle")]
        [Tooltip("X축 회전 각도 — 45° 가 쿼터뷰")]
        [SerializeField] private float pitchAngle = 45f;

        [Header("Offset")]
        [Tooltip("관측선 위로 카메라가 올라가는 높이")]
        [SerializeField] private float height = 12f;

        [Tooltip("카메라가 뒤로 물러나는 거리 (pitchAngle과 함께 시점 거리 결정)")]
        [SerializeField] private float depth  = 12f;

        [Header("RenderTexture")]
        [Tooltip("LeftFrame RectTransform — RenderTexture 해상도 기준으로 사용됩니다.")]
        [SerializeField] private RectTransform viewportFrame;

        [Tooltip("LeftFrame > VesselView RawImage — RenderTexture를 표시할 UI 이미지입니다.")]
        [SerializeField] private RawImage displayImage;

        // ── 내부 ─────────────────────────────────────────────────────
        private Camera        _camera;
        private RenderTexture _renderTexture;

        // ── 쉐이킹 상태 ──────────────────────────────────────────────
        private Vector3    _shakeOffset;
        private Coroutine  _shakeCoroutine;

        private void Awake()
        {
            Singleton = this;
            _camera = GetComponent<Camera>();
            transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        }

        private void Start()
        {
            SetupRenderTexture();
        }

        private void LateUpdate()
        {
            if (vessel == null) return;

            Vector3 vesselPos = vessel.transform.position;

            // height 위, depth 뒤 (-Z) 에서 45° 로 내려다보는 위치
            Vector3 desiredPosition = vesselPos + new Vector3(0f, height, -depth);

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                followSpeed * Time.deltaTime) + _shakeOffset;

            transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        }

        private void OnDestroy()
        {
            if (Singleton == this) Singleton = null;
            ReleaseRenderTexture();
        }

        // ── 카메라 쉐이킹 ────────────────────────────────────────────

        /// <summary>
        /// 카메라 쉐이킹을 시작합니다.
        /// </summary>
        /// <param name="duration">쉐이킹 지속 시간(초)</param>
        /// <param name="magnitude">쉐이킹 강도 (기본값 0.3)</param>
        public void ShakeCamera(float duration, float magnitude = 0.3f)
        {
            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);

            _shakeCoroutine = StartCoroutine(ShakeCameraRoutine(duration, magnitude));
        }

        private IEnumerator ShakeCameraRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                _shakeOffset = Random.insideUnitSphere * magnitude;
                elapsed += Time.deltaTime;
                yield return null;
            }

            _shakeOffset    = Vector3.zero;
            _shakeCoroutine = null;
        }

        // ── RenderTexture 설정 ────────────────────────────────────────
        /// <summary>
        /// LeftFrame RectTransform의 픽셀 크기로 RenderTexture를 생성하고
        /// Camera.targetTexture 와 RawImage.texture 에 동시 할당합니다.
        /// </summary>
        private void SetupRenderTexture()
        {
            if (_camera == null)
            {
                Debug.LogError("[VesselCameraController] Camera component not found.", this);
                return;
            }

            // 해상도 결정 — viewportFrame이 연결된 경우 그 픽셀 크기를 사용, 없으면 기본값
            int rtWidth  = 1280;
            int rtHeight = 720;

            if (viewportFrame != null)
            {
                // Canvas가 Screen Space - Overlay이면 rect.width/height가 픽셀 크기
                Rect frameRect = viewportFrame.rect;
                int w = Mathf.RoundToInt(Mathf.Abs(frameRect.width));
                int h = Mathf.RoundToInt(Mathf.Abs(frameRect.height));

                if (w > 0 && h > 0)
                {
                    rtWidth  = w;
                    rtHeight = h;
                }
            }

            // 기존 RenderTexture 해제 후 새로 생성
            ReleaseRenderTexture();

            _renderTexture = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.ARGB32)
            {
                name            = "VesselCameraRT",
                antiAliasing    = 1,
                filterMode      = FilterMode.Bilinear,
                wrapMode        = TextureWrapMode.Clamp
            };
            _renderTexture.Create();

            // Camera에 RenderTexture 할당 — Main Display 렌더링 중단
            _camera.targetTexture = _renderTexture;

            // RawImage에 RenderTexture 할당
            if (displayImage != null)
            {
                displayImage.texture = _renderTexture;
            }
            else
            {
                Debug.LogWarning("[VesselCameraController] displayImage(RawImage) 가 연결되지 않았습니다. " +
                                 "Inspector에서 LeftFrame > VesselView RawImage를 연결해주세요.", this);
            }

            Debug.Log($"[VesselCameraController] RenderTexture 생성 완료: {rtWidth}x{rtHeight}", this);
        }

        private void ReleaseRenderTexture()
        {
            if (_renderTexture == null) return;

            if (_camera != null && _camera.targetTexture == _renderTexture)
                _camera.targetTexture = null;

            if (displayImage != null && displayImage.texture == _renderTexture)
                displayImage.texture = null;

            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 에디터 플레이 중 Inspector 변경 시 RenderTexture 재생성
            if (!Application.isPlaying) return;
            SetupRenderTexture();
        }
#endif
    }
}
