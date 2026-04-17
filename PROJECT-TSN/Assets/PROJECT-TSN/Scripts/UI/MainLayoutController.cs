using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO : 해상도 EXCHANGE 코드인거 같음. 
namespace TST
{
    /// <summary>
    /// Root controller for the two-panel main layout.
    ///
    /// Layout overview:
    ///   LeftFrame  — circular masked frame, shows the field side-view
    ///                (Sprite / dynamic prefab content).
    ///   RightFrame — rectangular frame, shows POV / dialogue content.
    ///
    /// Dialogue box:
    ///   Attached to RightFrame. Has speaker name, body text,
    ///   an auto-play toggle, and a log button.
    ///
    /// Phase wiring:
    ///   Subscribes to PhaseManager.OnPhaseChanged and fires
    ///   OnPhaseContentRequested so scene-level controllers
    ///   can swap content without MainLayoutController holding
    ///   scene-specific references.
    ///
    /// Inspector wiring:
    ///   - LeftFrame        : RectTransform of the left panel root.
    ///   - LeftImage        : Image inside LeftFrame for sprite display.
    ///   - LeftContentRoot  : Transform used as parent for dynamic prefabs in left panel.
    ///   - RightFrame       : RectTransform of the right panel root.
    ///   - RightImage       : Image inside RightFrame for sprite display.
    ///   - RightContentRoot : Transform used as parent for dynamic prefabs in right panel.
    ///   - DialogueBox      : GameObject containing all dialogue UI elements.
    ///   - SpeakerLabel     : TMP text for the speaker name.
    ///   - DialogueLabel    : TMP text for the dialogue body.
    ///   - AutoPlayButton   : Button that toggles auto-play mode.
    ///   - LogButton        : Button that opens the dialogue log.
    ///
    ///   Attach this component to the root Canvas GameObject of the
    ///   prefab named "UI.MainLayout" placed in Resources/UI/Prefabs/.
    /// </summary>
    public class MainLayoutController : UIBase
    {
        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        /// <summary>
        /// Fired when the phase changes so external systems can
        /// push new content to the frames.
        /// Args: (oldPhase, newPhase)
        /// </summary>
        public event Action<GamePhase, GamePhase> OnPhaseContentRequested;

        // ----------------------------------------------------------------
        //  Inspector references — Left frame
        // ----------------------------------------------------------------
        [Header("Left Frame (Circular Mask)")]
        [SerializeField] private RectTransform leftFrame;
        [SerializeField] private Image leftImage;
        [SerializeField] private Transform leftContentRoot;

        // ----------------------------------------------------------------
        //  Inspector references — Right frame
        // ----------------------------------------------------------------
        [Header("Right Frame (Rectangular)")]
        [SerializeField] private RectTransform rightFrame;
        [SerializeField] private Image rightImage;
        [SerializeField] private Transform rightContentRoot;

        // ----------------------------------------------------------------
        //  Inspector references — Dialogue box
        // ----------------------------------------------------------------
        [Header("Dialogue Box")]
        [SerializeField] private RectTransform dialogueBox;
        [SerializeField] private TextMeshProUGUI speakerLabel;
        [SerializeField] private TextMeshProUGUI dialogueLabel;
        [SerializeField] private Button autoPlayButton;
        [SerializeField] private Button logButton;
        [SerializeField] private Button inventoryButton;

        [Header("Main HUD Framework Roots (Optional)")]
        [SerializeField] private Transform mainHudPanelRoot;
        [SerializeField] private Transform mainHudPopupRoot;

        // ----------------------------------------------------------------
        //  ResponsiveLayout
        // ----------------------------------------------------------------

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        private GameObject _activeLeftPrefab;
        private GameObject _activeRightPrefab;

        public Transform MainHudPanelRoot => mainHudPanelRoot != null ? mainHudPanelRoot : transform;
        public Transform MainHudPopupRoot => mainHudPopupRoot != null ? mainHudPopupRoot : transform;

        // ----------------------------------------------------------------
        //  Unity lifecycle
        // ----------------------------------------------------------------
        private void OnEnable()
        {
            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void Start()
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            //if (dialogueBox != null)
            //    dialogueBox.gameObject.SetActive(false);

            

            if (autoPlayButton != null)
                autoPlayButton.onClick.AddListener(OnAutoPlayClicked);

            if (logButton != null)
                logButton.onClick.AddListener(OnLogClicked);

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OnInventoryClicked);
        }

        private void Update()
        {
            //var pos = Input.mousePosition;
            //Debug.Log(pos);
        }

        // ----------------------------------------------------------------
        //  Left frame content
        // ----------------------------------------------------------------
        public void SetLeftContent(Sprite sprite)
        {
            ClearLeftPrefab();
            if (leftImage == null) return;
            leftImage.sprite  = sprite;
            leftImage.enabled = sprite != null;
        }

        public void SetLeftContent(GameObject prefab)
        {
            ClearLeftPrefab();
            if (prefab == null || leftContentRoot == null) return;
            _activeLeftPrefab = Instantiate(prefab, leftContentRoot);
        }

        // ----------------------------------------------------------------
        //  Right frame content
        // ----------------------------------------------------------------
        public void SetRightContent(Sprite sprite)
        {
            ClearRightPrefab();
            if (rightImage == null) return;
            rightImage.sprite  = sprite;
            rightImage.enabled = sprite != null;
        }

        public void SetRightContent(GameObject prefab)
        {
            ClearRightPrefab();
            if (prefab == null || rightContentRoot == null) return;
            _activeRightPrefab = Instantiate(prefab, rightContentRoot);
        }

        // ----------------------------------------------------------------
        //  Dialogue
        // ----------------------------------------------------------------
        public void ShowDialogue(string speaker, string text)
        {
            if (dialogueBox != null)
                dialogueBox.gameObject.SetActive(true);

            if (speakerLabel != null)
                speakerLabel.text = speaker ?? string.Empty;

            if (dialogueLabel != null)
                dialogueLabel.text = text ?? string.Empty;
        }

        public void HideDialogue()
        {
            //if (dialogueBox != null)
            //    dialogueBox.gameObject.SetActive(false);
        }

        // ----------------------------------------------------------------
        //  Phase handler — delegates to external listener via event
        // ----------------------------------------------------------------
        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            OnPhaseContentRequested?.Invoke(oldPhase, newPhase);
        }

        // ----------------------------------------------------------------
        //  Button callbacks
        // ----------------------------------------------------------------
        private void OnAutoPlayClicked()
        {
            // TODO: implement auto-play toggle; connect to a dialogue system.
            Debug.Log("[MainLayoutController] AutoPlay toggled.");
        }

        private void OnLogClicked()
        {
            // TODO: open dialogue log popup.
            Debug.Log("[MainLayoutController] Log button clicked.");
        }

        private void OnInventoryClicked()
        {
            UIManager.Show<InventoryPopupUI>(UIList.Popup_Inventory);
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------
        private void ClearLeftPrefab()
        {
            if (_activeLeftPrefab != null)
            {
                Destroy(_activeLeftPrefab);
                _activeLeftPrefab = null;
            }
        }

        private void ClearRightPrefab()
        {
            if (_activeRightPrefab != null)
            {
                Destroy(_activeRightPrefab);
                _activeRightPrefab = null;
            }
        }
    }
}
