using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TST
{
    /// <summary>
    /// 화면 9 (낚시 우주 영역 / 필드 선택) A/D 키 순환 선택 및 Space(BG) Lerp 이동 컨트롤러.
    /// SpaceMapController 게임오브젝트에 추가하세요.
    /// </summary>
    public class SpaceFieldSelectController : SingletonBase<SpaceFieldSelectController>
    {
        // ── 이벤트 ────────────────────────────────────────────────────
        /// <summary>선택 인덱스가 변경됐을 때 발행됩니다.</summary>
        public event Action<int> OnSelectionChanged;

        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private SpaceMapController spaceMapController;

        [Header("Scene References")]
        [Tooltip("Space(BG) Transform — 배경 이동 대상")]
        [SerializeField] private Transform spaceBG;

        [Tooltip("활성화된 FieldPoint Transform 리스트 (순서 = 선택 인덱스)")]
        [SerializeField] private List<Transform> fieldPoints = new List<Transform>();

        [Header("Settings")]
        [Tooltip("Lerp 이동 속도")]
        [SerializeField] private float moveSpeed = 3f;

        [Tooltip("Space 화면에서 사용하는 2D 카메라 Transform (2DTransitionCamera). " +
                 "CinemachineBrain이 붙은 Main Camera가 아닌 가상 카메라를 직접 연결해야 " +
                 "카메라 전환 중 위치가 튀는 문제를 방지할 수 있습니다.")]
        [SerializeField] private Transform cameraAnchor;

        // ── 공개 프로퍼티 ────────────────────────────────────────────
        public int SelectedIndex { get; private set; } = 0;
        public IReadOnlyList<Transform> FieldPoints => fieldPoints;

        // ── Unity 생명주기 ────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            spaceMapController = GetComponent<SpaceMapController>();
        }

        private void Update()
        {
            if (PhaseManager.Singleton.CurrentPhase != GamePhase.Space) return;

            HandleInput();
            UpdateBGPosition();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void HandleInput()
        {
            if (fieldPoints == null || fieldPoints.Count == 0) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
            {
                int prev = (SelectedIndex - 1 + fieldPoints.Count) % fieldPoints.Count;
                SetSelection(prev);
            }
            else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
            {
                int next = (SelectedIndex + 1) % fieldPoints.Count;
                SetSelection(next);
            }

            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
            {
                ConfirmSelection();
            }
        }

        private void SetSelection(int index)
        {
            if (index < 0 || index >= fieldPoints.Count) return;
            SelectedIndex = index;
            OnSelectionChanged?.Invoke(SelectedIndex);
        }

        private void ConfirmSelection()
        {
            if (spaceMapController == null) return;
            spaceMapController.SelectZone(SelectedIndex);
        }

        private void UpdateBGPosition()
        {
            if (spaceBG == null || fieldPoints == null || fieldPoints.Count == 0) return;
            if (cameraAnchor == null) return;

            Transform selected = fieldPoints[SelectedIndex];
            if (selected == null) return;

            // CinemachineBrain(Main Camera)이 아닌 2DTransitionCamera의 고정 위치를 기준으로 계산.
            // Main Camera는 Cinemachine 전환 중 위치가 튈 수 있으므로 직접 사용하지 않는다.
            Vector3 anchorPos = cameraAnchor.position;
            Vector3 target = new Vector3(
                spaceBG.position.x + (anchorPos.x - selected.position.x),
                spaceBG.position.y + (anchorPos.y - selected.position.y),
                spaceBG.position.z
            );

            spaceBG.position = Vector3.Lerp(spaceBG.position, target, Time.deltaTime * moveSpeed);
        }
    }
}
