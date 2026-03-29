using System;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 앱 수명주기 관리: 오프라인 생산 처리, 세션 저장.
    /// 씬에 하나만 배치. DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        const string LastQuitTimeKey = "tst_last_quit_unix";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // 한 프레임 뒤에 실행 → 모든 Start() (IdleBootstrapper 포함) 완료 후 건물 등록 보장
            StartCoroutine(DelayedOfflineCheck());
        }

        System.Collections.IEnumerator DelayedOfflineCheck()
        {
            yield return null;
            HandleOfflineProductionOnStart();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveSessionEnd();
        }

        void OnApplicationQuit()
        {
            SaveSessionEnd();
        }

        // ──────────────────────────────────────────────────
        void HandleOfflineProductionOnStart()
        {
            long lastQuit = LoadLastQuitTime();
            if (lastQuit <= 0L) return;

            float offlineSeconds = Mathf.Max(0f, (float)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastQuit));
            if (offlineSeconds > 10f)
                ProductionManager.Singleton?.HandleOfflineProduction(offlineSeconds);
        }

        void SaveSessionEnd()
        {
            ProductionManager.Singleton?.SaveAllBuildingStates();
            PlayerPrefs.SetString(LastQuitTimeKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            PlayerPrefs.Save();
        }

        static long LoadLastQuitTime()
        {
            string raw = PlayerPrefs.GetString(LastQuitTimeKey, "0");
            return long.TryParse(raw, out long result) ? result : 0L;
        }
    }
}
