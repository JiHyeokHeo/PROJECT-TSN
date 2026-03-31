using UnityEngine;

namespace TST
{
    /// <summary>
    /// 사운드 관리 시스템.
    /// BGM / SFX / UI 세 개의 AudioSource 채널을 관리합니다.
    /// 볼륨 설정은 PlayerPrefs에 영속하며, Initialize() 호출 시 복원됩니다.
    ///
    /// Inspector 연결: bgmSource, sfxSource, uiSource
    ///   미연결 시 Awake에서 자식 GameObject에 자동 생성합니다.
    ///
    /// 사용 예:
    ///   SoundManager.Singleton.PlayBGM(bgmClip);
    ///   SoundManager.Singleton.PlaySFX(sfxClip);
    ///   SoundManager.Singleton.Volume_BGM = 0.8f;
    /// </summary>
    public class SoundManager : SingletonBase<SoundManager>
    {
        // PlayerPrefs 키 — OptionsPopupUI와 공유
        public const string PrefKey_Master = "Vol_Master";
        public const string PrefKey_BGM    = "Vol_BGM";
        public const string PrefKey_SFX    = "Vol_SFX";
        public const string PrefKey_UI     = "Vol_UI";
        private const float DefaultVolume  = 1f;

        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        // ----------------------------------------------------------------
        //  Volume properties — PlayerPrefs 영속 + AudioSource 즉시 적용
        // ----------------------------------------------------------------
        public float Volume_Master
        {
            get => PlayerPrefs.GetFloat(PrefKey_Master, DefaultVolume);
            set
            {
                value = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PrefKey_Master, value);
                AudioListener.volume = value;
            }
        }

        public float Volume_BGM
        {
            get => PlayerPrefs.GetFloat(PrefKey_BGM, DefaultVolume);
            set
            {
                value = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PrefKey_BGM, value);
                if (bgmSource != null) bgmSource.volume = value;
            }
        }

        public float Volume_SFX
        {
            get => PlayerPrefs.GetFloat(PrefKey_SFX, DefaultVolume);
            set
            {
                value = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PrefKey_SFX, value);
                if (sfxSource != null) sfxSource.volume = value;
            }
        }

        public float Volume_UI
        {
            get => PlayerPrefs.GetFloat(PrefKey_UI, DefaultVolume);
            set
            {
                value = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PrefKey_UI, value);
                if (uiSource != null) uiSource.volume = value;
            }
        }

        // ----------------------------------------------------------------
        //  Lifecycle
        // ----------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();
            EnsureAudioSources();
        }

        /// <summary>저장된 볼륨값을 AudioSource에 복원합니다. BootStrapper에서 호출.</summary>
        public void Initialize()
        {
            AudioListener.volume      = Volume_Master;
            if (bgmSource != null) bgmSource.volume = Volume_BGM;
            if (sfxSource != null) sfxSource.volume = Volume_SFX;
            if (uiSource  != null) uiSource.volume  = Volume_UI;
            Debug.Log("[SoundManager] Initialized.");
        }

        // ----------------------------------------------------------------
        //  BGM
        // ----------------------------------------------------------------
        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null || clip == null) return;
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        public void StopBGM()  => bgmSource?.Stop();
        public void PauseBGM() => bgmSource?.Pause();
        public void ResumeBGM() => bgmSource?.UnPause();

        public bool IsBGMPlaying => bgmSource != null && bgmSource.isPlaying;

        // ----------------------------------------------------------------
        //  SFX / UI
        // ----------------------------------------------------------------
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, Volume_SFX);
        }

        public void PlayUISFX(AudioClip clip)
        {
            if (uiSource == null || clip == null) return;
            uiSource.PlayOneShot(clip, Volume_UI);
        }

        public void StopAll()
        {
            bgmSource?.Stop();
            sfxSource?.Stop();
            uiSource?.Stop();
        }

        // ----------------------------------------------------------------
        //  PlayerPrefs 영속화
        // ----------------------------------------------------------------
        public void SaveVolumes() => PlayerPrefs.Save();

        // ----------------------------------------------------------------
        //  Internal — AudioSource 자동 생성
        // ----------------------------------------------------------------
        private void EnsureAudioSources()
        {
            bgmSource = EnsureSource(bgmSource, "BGM_Source", loop: true);
            sfxSource = EnsureSource(sfxSource, "SFX_Source", loop: false);
            uiSource  = EnsureSource(uiSource,  "UI_Source",  loop: false);
        }

        private AudioSource EnsureSource(AudioSource existing, string childName, bool loop)
        {
            if (existing != null) return existing;

            var go   = new GameObject(childName);
            go.transform.SetParent(transform);
            var src  = go.AddComponent<AudioSource>();
            src.loop         = loop;
            src.playOnAwake  = false;
            src.spatialBlend = 0f; // 2D
            return src;
        }
    }
}
