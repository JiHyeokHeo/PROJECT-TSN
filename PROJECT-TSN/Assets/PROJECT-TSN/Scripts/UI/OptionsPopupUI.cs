using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// Popup_Options — settings popup for BGM volume, SFX volume, and fullscreen toggle.
    ///
    /// Volume values are persisted via PlayerPrefs (keys: "Vol_BGM", "Vol_SFX").
    /// AudioListener.volume is driven by BGM volume as a master proxy until a
    /// dedicated audio system (SoundManager / AudioMixer) is wired in.
    ///
    /// Inspector wiring: bgmSlider, sfxSlider, fullscreenToggle, closeButton.
    /// Prefab path: Resources/UI/Prefabs/UI.Popup_Options
    /// </summary>
    public class OptionsPopupUI : UIBase
    {
        private const string PrefKey_BGM        = "Vol_BGM";
        private const string PrefKey_SFX        = "Vol_SFX";
        private const float  DefaultVolume      = 1f;

        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button closeButton;

        private bool _fromTitle;

        private void Awake()
        {
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            closeButton.onClick.AddListener(OnClose);
        }

        // ----------------------------------------------------------------
        //  Show overloads
        // ----------------------------------------------------------------
        public override void Show()
        {
            Show(fromTitle: false);
        }

        /// <param name="fromTitle">
        /// True when opened from the title screen — closing returns to TitleUI
        /// instead of MenuPopupUI.
        /// </param>
        public void Show(bool fromTitle)
        {
            _fromTitle = fromTitle;
            base.Show();
            LoadSettings();
        }

        // ----------------------------------------------------------------
        //  Slider / toggle handlers
        // ----------------------------------------------------------------
        private void OnBGMChanged(float value)
        {
            PlayerPrefs.SetFloat(PrefKey_BGM, value);
            // Proxy until SoundManager is active
            AudioListener.volume = value;
        }

        private void OnSFXChanged(float value)
        {
            PlayerPrefs.SetFloat(PrefKey_SFX, value);
            // SFX category hook — replace with SoundManager.Singleton.Volume_SFX = value
        }

        private static void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        // ----------------------------------------------------------------
        //  Close
        // ----------------------------------------------------------------
        private void OnClose()
        {
            PlayerPrefs.Save();
            Hide();

            if (_fromTitle)
                UIManager.Show<TitleUI>(UIList.Panel_Title);
            else
                UIManager.Show<MenuPopupUI>(UIList.Popup_Menu);
        }

        // ----------------------------------------------------------------
        //  Persistence
        // ----------------------------------------------------------------
        private void LoadSettings()
        {
            float bgm = PlayerPrefs.GetFloat(PrefKey_BGM, DefaultVolume);
            float sfx = PlayerPrefs.GetFloat(PrefKey_SFX, DefaultVolume);

            bgmSlider.SetValueWithoutNotify(bgm);
            sfxSlider.SetValueWithoutNotify(sfx);
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);

            // Apply immediately so audio reflects saved state on open
            AudioListener.volume = bgm;
        }
    }
}
