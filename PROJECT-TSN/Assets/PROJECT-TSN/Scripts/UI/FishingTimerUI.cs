using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// Fishing phase timer UI.
    /// - Updates arc/needle visuals from remaining time.
    /// - Supports click-to-skip fishing.
    /// - Reacts to shared UI visibility events so timer hides while blocking popups are open.
    /// </summary>
    public class FishingTimerUI : UIBase, IPointerClickHandler
    {
        [Header("Dependencies")]
        [SerializeField] private FishingPhaseController fishingPhaseController;

        [Header("Arc")]
        [SerializeField] private Image arcImage;

        [Header("Needle")]
        [SerializeField] private Image needleImage;

        private const float fullRotationDeg = 360f;
        private bool isVisibilityEventBound;

        public override void Show()
        {
            base.Show();
            BindTimerEvent();
            BindVisibilityEvent();
            SyncTimer();
            ApplyOverlayVisibilityPolicy();
        }

        public override void Hide()
        {
            UnbindVisibilityEvent();
            UnbindTimerEvent();
            base.Hide();
        }

        private void BindTimerEvent()
        {
            if (fishingPhaseController == null)
                return;

            fishingPhaseController.OnTimerUpdated -= HandleTimerUpdated;
            fishingPhaseController.OnTimerUpdated += HandleTimerUpdated;
        }

        private void UnbindTimerEvent()
        {
            if (fishingPhaseController == null)
                return;

            fishingPhaseController.OnTimerUpdated -= HandleTimerUpdated;
        }

        private void BindVisibilityEvent()
        {
            if (isVisibilityEventBound)
                return;

            if (!UIManager.TryGetExisting(out UIManager uiManager))
                return;

            uiManager.OnUIVisibilityChanged += HandleUIVisibilityChanged;
            isVisibilityEventBound = true;
        }

        private void UnbindVisibilityEvent()
        {
            if (!isVisibilityEventBound)
                return;

            if (UIManager.TryGetExisting(out UIManager uiManager))
            {
                uiManager.OnUIVisibilityChanged -= HandleUIVisibilityChanged;
            }

            isVisibilityEventBound = false;
        }

        private void HandleTimerUpdated(float remainingTime)
        {
            ApplyTimer(remainingTime);
        }

        private void HandleUIVisibilityChanged(UIList uiType, bool isVisible, UIBase ui)
        {
            if (!IsVisibilityAffectingTimer(uiType))
                return;

            ApplyOverlayVisibilityPolicy();
        }

        private static bool IsVisibilityAffectingTimer(UIList uiType)
        {
            return uiType == UIList.Popup_Inventory
                || uiType == UIList.Popup_ObservationJournal
                || uiType == UIList.Popup_Menu;
        }

        private void ApplyOverlayVisibilityPolicy()
        {
            if (fishingPhaseController == null || !fishingPhaseController.IsActive)
                return;

            bool hasBlockingPopup =
                IsPopupVisible(UIList.Popup_Inventory) ||
                IsPopupVisible(UIList.Popup_ObservationJournal) ||
                IsPopupVisible(UIList.Popup_Menu);

            SetTimerVisible(!hasBlockingPopup);
        }

        private static bool IsPopupVisible(UIList popupType)
        {
            if (!UIManager.TryGetExisting(out UIManager uiManager))
                return false;

            if (!uiManager.GetUI<UIBase>(popupType, out UIBase popup))
                return false;

            return popup.gameObject.activeSelf;
        }

        private void SetTimerVisible(bool visible)
        {
            if (gameObject.activeSelf == visible)
                return;

            gameObject.SetActive(visible);
            if (visible)
            {
                SyncTimer();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (fishingPhaseController == null)
                return;

            if (!fishingPhaseController.IsActive)
                return;

            fishingPhaseController.SkipFishing();
        }

        private void SyncTimer()
        {
            if (fishingPhaseController == null)
                return;

            ApplyTimer(fishingPhaseController.RemainingTime);
        }

        private void ApplyTimer(float remainingTime)
        {
            if (fishingPhaseController == null)
                return;

            float totalTime = fishingPhaseController.TotalTime;
            if (totalTime <= 0f)
                return;

            float fillAmount = 1f - Mathf.Clamp01(remainingTime / totalTime);

            if (arcImage != null)
            {
                arcImage.fillAmount = fillAmount;
            }

            if (needleImage != null)
            {
                float angleDeg = -fillAmount * fullRotationDeg;
                needleImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
            }
        }
    }
}
