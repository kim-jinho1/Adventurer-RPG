using Code.Combats;
using Code.Modules;
using UnityEngine;
using UnityEngine.Animations;

namespace Code.Agents.Characters
{
    /// <summary>
    /// 캐릭터의 무기 모듈을 나타내는 클래스입니다.
    /// </summary>
    public class CharacterWeaponModule : MonoBehaviour, IModule, IWeaponModule
    {
        // 같이 뽑고 넣는 장비들 (예: 한손검 + 방패, 소켓 0번 = 손 · 1번 = 등, 켤 메쉬는 EquipmentVisualModule이 정함)
        [SerializeField] private ParentConstraint[] weaponConstraints;
        // 등에 메면 뛸 때 흔드는 팔이 뚫고 지나가서 넣을 때도 손에 남기는 장비 (예: 방패, weaponConstraints에도 같이 넣음)
        [SerializeField] private ParentConstraint[] stayInHandConstraints;

        public bool IsDrawn { get; private set; } = false;

        private const int _handSourceIndex = 0; // ParentConstraint 0번 소스 = 손 소켓
        private const int _backSourceIndex = 1; // ParentConstraint 1번 소스 = 등 소켓

        public void Initialize(ModuleOwner owner)
        {
            Sheathe();
        }

        /// <summary>
        /// 무기를 손 소켓으로 옮깁니다.
        /// </summary>
        public void Draw()
        {
            IsDrawn = true;
            SetSourceWeights(1f, 0f);
        }

        /// <summary>
        /// 무기를 등 소켓으로 옮깁니다. 손에 남는 장비는 손 소켓에 그대로 둡니다.
        /// </summary>
        public void Sheathe()
        {
            IsDrawn = false;
            SetSourceWeights(0f, 1f);

            foreach (ParentConstraint stayInHandConstraint in stayInHandConstraints)
                SetSourceWeights(stayInHandConstraint, 1f, 0f);
        }

        /// <summary>
        /// 무기 메쉬를 잠깐 숨기거나 다시 보여 줍니다. 소켓 위치는 바꾸지 않습니다.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            foreach (ParentConstraint weaponConstraint in weaponConstraints)
                weaponConstraint.gameObject.SetActive(isVisible);
        }

        /// <summary>
        /// 손 · 등 소켓의 가중치를 함께 바꿉니다. 한쪽만 바꾸면 두 소켓 사이에 무기가 뜬다.
        /// </summary>
        private void SetSourceWeights(float handWeight, float backWeight)
        {
            foreach (ParentConstraint weaponConstraint in weaponConstraints)
                SetSourceWeights(weaponConstraint, handWeight, backWeight);
        }

        private void SetSourceWeights(ParentConstraint weaponConstraint, float handWeight, float backWeight)
        {
            // ConstraintSource는 struct라 꺼내서 고친 뒤 다시 넣어야 반영된다
            ConstraintSource hand = weaponConstraint.GetSource(_handSourceIndex);
            hand.weight = handWeight;
            weaponConstraint.SetSource(_handSourceIndex, hand);

            ConstraintSource back = weaponConstraint.GetSource(_backSourceIndex);
            back.weight = backWeight;
            weaponConstraint.SetSource(_backSourceIndex, back);
        }
    }
}
