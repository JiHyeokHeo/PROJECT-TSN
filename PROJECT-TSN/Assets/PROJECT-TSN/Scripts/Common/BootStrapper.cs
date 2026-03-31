using UnityEngine;

namespace TST
{
    /// <summary>
    /// TSN 게임 시작 초기화 진입점.
    /// 씬에 하나만 배치하십시오. 다른 모든 Start() 완료를 보장하기 위해
    /// Script Execution Order에서 Default보다 늦게 실행되도록 설정하십시오.
    ///   Project Settings > Script Execution Order > BootStrapper > 100
    ///
    /// 초기화 순서:
    ///   1. UIManager  — Panel Root, Popup Root, UICamera 생성
    ///   2. SoundManager — 저장된 볼륨값 복원
    ///   3. Panel_Title  — 타이틀 화면 표시
    /// </summary>
    public class BootStrapper : MonoBehaviour
    {
        private void Start()
        {
            UIManager.Singleton.Initialize();
            SoundManager.Singleton.Initialize();

            UIManager.Show<TitleUI>(UIList.Panel_Title);
        }
    }
}
