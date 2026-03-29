using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    // 게임에서, Main이 관리하는 씬에 대한 인터페이스를 내포한 추상 부모 클래스
    public abstract class SceneBase : MonoBehaviour
    {
        public abstract IEnumerator OnStart();

        public abstract IEnumerator OnEnd();
    }
}
