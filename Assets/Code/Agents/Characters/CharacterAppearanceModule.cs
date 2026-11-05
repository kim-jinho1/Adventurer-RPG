using System;
using Code.GameSystems.Saves;
using Code.Modules;
using UnityEngine;

namespace Code.Agents.Characters
{
    /// <summary>
    /// 캐릭터의 성별 외형을 바꾸는 모듈입니다. 몸 · 방어구 메쉬 루트, 기본 머리카락, 애니메이션을 성별에 맞게 고릅니다.
    /// 능력치는 바꾸지 않고, 고른 성별은 저장 파일에 남습니다.
    /// </summary>
    public class CharacterAppearanceModule : MonoBehaviour, IModule, ISaveable
    {
        /// <summary>
        /// 저장 파일에 쓰는 외형 데이터(성별)입니다.
        /// </summary>
        [Serializable]
        private struct AppearanceSaveData
        {
            public GenderEnum Gender;
        }

        [Header("Mesh")]
        [SerializeField] private GameObject femaleRoot;     // 여자 몸 · 방어구 세트 루트 (Mesh/Female)
        [SerializeField] private GameObject maleRoot;       // 남자 몸 · 방어구 세트 루트 (Mesh/Male)

        [Header("Hair")]
        [SerializeField] private Transform hairRoot;        // 자식이 Hair_01 … (Mesh/Hair)
        [SerializeField, Min(1)] private int femaleHairIndex = 1;
        [SerializeField, Min(1)] private int maleHairIndex = 1;

        [Header("Animation")]
        [SerializeField] private RuntimeAnimatorController femaleController;
        [SerializeField] private RuntimeAnimatorController maleController;
        [SerializeField] private Avatar femaleAvatar;       // 성별마다 다리 자세 보정이 달라 아바타를 따로 쓴다
        [SerializeField] private Avatar maleAvatar;

        [field: Header("Save")]
        [field: SerializeField] public SaveIdSO SaveId { get; private set; }

        public GenderEnum Gender { get; private set; }

        private const string _hairPrefix = "Hair_";

        private Animator _animator;
        private IRenderer _renderer;

        public void Initialize(ModuleOwner owner)
        {
            _animator = owner.GetComponentInChildren<Animator>();
            _renderer = owner.GetModule<IRenderer>();
            Debug.Assert(femaleRoot != null, $"[CharacterAppearanceModule] femaleRoot 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(maleRoot != null, $"[CharacterAppearanceModule] maleRoot 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(_animator != null, $"[CharacterAppearanceModule] Animator 컴포넌트 없음: {gameObject.name}", this);

            // 처음 성별은 Character에 지정된 값을 따른다
            GenderEnum startGender = owner is Character character ? character.Gender : GenderEnum.Female;
            SetGender(startGender);
        }

        /// <summary>
        /// 성별을 바꾸고 외형에 바로 반영합니다.
        /// </summary>
        public void SetGender(GenderEnum gender)
        {
            Gender = gender;
            bool isFemale = gender == GenderEnum.Female;

            if (femaleRoot != null)
                femaleRoot.SetActive(isFemale);
            if (maleRoot != null)
                maleRoot.SetActive(!isFemale);

            ShowHair(isFemale ? femaleHairIndex : maleHairIndex);
            SetAnimator(isFemale ? femaleController : maleController, isFemale ? femaleAvatar : maleAvatar);
        }

        public string GetSaveData()
            => JsonUtility.ToJson(new AppearanceSaveData { Gender = Gender });

        public void RestoreData(string data)
        {
            AppearanceSaveData saveData = JsonUtility.FromJson<AppearanceSaveData>(data);
            SetGender(saveData.Gender);
        }

        // 번호가 맞는 머리카락 하나만 켜고 SkinHead_* 같은 다른 자식은 건드리지 않는다
        private void ShowHair(int hairIndex)
        {
            if (hairRoot == null)
                return;

            string hairName = $"{_hairPrefix}{hairIndex:00}";
            foreach (Transform hair in hairRoot)
            {
                if (hair.name.StartsWith(_hairPrefix))
                    hair.gameObject.SetActive(hair.name == hairName);
            }
        }

        private void SetAnimator(RuntimeAnimatorController controller, Avatar avatar)
        {
            if (_animator == null || controller == null)
                return;

            bool isAvatarChanged = avatar != null && _animator.avatar != avatar;
            if (_animator.runtimeAnimatorController == controller && !isAvatarChanged)
                return;

            // 컨트롤러나 아바타를 바꾸면 파라미터가 기본값으로 돌아가므로 (예: 공격 속도) 값을 옮겨 담는다
            AnimatorControllerParameter[] parameters = _animator.parameters;
            float[] floatValues = new float[parameters.Length];
            int[] intValues = new int[parameters.Length];
            bool[] boolValues = new bool[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                int hash = parameters[i].nameHash;
                switch (parameters[i].type)
                {
                    case AnimatorControllerParameterType.Float: floatValues[i] = _animator.GetFloat(hash); break;
                    case AnimatorControllerParameterType.Int: intValues[i] = _animator.GetInteger(hash); break;
                    case AnimatorControllerParameterType.Bool: boolValues[i] = _animator.GetBool(hash); break;
                }
            }

            if (isAvatarChanged)
                _animator.avatar = avatar;
            _animator.runtimeAnimatorController = controller;

            for (int i = 0; i < parameters.Length; i++)
            {
                int hash = parameters[i].nameHash;
                switch (parameters[i].type)
                {
                    case AnimatorControllerParameterType.Float: _animator.SetFloat(hash, floatValues[i]); break;
                    case AnimatorControllerParameterType.Int: _animator.SetInteger(hash, intValues[i]); break;
                    case AnimatorControllerParameterType.Bool: _animator.SetBool(hash, boolValues[i]); break;
                }
            }

            // 컨트롤러나 아바타가 바뀌면 기본 상태부터 다시 시작하므로, FSM이 틀고 있던 상태로 되돌린다
            if (_renderer != null && _renderer.CurrentStateHash != 0)
                _renderer.PlayClip(_renderer.CurrentStateHash, 0, 0f);
        }
    }
}
