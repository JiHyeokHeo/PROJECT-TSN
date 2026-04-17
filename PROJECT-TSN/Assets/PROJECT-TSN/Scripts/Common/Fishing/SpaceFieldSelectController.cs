using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TST
{
    /// <summary>
    /// Field selection controller in Space phase.
    /// - A/D (or Left/Right) changes selected point.
    /// - Space/Enter confirms selection through SpaceMapController.
    /// - Space BG follows selected point based on LeftFrame anchor helper.
    /// </summary>
    public class SpaceFieldSelectController : SingletonBase<SpaceFieldSelectController>
    {
        public event Action<int> OnSelectionChanged;

        [Header("Dependencies")]
        [SerializeField] private SpaceMapController spaceMapController;

        [Header("Scene References")]
        [SerializeField] private Transform spaceBG;
        [SerializeField] private List<Transform> fieldPoints = new List<Transform>();

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 3f;

        [Header("Anchor Sources")]
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private bool useLeftFrameAnchor = true;

        public int SelectedIndex { get; private set; }
        public IReadOnlyList<Transform> FieldPoints => fieldPoints;

        protected override void Awake()
        {
            base.Awake();

            if (spaceMapController == null)
                spaceMapController = GetComponent<SpaceMapController>();
        }

        private void Update()
        {
            if (PhaseManager.Singleton.CurrentPhase != GamePhase.Space)
                return;

            HandleInput();
            UpdateBGPosition();
        }

        private void HandleInput()
        {
            if (fieldPoints == null || fieldPoints.Count == 0)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                int prev = (SelectedIndex - 1 + fieldPoints.Count) % fieldPoints.Count;
                SetSelection(prev);
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                int next = (SelectedIndex + 1) % fieldPoints.Count;
                SetSelection(next);
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
                ConfirmSelection();
        }

        private void SetSelection(int index)
        {
            if (index < 0 || index >= fieldPoints.Count)
                return;

            SelectedIndex = index;
            OnSelectionChanged?.Invoke(SelectedIndex);
        }

        private void ConfirmSelection()
        {
            if (spaceMapController == null)
                return;

            spaceMapController.SelectZone(SelectedIndex);
        }

        private void UpdateBGPosition()
        {
            if (spaceBG == null || fieldPoints == null || fieldPoints.Count == 0)
                return;

            Transform selected = fieldPoints[SelectedIndex];
            if (selected == null)
                return;

            if (!TryGetAnchorPosition(selected, out Vector3 anchorPos))
                return;

            Vector3 target = new Vector3(
                spaceBG.position.x + (anchorPos.x - selected.position.x),
                spaceBG.position.y + (anchorPos.y - selected.position.y),
                spaceBG.position.z);

            spaceBG.position = Vector3.Lerp(spaceBG.position, target, Time.deltaTime * moveSpeed);
        }

        private bool TryGetAnchorPosition(Transform selected, out Vector3 anchorPos)
        {
            anchorPos = Vector3.zero;

            if (useLeftFrameAnchor)
            {
                anchorPos = Utils.GetWorldPosToLeftFramePos(selected);
                if (anchorPos != Vector3.zero)
                    return true;
            }

            if (cameraAnchor != null)
            {
                anchorPos = cameraAnchor.position;
                return true;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                anchorPos = mainCamera.transform.position;
                return true;
            }

            return false;
        }
    }
}
