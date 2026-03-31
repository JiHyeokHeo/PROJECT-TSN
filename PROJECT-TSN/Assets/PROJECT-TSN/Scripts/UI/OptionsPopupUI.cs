using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// Popup_Options — BGM 볼륨, SFX 볼륨, 전체화면 설정 팝업.
    ///
    /// 볼륨 설정은 SoundManager를 통해 PlayerPrefs에 영속합니다.
    /// Inspector 연결: bgmSlider, sfxSlider, fullscreenToggle, closeButton.
    /// Prefab 경로: Resources/UI/Prefabs/UI.Popup_Options
    /// </summary>
    public class OptionsPopupUI : UIBase
    {
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
        //  Show
        // ----------------------------------------------------------------
        public override void Show() => Show(fromTitle: false);

        /// <param name="fromTitle">
        /// true 이면 닫을 때 TitleUI로 복귀, false 이면 MenuPopupUI로 복귀.
        /// </param>
        public void Show(bool fromTitle)
        {
            _fromTitle = fromTitle;
            base.Show();
            LoadSettings();
        }

        // ----------------------------------------------------------------
        //  Slider / Toggle 핸들러
        // ----------------------------------------------------------------
        private void OnBGMChanged(float value) => SoundManager.Singleton.Volume_BGM = value;

        private void OnSFXChanged(float value) => SoundManager.Singleton.Volume_SFX = value;

        private static void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        // ----------------------------------------------------------------
        //  닫기
        // ----------------------------------------------------------------
        private void OnClose()
        {
            SoundManager.Singleton.SaveVolumes();
            Hide();

            if (_fromTitle)
                UIManager.Show<TitleUI>(UIList.Panel_Title);
            else
                UIManager.Show<MenuPopupUI>(UIList.Popup_Menu);
        }

        // ----------------------------------------------------------------
        //  설정 불러오기
        // ----------------------------------------------------------------
        private void LoadSettings()
        {
            bgmSlider.SetValueWithoutNotify(SoundManager.Singleton.Volume_BGM);
            sfxSlider.SetValueWithoutNotify(SoundManager.Singleton.Volume_SFX);
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        }
    }
}
