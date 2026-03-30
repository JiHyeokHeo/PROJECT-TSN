using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace TST
{
    /// <summary>
    /// Popup_Menu — in-game pause menu, opened with ESC.
    /// Inspector wiring: resumeButton, loadButton, optionsButton, quitButton.
    /// Prefab path: Resources/UI/Prefabs/UI.Popup_Menu
    /// </summary>
    public class MenuPopupUI : UIBase
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        // Tracks whether the ESC-open shortcut is currently enabled.
        // Disabled while on the title screen so the menu does not steal ESC.
        private bool _escEnabled = true;

        private void Awake()
        {
            resumeButton.onClick.AddListener(Hide);
            loadButton.onClick.AddListener(OnLoad);
            optionsButton.onClick.AddListener(OnOptions);
            quitButton.onClick.AddListener(OnQuit);
        }

        private void Update()
        {
            if (!_escEnabled) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (gameObject.activeSelf)
                    Hide();
                else
                    Show();
            }
        }

        // ----------------------------------------------------------------
        //  Public control
        // ----------------------------------------------------------------
        /// <param name="escEnabled">Pass false from the title screen so ESC does not re-open this popup.</param>
        public void SetEscEnabled(bool escEnabled)
        {
            _escEnabled = escEnabled;
        }

        // ----------------------------------------------------------------
        //  Button handlers
        // ----------------------------------------------------------------
        private void OnLoad()
        {
            Hide();
            UIManager.Show<LoadScreenUI>(UIList.Panel_LoadScreen);
        }

        private void OnOptions()
        {
            var options = UIManager.Show<OptionsPopupUI>(UIList.Popup_Options);
            options?.Show(fromTitle: false);
            Hide();
        }

        private static void OnQuit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
