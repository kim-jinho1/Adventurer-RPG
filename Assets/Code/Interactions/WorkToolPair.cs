using System;
using UnityEngine;

namespace Code.Interactions
{
    /// <summary>
    /// 작업 종류와 그 작업에 드는 도구 한 쌍입니다. WorkToolModule 인스펙터에서 채웁니다.
    /// </summary>
    [Serializable]
    public struct WorkToolPair
    {
        [field: SerializeField] public WorkTypeSO WorkType { get; private set; }
        [field: SerializeField] public GameObject WorkTool { get; private set; }
    }
}
