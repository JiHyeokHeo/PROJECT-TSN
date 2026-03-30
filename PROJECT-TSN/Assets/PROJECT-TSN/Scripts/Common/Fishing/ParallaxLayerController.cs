using UnityEngine;

namespace TST
{
    /// <summary>
    /// 패럴랙스 레이어 시스템.
    /// VesselController의 XZ 이동량을 기준으로 각 레이어 Transform에
    /// parallaxFactor(0~1) 를 곱한 offset 을 적용합니다.
    ///
    /// [레이어 우선순위 예시 — 뒤→앞]
    ///   0  Background      factor ≈ 0.05
    ///   1  Decoration C    factor ≈ 0.15
    ///   2  Decoration B    factor ≈ 0.30
    ///   3  Decoration A    factor ≈ 0.50
    ///   4  Objects         factor ≈ 0.75
    ///   5  FloorGrid       factor ≈ 0.90
    ///   6  Player          factor = 1.00  (또는 미등록 — 관측선 자신이므로)
    ///
    /// [사용법]
    ///   1. 씬에 빈 GameObject를 두고 이 컴포넌트를 추가합니다.
    ///   2. Inspector → Layers 배열에 각 레이어의 Transform과 parallaxFactor를 입력합니다.
    ///   3. Vessel Reference 필드에 VesselController를 보유한 Transform을 연결합니다.
    ///      (비워 두면 VesselController.Singleton 을 Awake에서 자동 참조합니다.)
    /// </summary>
    public class ParallaxLayerController : MonoBehaviour
    {
        // ── 중첩 타입 ─────────────────────────────────────────────────

        [System.Serializable]
        public struct ParallaxLayer
        {
            [Tooltip("패럴랙스를 적용할 레이어 Transform")]
            public Transform layerTransform;

            [Tooltip("0 = 완전 고정(배경), 1 = 관측선과 동일 이동")]
            [Range(0f, 1f)]
            public float parallaxFactor;
        }

        // ── Inspector 필드 ────────────────────────────────────────────

        [Header("Vessel Reference")]
        [Tooltip("이동 기준이 되는 관측선 Transform. 비어 있으면 VesselController.Singleton 자동 참조.")]
        [SerializeField] private Transform vesselTransform;

        [Header("Layers (뒤 → 앞 순서로 나열)")]
        [SerializeField] private ParallaxLayer[] layers;

        // ── 런타임 상태 ──────────────────────────────────────────────

        private Vector3 _prevVesselPos;

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Awake()
        {
            if (vesselTransform == null && VesselController.Singleton != null)
                vesselTransform = VesselController.Singleton.transform;
        }

        private void Start()
        {
            if (vesselTransform != null)
                _prevVesselPos = vesselTransform.position;
        }

        private void LateUpdate()
        {
            if (vesselTransform == null) return;
            if (layers == null || layers.Length == 0) return;

            Vector3 currentPos = vesselTransform.position;

            // XZ 평면 이동량만 추출 (Y 고정)
            Vector3 delta = currentPos - _prevVesselPos;
            delta.y = 0f;

            if (delta.sqrMagnitude > 0f)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    Transform t = layers[i].layerTransform;
                    if (t == null) continue;

                    Vector3 offset = delta * layers[i].parallaxFactor;
                    t.position += offset;
                }
            }

            _prevVesselPos = currentPos;
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// 모든 레이어의 위치를 기준점으로 즉시 리셋합니다.
        /// 낚시 세션 시작 시 호출해 레이어 위치 초기화에 사용합니다.
        /// </summary>
        public void ResetLayers(Vector3[] originPositions)
        {
            if (layers == null) return;

            int count = Mathf.Min(layers.Length, originPositions.Length);
            for (int i = 0; i < count; i++)
            {
                if (layers[i].layerTransform == null) continue;

                Vector3 origin = originPositions[i];
                origin.y = layers[i].layerTransform.position.y; // Y 유지
                layers[i].layerTransform.position = origin;
            }

            if (vesselTransform != null)
                _prevVesselPos = vesselTransform.position;
        }

        /// <summary>
        /// 세션 시작 시 현재 위치를 기준 위치로 스냅합니다 (이동 누적 리셋만).
        /// </summary>
        public void SnapToCurrent()
        {
            if (vesselTransform != null)
                _prevVesselPos = vesselTransform.position;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // layers 배열 요소가 null Transform을 가지지 않도록 경고
            if (layers == null) return;
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].layerTransform == null)
                    Debug.LogWarning($"[ParallaxLayerController] layers[{i}].layerTransform이 비어 있습니다.");
            }
        }
#endif
    }
}
