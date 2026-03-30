using UnityEngine;

namespace TST
{
    /// <summary>
    /// 관측선 발밑 원형 방향 포인터.
    /// 바닥(XZ 평면)과 수평하게 눕혀진 스프라이트를 회전시켜
    /// 현재 이동 방향을 가리킵니다.
    ///
    /// 세팅 방법:
    ///   - 이 컴포넌트를 붙인 GameObject에 SpriteRenderer (원형 화살표 스프라이트)를 연결합니다.
    ///   - 로컬 Rotation을 (90, 0, 0) 으로 설정해 바닥면에 눕힙니다.
    ///   - VesselController 자식으로 배치합니다.
    /// </summary>
    public class VesselDirectionPointer : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (VesselController.Singleton == null) return;

            // X=90° 로 바닥에 눕히고, Y만 이동 방향으로 회전
            transform.rotation = Quaternion.Euler(90f, VesselController.Singleton.FacingAngle, 0f);
        }
    }
}
