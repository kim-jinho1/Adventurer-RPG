using System.Collections.Generic;
using UnityEngine;

namespace Code.Items.Drops
{
    /// <summary>
    /// 적 처치 · 채집으로 나오는 아이템 목록과 경험치입니다. 줄마다 따로 확률을 굴립니다.
    /// </summary>
    [CreateAssetMenu(fileName = "DropTable", menuName = "SO/Item/Drop table", order = 10)]
    public class DropTableSO : ScriptableObject
    {
        [field: Header("Reward")]
        [field: SerializeField, Min(0)] public int Exp { get; private set; }
        [SerializeField] private List<DropEntry> drops = new();

        /// <summary> 드랍 목록입니다. </summary>
        public IReadOnlyList<DropEntry> Drops => drops;

        /// <summary>
        /// 확률을 굴려 나온 아이템을 results에 채웁니다. 매번 리스트를 만들지 않도록 호출하는 쪽이 재사용할 리스트를 넘깁니다.
        /// </summary>
        public void Roll(List<ItemAmount> results)
        {
            results.Clear();
            foreach (DropEntry drop in drops)
            {
                if (drop == null || drop.Item == null || Random.value > drop.Chance)
                    continue;

                int count = Random.Range(drop.MinCount, drop.MaxCount + 1);
                if (count > 0)
                    results.Add(new ItemAmount(drop.Item, count));
            }
        }

        private void OnValidate()
        {
            foreach (DropEntry drop in drops)
                drop?.Validate();
        }
    }
}
