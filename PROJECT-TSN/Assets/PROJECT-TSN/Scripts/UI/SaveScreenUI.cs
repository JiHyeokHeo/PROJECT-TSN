using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TST
{
    /// <summary>
    /// Panel_SaveScreen — displays three save slots and writes the current session to the chosen slot.
    /// Inspector wiring: slotButtons[3], slotLabels[3].
    /// Prefab path: Resources/UI/Prefabs/UI.Panel_SaveScreen
    /// </summary>
    public class SaveScreenUI : UIBase
    {
        private const string EmptySlotLabel   = "빈 슬롯";
        private const string SlotLabelFormat  = "Day {0}  |  {1}";

        [SerializeField] private Button[]            slotButtons;
        [SerializeField] private TextMeshProUGUI[]   slotLabels;
        [SerializeField] private Button              closeButton;

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
            bool saved = SaveSystem.Singleton.Save(slotIndex);
            if (saved)
            {
                RefreshSlots();
                // After saving, continue the game — keep the current phase.
                Hide();
            }
        }

        // ----------------------------------------------------------------
        //  Display helpers
        // ----------------------------------------------------------------
        private void RefreshSlots()
        {
            for (int i = 0; i < SaveSystem.SLOT_COUNT; i++)
            {
                SaveData preview = SaveSystem.Singleton.GetPreview(i);
                if (preview != null)
                    slotLabels[i].text = string.Format(SlotLabelFormat, preview.currentDay, preview.lastSavedAt);
                else
                    slotLabels[i].text = EmptySlotLabel;
            }
        }
    }
}
