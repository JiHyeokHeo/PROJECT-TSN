using System;
using UnityEngine;
using UnityEngine.UI;

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
            //StartCoroutine(DelayedOfflineCheck());
        }

        //[Button()]
        //void StartFishingButton()
        //{
        //    //FishingPhaseController.Singleton.StartFishing();
        //}
    }
}
