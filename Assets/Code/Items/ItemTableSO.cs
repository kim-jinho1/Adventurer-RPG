using System.Collections.Generic;
using UnityEngine;

namespace Code.Items
{
    /// <summary>
    /// 게임에 있는 모든 아이템을 모아 AssetIndex로 찾게 해 주는 테이블입니다. 저장 · 불러오기에서 인덱스를 아이템으로 되돌릴 때 씁니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemTable", menuName = "SO/Item/Item table", order = 0)]
    public class ItemTableSO : ScriptableObject
    {
        [SerializeField] private List<AbstractItemDataSO> items = new();

        /// <summary> 등록된 아이템 목록입니다. </summary>
        public IReadOnlyList<AbstractItemDataSO> Items => items;

        private Dictionary<int, AbstractItemDataSO> _itemDict;

        private void OnEnable()
        {
            BuildTable();
        }

        /// <summary> AssetIndex로 아이템을 찾습니다. 없으면 null입니다. </summary>
        public AbstractItemDataSO GetItemData(int assetIndex)
            => TryGetItemData(assetIndex, out AbstractItemDataSO item) ? item : null;

        /// <summary> AssetIndex로 아이템을 찾습니다. 없으면 false를 반환합니다. </summary>
        public bool TryGetItemData(int assetIndex, out AbstractItemDataSO item)
        {
            if (_itemDict == null)
                BuildTable();

            return _itemDict.TryGetValue(assetIndex, out item);
        }

        private void BuildTable()
        {
            _itemDict = new Dictionary<int, AbstractItemDataSO>(items.Count);
            foreach (AbstractItemDataSO item in items)
            {
                if (item == null)
                    continue;

                // 중복이면 먼저 등록된 것을 쓴다 (경고는 OnValidate)
                _itemDict.TryAdd(item.AssetIndex, item);
            }
        }

        private void OnValidate()
        {
            HashSet<int> indices = new();
            foreach (AbstractItemDataSO item in items)
            {
                if (item != null && !indices.Add(item.AssetIndex))
                    Debug.LogWarning($"[ItemTableSO] AssetIndex {item.AssetIndex} 중복 ({item.name}): {name}", this);
            }

            BuildTable();
        }
    }
}
