using UnityEngine;

namespace Code.Modules
{
    /// <summary>
    /// 마지막으로 받은 피격 정보를 담아 두는 모듈입니다. 피격 상태·이펙트·데미지 숫자가 여기서 읽어 갑니다.
    /// </summary>
    public class ActionDataModule : MonoBehaviour, IModule
    {
        [field: SerializeField] public Vector3 LastHitPoint { get; set; }           // 맞은 위치
        [field: SerializeField] public Vector3 LastHitDirection { get; set; }       // 공격이 들어온 방향
        [field: SerializeField] public Vector3 LastHitNormal { get; set; }          // 맞은 면의 법선
        [field: SerializeField] public bool IsLastHitCritical { get; set; }         // 크리티컬 여부
        [field: SerializeField] public bool IsLastHitStagger { get; set; }          // 강공격(경직) 여부
        [field: SerializeField] public Vector3 LastKnockbackForce { get; set; }     // 월드 기준 넉백 힘
        [field: SerializeField] public float LastDamage { get; set; }               // 받은 데미지

        private ModuleOwner _owner;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
        }
    }
}
