using System;
using Code.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 인벤토리 탭 버튼 하나의 설정입니다. 어떤 종류를 보여 줄지와 버튼 · 배경 이미지를 묶습니다.
    /// </summary>
    [Serializable]
    public class InventoryTabData
    {
        [field: SerializeField] public ItemTypeEnum ItemType { get; private set; }
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public Image Background { get; private set; }

        public InventoryTabData(ItemTypeEnum itemType, Button button, Image background)
        {
            ItemType = itemType;
            Button = button;
            Background = background;
        }
    }
}
