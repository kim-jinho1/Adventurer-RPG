using Code.Combats;
using Code.Modules;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Debugs
{
    /// <summary>
    /// 지정한 키를 누르면 자기 자신에게 데미지를 주는 디버그 도구입니다. 에이전트 루트에 붙입니다.
    /// </summary>
    public class CharacterSelfHit : MonoBehaviour
    {
        [SerializeField] private Key hitKey = Key.H;
        [SerializeField] private float damage = 10f;
        [SerializeField] private Vector3 knockBackForce;

        private IDamageable _damageable;
        private ModuleOwner _owner;

        private void Awake()
        {
            // 실제 공격(OverlapDamageCaster)과 같은 경로로 맞도록 IDamageable을 찾는다
            _damageable = GetComponent<IDamageable>();
            _owner = GetComponent<ModuleOwner>();
            Debug.Assert(_damageable != null, $"[CharacterSelfHit] IDamageable 컴포넌트 없음: {gameObject.name}", this);
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return; // 키보드가 없는 환경(모바일)에서는 null이다

            if (Keyboard.current[hitKey].wasPressedThisFrame)
                HitSelf(); // isPressed는 누르고 있는 동안 매 프레임 true라 한 번 누르면 여러 번 맞는다
        }

        private void HitSelf()
        {
            DamageData damageData = new DamageData
            {
                DamageAmount = damage,
                Dealer = _owner,
                IsCritical = false,
                CausesStagger = true, // 피격 상태를 확인하는 도구라 경직을 건다
                KnockBackForce = transform.TransformDirection(knockBackForce)
            };

            // 정면에서 맞은 것으로 친다
            Vector3 hitDirection = -transform.forward;
            Vector3 hitPoint = transform.position + Vector3.up;
            _damageable.ApplyDamage(damageData, hitPoint, hitDirection, transform.forward);
        }
    }
}
