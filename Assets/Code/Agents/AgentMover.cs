using Code.Agents.Stats;
using Code.Modules;
using UnityEngine;

namespace Code.Agents
{
    /// <summary>
    /// AgentMover 클래스는 Agent의 이동과 관련된 기능을 관리합니다.
    /// </summary>
    public class AgentMover : MonoBehaviour, IModule, IMover
    {
        [Header("Mover")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float rotationSpeed = 8f;

        [Header("Movement")]
        [SerializeField] private StatSO moveSpeedStat;

        public bool CanManualMovement { get; set; } = true;
        public bool IsRotationLocked { get; set; } = false;

        public CharacterController CharacterController => characterController;
        public Vector3 Velocity => _velocity;    // 이번 프레임 이동 속도

        private bool IsGround => characterController.isGrounded;

        private Agent _owner;
        private IStatModule _statModule;

        private Vector3 _autoMovement;

        private Vector3 _velocity;
        private Vector3 _movementDirection;

        private float _verticalVelocity;
        private float _moveSpeed = 6f;
        private float _moveSpeedMultiplier = 1f;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            if (characterController == null)
                characterController = owner.GetComponent<CharacterController>();

            _statModule = owner.GetModule<IStatModule>();
        }

        private void Start()
        {
            _moveSpeed = _statModule.SubscribeStat(moveSpeedStat.AssetIndex, HandleMoveSpeedChange, 1);
        }

        private void FixedUpdate()
        {
            CalculateMovement();
            ApplyGravity();
            characterController.Move(_velocity * Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            if (_statModule != null)
                _statModule.UnSubscribeStat(moveSpeedStat.AssetIndex, HandleMoveSpeedChange);
        }

        public void SetMovementDirection(Vector3 direction)
        {
            _movementDirection = direction;
        }

        public void SetMoveSpeedMultiplier(float value)
            => _moveSpeedMultiplier = value;

        public void StopImmediately()
        {
            _autoMovement = Vector3.zero;
            _movementDirection = Vector3.zero;
        }

        public void SetAutoMovement(Vector3 velocity)
        {
            _autoMovement = velocity;
        }

        private void HandleMoveSpeedChange(StatSO stat, float currentValue, float prevValue)
            => _moveSpeed = currentValue;

        private void CalculateMovement()
        {
            if (CanManualMovement)
                _velocity = _movementDirection * (_moveSpeed * _moveSpeedMultiplier);
            else
                _velocity = _autoMovement;

            if (_velocity.sqrMagnitude > 0.0001f && !IsRotationLocked)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(_velocity.x, 0, _velocity.z));
                Transform parent = _owner.transform;
                parent.rotation = Quaternion.Lerp(parent.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            }
        }

        private void ApplyGravity()
        {
            if (IsGround && _verticalVelocity < 0)
                _verticalVelocity = -0.03f;
            else
                _verticalVelocity += gravity * Time.fixedDeltaTime;

            _velocity.y = _verticalVelocity;
        }
    }
}
