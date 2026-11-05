using UnityEngine;

namespace Code.Agents
{
    /// <summary>
    /// IMover 인터페이스는 이동 관련 기능을 정의합니다.
    /// </summary>
    public interface IMover
    {
        /// <summary> 이동에 쓰는 CharacterController </summary>
        CharacterController CharacterController { get; }

        /// <summary> 입력 방향으로 움직일지 여부 (끄면 자동 이동만) </summary>
        bool CanManualMovement { get; set; }
        /// <summary> 이동 방향으로 돌지 않게 잠갔는지 여부 </summary>
        bool IsRotationLocked { get; set; }

        /// <summary> 이동 속도 배율을 설정합니다. </summary>
        void SetMoveSpeedMultiplier(float value);
        /// <summary> 이동 방향을 설정합니다. 실제 속도는 이 값에 이동 속도와 배율을 곱한 값입니다. </summary>
        void SetMovementDirection(Vector3 direction);

        /// <summary> 이동을 즉시 중지합니다. </summary>
        void StopImmediately();

        /// <summary> 수동 이동이 꺼져 있을 때 적용할 자동 이동 속도(m/s)를 설정합니다 </summary>
        void SetAutoMovement(Vector3 velocity);
    }
}
