using System.Collections.Generic;
using Code.Modules;
using UnityEngine;

namespace Code.Interactions
{
    /// <summary>
    /// 작업하는 동안 손에 드는 도구(곡괭이, 도끼, 망치 등)를 보여 줍니다. 작업 종류와 도구를 짝으로 넣고, 맨손 작업은 넣지 않습니다.
    /// </summary>
    public class WorkToolModule : MonoBehaviour, IModule, IWorkToolModule
    {
        [Header("Work Tool Setting")]
        [SerializeField] private WorkToolPair[] workTools;

        private Dictionary<WorkTypeSO, GameObject> _toolsByWorkType;

        public void Initialize(ModuleOwner owner)
        {
            _toolsByWorkType = new Dictionary<WorkTypeSO, GameObject>();
            foreach (WorkToolPair toolPair in workTools)
            {
                // null은 딕셔너리 키로 못 쓰므로 인스펙터에서 비워 둔 칸은 건너뛴다
                if (toolPair.WorkType == null || toolPair.WorkTool == null)
                    continue;

                bool isAdded = _toolsByWorkType.TryAdd(toolPair.WorkType, toolPair.WorkTool);
                Debug.Assert(isAdded, $"[WorkToolModule] 작업 종류 {toolPair.WorkType.name} 중복: {gameObject.name}", this);
            }

            HideAll();
        }

        /// <summary>
        /// 작업 종류에 맞는 도구 하나만 보여 줍니다. 도구가 없는 작업(맨손)이면 모두 숨깁니다.
        /// </summary>
        public void Show(WorkTypeSO workType)
        {
            HideAll();
            if (workType != null && _toolsByWorkType.TryGetValue(workType, out GameObject tool))
                tool.SetActive(true);
        }

        /// <summary>
        /// 모든 도구를 숨깁니다.
        /// </summary>
        public void HideAll()
        {
            foreach (GameObject tool in _toolsByWorkType.Values)
                tool.SetActive(false);
        }
    }
}
