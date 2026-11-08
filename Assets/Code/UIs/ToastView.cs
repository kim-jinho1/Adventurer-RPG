using System.Collections.Generic;
using Code.CoreSystem.Bus;
using Code.Craftings;
using Code.Interactions;
using Code.Inventories;
using Code.Levels;
using Code.Modules;
using TMPro;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 아이템 획득 · 가방 가득 참 · 제작 완료 · 레벨업을 화면에 잠깐 띄웠다가 흐리게 사라지게 합니다.
    /// </summary>
    public class ToastView : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private ModuleOwner player;
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        [Header("View")]
        [SerializeField] private TextMeshProUGUI lineTemplate;
        [SerializeField] private Transform lineContainer;
        [SerializeField, Min(1)] private int maxLineCount = 4;
        [SerializeField] private float showDuration = 2f;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color acquireColor = Color.blanchedAlmond;
        [SerializeField] private Color warningColor = Color.indianRed;
        [SerializeField] private Color levelUpColor = Color.yellowNice;
        [SerializeField] private Color craftColor = new(0.6f, 0.85f, 1f);

        [Header("Texts")]
        [SerializeField] private string acquireFormat = "{0} x{1} 획득";           // {0} 아이템 이름, {1} 개수
        [SerializeField] private string inventoryFullMessage = "가방이 가득 찼습니다";
        [SerializeField] private string craftFormat = "{0} 제작 완료!";            // {0} 결과 아이템 이름
        [SerializeField] private string levelUpFormat = "레벨 업! Lv.{0}";         // {0} 새 레벨

        private readonly List<TextMeshProUGUI> _lines = new();
        private readonly List<float> _remainTimes = new();

        private BusSubscriptionScope _busScope;

        private ILevelModule _levelModule;
        private ICraftingModule _craftingModule;
        private IWorkModule _workModule;

        private RecipeSO _pendingRecipe;
        private CraftingStation _pendingStation;

        private void Start()
        {
            Debug.Assert(lineTemplate != null, $"[ToastView] lineTemplate 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(lineContainer != null, $"[ToastView] lineContainer 연결 안 됨: {gameObject.name}", this);
            if (lineTemplate == null || lineContainer == null)
                return;

            lineTemplate.gameObject.SetActive(false);
            for (int i = 0; i < maxLineCount; i++)
            {
                TextMeshProUGUI line = Instantiate(lineTemplate, lineContainer);
                line.gameObject.SetActive(false);
                _lines.Add(line);
                _remainTimes.Add(0f);
            }

            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<ItemAcquiredEvent>(eventOwner, HandleItemAcquired);
            _busScope.Subscribe<InventoryFullEvent>(eventOwner, HandleInventoryFull);

            if (player != null)
            {
                _levelModule = player.GetModule<ILevelModule>();
                _craftingModule = player.GetModule<ICraftingModule>();
                _workModule = player.GetModule<IWorkModule>();
            }

            if (_levelModule != null)
                _levelModule.OnLevelUp += HandleLevelUp;

            if (_craftingModule != null)
                _craftingModule.OnCrafted += HandleCrafted;

            if (_workModule != null)
                _workModule.OnWorkCompleted += HandleWorkCompleted;
        }

        private void OnDestroy()
        {
            _busScope?.Dispose();
            if (_levelModule != null)
                _levelModule.OnLevelUp -= HandleLevelUp;

            if (_craftingModule != null)
                _craftingModule.OnCrafted -= HandleCrafted;

            if (_workModule != null)
                _workModule.OnWorkCompleted -= HandleWorkCompleted;
        }

        private void Update()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                if (_remainTimes[i] <= 0f)
                    continue;

                _remainTimes[i] -= Time.unscaledDeltaTime;
                TextMeshProUGUI line = _lines[i];
                if (_remainTimes[i] <= 0f)
                {
                    line.gameObject.SetActive(false);
                    continue;
                }

                Color color = line.color;
                color.a = Mathf.Clamp01(_remainTimes[i] / fadeDuration);
                line.color = color;
            }
        }

        /// <summary>
        /// 문구를 한 줄 띄웁니다. 줄이 모두 차 있으면 가장 오래된 줄을 다시 씁니다.
        /// </summary>
        public void ShowMessage(string message, Color color)
        {
            if (_lines.Count == 0)
                return;

            int index = FindReusableLine();
            TextMeshProUGUI line = _lines[index];
            line.text = message;
            line.color = color;
            line.gameObject.SetActive(true);
            // 새 줄이 항상 아래에 오도록 순서를 맨 뒤로 보낸다
            line.transform.SetAsLastSibling();
            _remainTimes[index] = showDuration + fadeDuration;
        }

        private int FindReusableLine()
        {
            int oldestIndex = 0;
            for (int i = 0; i < _remainTimes.Count; i++)
            {
                if (_remainTimes[i] <= 0f)
                    return i;

                if (_remainTimes[i] < _remainTimes[oldestIndex])
                    oldestIndex = i;
            }

            return oldestIndex;
        }

        private void HandleItemAcquired(ItemAcquiredEvent evt)
        {
            if (evt.Item != null)
                ShowMessage(string.Format(acquireFormat, evt.Item.ItemName, evt.Count), acquireColor);
        }

        private void HandleInventoryFull(InventoryFullEvent evt)
        {
            ShowMessage(inventoryFullMessage, warningColor);
        }

        private void HandleWorkCompleted(WorkRequest work)
        {
            // 채집처럼 제작대가 아닌 작업이 끝났거나, 메모한 제작이 없으면 넘어간다
            if (_pendingRecipe == null || work.Target is not CraftingStation station || station != _pendingStation)
                return;

            ShowMessage(string.Format(craftFormat, _pendingRecipe.Result.Item.ItemName), craftColor);
            _pendingRecipe = null;
            _pendingStation = null;
        }

        private void HandleCrafted(RecipeSO recipe, CraftingStation station)
        {
            // 아직 망치질 전이라 메모만 해 두고, 끝나면 HandleWorkCompleted가 띄운다
            _pendingRecipe = recipe;
            _pendingStation = station;
        }

        private void HandleLevelUp(int level)
        {
            ShowMessage(string.Format(levelUpFormat, level), levelUpColor);
        }
    }
}
