using UnityEngine;

namespace TST
{
    /// <summary>
    /// 앱 수명주기 관리.
    /// - 앱 종료/포그라운드 이탈 시 마지막으로 사용한 슬롯에 자동 저장합니다.
    /// - DontDestroyOnLoad. 씬에 하나만 배치하십시오.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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

        void OnApplicationPause(bool pauseStatus)
        {
            // 모바일: 백그라운드 전환 시 자동 저장
            if (pauseStatus) AutoSave();
        }

        void OnApplicationQuit()
        {
            AutoSave();
            PlayerPrefs.Save();
        }

        // ----------------------------------------------------------------
        //  자동 저장
        // ----------------------------------------------------------------
        private static void AutoSave()
        {
            int slot = SaveSystem.Singleton.LastUsedSlot;
            if (slot < 0) return;   // 이번 세션에 저장/로드를 한 번도 안 한 경우 스킵

            bool ok = SaveSystem.Singleton.Save(slot);
            if (ok)
                Debug.LogFormat("[GameManager] Auto-saved to slot {0}.", slot);
        }
    }
}
