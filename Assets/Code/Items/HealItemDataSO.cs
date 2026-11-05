using Code.Agents;
using Code.Modules;
using UnityEngine;

namespace Code.Items
{
    /// <summary>
    /// 체력을 회복하는 소모품입니다. (회복 포션 등)
    /// </summary>
    [CreateAssetMenu(fileName = "HealItem", menuName = "SO/Item/Heal item", order = 3)]
    public class HealItemDataSO : AbstractConsumableItemDataSO
    {
        [field: Header("Heal Info")]
        [field: SerializeField, Min(0f)] public float HealAmount { get; private set; } = 20f;

        public override bool CanUse(ModuleOwner user)
        {
            if (!base.CanUse(user))
                return false;

            // 체력이 가득일 때 쓰면 포션만 사라진다
            IHealthModule healthModule = user.GetModule<IHealthModule>();
            return healthModule != null && healthModule.CurrentHealth > 0f && healthModule.CurrentHealth < healthModule.MaxHealth;
        }

        public override void Use(ModuleOwner user)
        {
            IHealthModule healthModule = user.GetModule<IHealthModule>();
            healthModule?.Heal(HealAmount);
        }

        public override string GetDescription()
            => $"{Description}\n체력 +{HealAmount}";
    }
}
