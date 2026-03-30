//using UnityEngine;

//namespace TST
//{
//    /// <summary>
//    /// 씬 로드 시 UserDataModel 초기화 및 건물 복원 요청.
//    /// GameManager(앱 수명주기)와 역할이 다름.
//    /// </summary>
//    public class IdleBootstrapper : MonoBehaviour
//    {
//        void Start()
//        {
//            // 재화 및 기타 저장 데이터 초기화
//            UserDataModel.Singleton.Initialize();

//            // 건물 씬 복원 (GridPlacementController가 수행)
//            ProductionManager.Singleton?.RequestRebuild();
//        }
//    }
//}
