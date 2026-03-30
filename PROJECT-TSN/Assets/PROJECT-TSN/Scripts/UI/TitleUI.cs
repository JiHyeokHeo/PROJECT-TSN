using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// Panel_Title — main title screen.
    /// Inspector wiring: newGameButton, loadButton, optionsButton, quitButton.
    /// Prefab path: Resources/UI/Prefabs/UI.Panel_Title
    /// </summary>
    public class TitleUI : UIBase
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            newGameButton.onClick.AddListener(OnNewGame);
            loadButton.onClick.AddListener(OnLoad);
            optionsButton.onClick.AddListener(OnOptions);
            quitButton.onClick.AddListener(OnQuit);
        }

        public override void Show()
        {
            base.Show();
            RefreshLoadButton();

            // Prevent ESC from opening the in-game menu while the title screen is visible.
            var menu = UIManager.Singleton.GetUI<MenuPopupUI>(UIList.Popup_Menu);
            if (menu != null) menu.SetEscEnabled(false);
        }

        public override void Hide()
        {
            base.Hide();

            // Re-enable ESC-menu once the player enters the game world.
            var menu = UIManager.Singleton.GetUI<MenuPopupUI>(UIList.Popup_Menu);
            if (menu != null) menu.SetEscEnabled(true);
        }

        // ----------------------------------------------------------------
        //  Button handlers
        // ----------------------------------------------------------------
        private void OnNewGame()
        {
            Hide();
            // 프롤로그 컷신 재생 후 DayAttic으로 전환합니다.
            GameFlowDirector.Singleton.PlayPrologue();
        }

        private void OnLoad()
        {
            UIManager.Show<LoadScreenUI>(UIList.Panel_LoadScreen);
        }

        private void OnOptions()
        {
            var options = UIManager.Show<OptionsPopupUI>(UIList.Popup_Options);
            options?.Show(fromTitle: true);
        }

        private static void OnQuit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------
        private void RefreshLoadButton()
        {
            if (loadButton != null)
                loadButton.interactable = SaveSystem.Singleton.HasAnySave();
        }
    }
}
