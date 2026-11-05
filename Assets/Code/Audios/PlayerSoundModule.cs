using Ami.BroAudio;
using Code.CoreSystem.Bus;
using Code.Craftings;
using Code.Equipments;
using Code.Inventories;
using Code.Items;
using Code.Levels;
using Code.Modules;
using UnityEngine;

namespace Code.Audios
{
    /// <summary>
    /// 플레이어 성장 · 가방에서 나는 소리를 냅니다. 레벨업 · 제작 · 장착 · 아이템 사용 · 획득 · 가방 가득 참.
    /// 화면 가까이에서 나는 알림이라 위치 없이(2D) 재생합니다.
    /// </summary>
    public class PlayerSoundModule : MonoBehaviour, IModule, IAfterInitModule
    {
        [Header("Progress")]
        [SerializeField] private SoundID levelUpSound;
        [SerializeField] private SoundID craftedSound;

        [Header("Items")]
        [SerializeField] private SoundID equipSound;
        [SerializeField] private SoundID itemUseSound;
        [SerializeField] private SoundID itemAcquiredSound;
        [SerializeField] private SoundID inventoryFullSound;
        [SerializeField, Min(0f)] private float equipSoundInterval = 0.2f;    // 불러오기로 여러 칸이 한꺼번에 바뀌어도 한 번만 낸다

        [Header("Event")]
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        private ILevelModule _levelModule;
        private ICraftingModule _craftingModule;
        private IEquipmentModule _equipmentModule;
        private IItemUseModule _itemUseModule;
        private BusSubscriptionScope _busScope;
        private float _nextEquipSoundTime;

        public void Initialize(ModuleOwner owner)
        {
            _levelModule = owner.GetModule<ILevelModule>();
            _craftingModule = owner.GetModule<ICraftingModule>();
            _equipmentModule = owner.GetModule<IEquipmentModule>();
            _itemUseModule = owner.GetModule<IItemUseModule>();
        }

        public void AfterInitialize()
        {
            if (_levelModule != null)
                _levelModule.OnLevelUp += HandleLevelUp;
            if (_craftingModule != null)
                _craftingModule.OnCrafted += HandleCrafted;
            if (_equipmentModule != null)
                _equipmentModule.OnEquipmentChanged += HandleEquipmentChanged;
            if (_itemUseModule != null)
                _itemUseModule.OnItemUsed += HandleItemUsed;

            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<ItemAcquiredEvent>(eventOwner, HandleItemAcquired);
            _busScope.Subscribe<InventoryFullEvent>(eventOwner, HandleInventoryFull);
        }

        private void OnDestroy()
        {
            if (_levelModule != null)
                _levelModule.OnLevelUp -= HandleLevelUp;
            if (_craftingModule != null)
                _craftingModule.OnCrafted -= HandleCrafted;
            if (_equipmentModule != null)
                _equipmentModule.OnEquipmentChanged -= HandleEquipmentChanged;
            if (_itemUseModule != null)
                _itemUseModule.OnItemUsed -= HandleItemUsed;

            _busScope?.Dispose();
        }

        private void HandleLevelUp(int level)
            => Play(levelUpSound);

        private void HandleCrafted(RecipeSO recipe, CraftingStation station)
            => Play(craftedSound);

        private void HandleEquipmentChanged(EquipSlotEnum slot, EquipItemDataSO prevItem, EquipItemDataSO currentItem)
        {
            if (Time.unscaledTime < _nextEquipSoundTime)
                return;

            _nextEquipSoundTime = Time.unscaledTime + equipSoundInterval;
            Play(equipSound);
        }

        private void HandleItemUsed(AbstractItemDataSO item)
            => Play(itemUseSound);

        private void HandleItemAcquired(ItemAcquiredEvent evt)
            => Play(itemAcquiredSound);

        private void HandleInventoryFull(InventoryFullEvent evt)
            => Play(inventoryFullSound);

        private static void Play(SoundID sound)
        {
            if (sound.IsValid())
                BroAudio.Play(sound);
        }
    }
}
