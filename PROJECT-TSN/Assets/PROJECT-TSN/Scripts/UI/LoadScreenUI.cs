using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TST
{
    /// <summary>
    /// Panel_LoadScreen — displays three save slots and restores the chosen slot into all runtime systems.
    /// Inspector wiring: slotButtons[3], slotLabels[3].
    /// Prefab path: Resources/UI/Prefabs/UI.Panel_LoadScreen
    /// </summary>
    public class LoadScreenUI : UIBase
    {
        private const string EmptySlotLabel  = "빈 슬롯";
        private const string SlotLabelFormat = "Day {0}  |  {1}";

        [SerializeField] private Button[]           slotButtons;
        [SerializeField] private TextMeshProUGUI[]  slotLabels;
        [SerializeField] private Button             closeButton;

        private void Awake()
        {
            for (int i = 0; i < SaveSystem.SLOT_COUNT; i++)
            {
                int slotIndex = i; // capture for closure
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(slotIndex));
            }

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            base.Show();
            RefreshSlots();
        }

        // ----------------------------------------------------------------
        //  Slot interaction
        // ----------------------------------------------------------------
        private void OnSlotClicked(int slotIndex)
        {
            bool loaded = SaveSystem.Singleton.Load(slotIndex);
            if (loaded)
            {
                Hide();
                // 메인 레이아웃 표시 후, 복원된 페이즈를 강제 발동해 UI를 동기화합니다.
                UIManager.Show<MainLayoutController>(UIList.MainLayout);
                PhaseManager.Singleton.ForceTransitionTo(PhaseManager.Singleton.CurrentPhase);
            }
        }

        // ----------------------------------------------------------------
        //  Display helpers
        // ----------------------------------------------------------------
        private void RefreshSlots()
        {
            for (int i = 0; i < SaveSystem.SLOT_COUNT; i++)
            {
                bool hasSave = SaveSystem.Singleton.HasSave(i);
                slotButtons[i].interactable = hasSave;

                if (hasSave)
                {
                    SaveData preview = SaveSystem.Singleton.GetPreview(i);
                    slotLabels[i].text = preview != null
                        ? string.Format(SlotLabelFormat, preview.currentDay, preview.lastSavedAt)
                        : EmptySlotLabel;
                }
                else
                {
                    slotLabels[i].text = EmptySlotLabel;
                }
            }
        }
    }
}
