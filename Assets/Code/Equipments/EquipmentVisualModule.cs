using System;
using System.Collections.Generic;
using Code.Items;
using Code.Modules;
using UnityEngine;

namespace Code.Equipments
{
    /// <summary>
    /// 장착한 장비에 맞춰 캐릭터 메쉬를 켜고 끕니다. 메쉬는 처음에 한 번 이름 규칙(Sword_00N, Armor_00N/..._Head)으로 찾아 둡니다.
    /// 부위 이름 끝과 빈 칸일 때 보여 줄 번호는 EquipSlotTableSO가 정합니다.
    /// </summary>
    public class EquipmentVisualModule : MonoBehaviour, IModule
    {
        [Header("Weapon")]
        [SerializeField] private Transform[] weaponGroups;      // 같이 바뀌는 무기 묶음 (예: Sword, Shield). 자식이 Sword_001 …

        [Header("Armor")]
        [SerializeField] private Transform[] armorRoots;        // 성별별 메쉬 루트 (Female, Male). 자식이 Armor_001 …

        [Header("Slot")]
        [SerializeField] private EquipSlotTableSO slotTable;   // 부위 이름 끝 · 빈 칸일 때 메쉬 번호

        // 슬롯 → 메쉬 번호 → 함께 켜고 끌 오브젝트 (검 + 방패, 여자 + 남자 부위)
        private readonly Dictionary<EquipSlotEnum, Dictionary<int, List<GameObject>>> _meshes = new();
        // 슬롯마다 지금 켜 둔 메쉬 번호 (바꿀 때 이것만 끈다)
        private readonly Dictionary<EquipSlotEnum, int> _shownMeshIndices = new();

        private const string _armorSetPrefix = "Armor_";

        private IEquipmentModule _equipmentModule;

        public void Initialize(ModuleOwner owner)
        {
            _equipmentModule = owner.GetModule<IEquipmentModule>();
            Debug.Assert(_equipmentModule != null, $"[EquipmentVisualModule] IEquipmentModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(slotTable != null, $"[EquipmentVisualModule] slotTable 연결 안 됨: {gameObject.name}", this);
            CacheMeshes();
            if (_equipmentModule == null)
                return;

            _equipmentModule.OnEquipmentChanged += HandleEquipmentChanged;
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_equipmentModule != null)
                _equipmentModule.OnEquipmentChanged -= HandleEquipmentChanged;
        }

        private static bool TryParseMeshIndex(string meshName, out int meshIndex)
        {
            return int.TryParse(meshName.Substring(meshName.LastIndexOf('_') + 1), out meshIndex);
        }

        private void HandleEquipmentChanged(EquipSlotEnum slot, EquipItemDataSO prevItem, EquipItemDataSO currentItem)
        {
            RefreshSlot(slot);
        }

        private void RefreshAll()
        {
            foreach (EquipSlotEnum slot in Enum.GetValues(typeof(EquipSlotEnum)))
                RefreshSlot(slot);
        }

        private void RefreshSlot(EquipSlotEnum slot)
        {
            EquipItemDataSO item = _equipmentModule.GetEquippedItem(slot);
            // 투구 메쉬가 머리에 붙는 형태라 머리카락은 투구를 써도 그대로 둔다
            int meshIndex = item != null ? item.MeshIndex : slotTable.GetEmptyMeshIndex(slot);
            bool isShown = ShowMesh(slot, meshIndex);

            // 빈 칸은 켤 메쉬가 없을 수 있으니(무기 0번) 장비를 끼고도 못 찾았을 때만 이름 규칙이 어긋난 것이다
            if (!isShown && item != null)
                Debug.LogWarning(
                    $"[EquipmentVisualModule] 장비 메쉬 찾지 못함 ({slot} · MeshIndex {item.MeshIndex} · {item.name}): {gameObject.name}", this);
        }

        // 지금 켜 둔 번호의 메쉬를 끄고 새 번호의 메쉬를 켠 뒤, 켤 메쉬를 찾았는지 돌려준다
        private bool ShowMesh(EquipSlotEnum slot, int meshIndex)
        {
            bool hasShown = _shownMeshIndices.TryGetValue(slot, out int shownIndex);
            if (hasShown && shownIndex == meshIndex)
                return true;

            if (hasShown)
                SetMeshesActive(slot, shownIndex, false);

            _shownMeshIndices[slot] = meshIndex;
            return SetMeshesActive(slot, meshIndex, true);
        }

        private bool SetMeshesActive(EquipSlotEnum slot, int meshIndex, bool isActive)
        {
            if (!TryGetMeshes(slot, meshIndex, out List<GameObject> meshes))
                return false;

            foreach (GameObject mesh in meshes)
                mesh.SetActive(isActive);

            return true;
        }

        private bool TryGetMeshes(EquipSlotEnum slot, int meshIndex, out List<GameObject> meshes)
        {
            meshes = null;
            return _meshes.TryGetValue(slot, out Dictionary<int, List<GameObject>> meshesByIndex)
                && meshesByIndex.TryGetValue(meshIndex, out meshes);
        }

        private void AddMesh(EquipSlotEnum slot, int meshIndex, GameObject mesh)
        {
            if (!_meshes.TryGetValue(slot, out Dictionary<int, List<GameObject>> meshesByIndex))
            {
                meshesByIndex = new Dictionary<int, List<GameObject>>();
                _meshes[slot] = meshesByIndex;
            }

            if (!meshesByIndex.TryGetValue(meshIndex, out List<GameObject> meshes))
            {
                meshes = new List<GameObject>();
                meshesByIndex[meshIndex] = meshes;
            }

            meshes.Add(mesh);
        }

        // 메쉬 이름은 실행 중에 바뀌지 않으므로 처음에 한 번만 훑어 번호별로 담아 두고 전부 꺼 둔다
        private void CacheMeshes()
        {
            // 무기 묶음의 자식은 같은 번호끼리 함께 켜진다 (Sword_005 + Shield_005)
            foreach (Transform group in weaponGroups)
            {
                foreach (Transform mesh in group)
                {
                    if (!TryParseMeshIndex(mesh.name, out int meshIndex))
                        continue;

                    AddMesh(EquipSlotEnum.Weapon, meshIndex, mesh.gameObject);
                    mesh.gameObject.SetActive(false);
                }
            }

            if (slotTable == null)
                return;

            foreach (Transform armorRoot in armorRoots)
            {
                foreach (Transform armorSet in armorRoot)
                {
                    if (!armorSet.name.StartsWith(_armorSetPrefix) || !TryParseMeshIndex(armorSet.name, out int setIndex))
                        continue;

                    // 부위만 켜고 끄므로 세트 오브젝트는 항상 켜 둔다
                    armorSet.gameObject.SetActive(true);
                    foreach (Transform part in armorSet)
                        CacheArmorPart(part, setIndex);
                }
            }
        }

        // 부위 이름 끝이 표의 어느 슬롯과 맞는지 찾고, 머리카락처럼 어느 슬롯과도 안 맞는 자식은 건드리지 않는다
        private void CacheArmorPart(Transform part, int setIndex)
        {
            foreach (EquipSlotRule rule in slotTable.Rules)
            {
                if (string.IsNullOrEmpty(rule.MeshPartSuffix) || !part.name.EndsWith(rule.MeshPartSuffix))
                    continue;

                AddMesh(rule.Slot, setIndex, part.gameObject);
                part.gameObject.SetActive(false);
                return;
            }
        }
    }
}
