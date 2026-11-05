using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Agents.FSM.Editor
{
    /// <summary>
    /// StateListSO 인스펙터입니다. 버튼으로 상태 번호를 매기고 상태 열거형 파일을 생성합니다.
    /// </summary>
    [CustomEditor(typeof(StateListSO))]
    public class StateListSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset stateListView = default;

        private const string _characterEnumPath = "Assets/Code/Agents/Characters/FSM/";

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            stateListView.CloneTree(root);

            root.Q<Button>("GenerateButton").clicked += HandleGenerateButtonClick;

            return root;
        }

        private void HandleGenerateButtonClick()
        {
            StateListSO list = target as StateListSO;

            int index = 0;
            // .cs 규칙(CRLF · 4칸 공백)에 맞춰 써야 다시 생성해도 diff가 생기지 않는다
            string enumString = string.Join(",\r\n", list.States.Select(so =>
            {
                SerializedObject stateObject = new SerializedObject(so);
                stateObject.FindProperty("<StateIndex>k__BackingField").intValue = index;
                stateObject.ApplyModifiedPropertiesWithoutUndo();
                return $"        {so.StateName} = {index++}";
            }));

            string path = _characterEnumPath;
            string code = string.Format(CodeFormat.EnumFormat, list.EnumName, enumString).TrimStart();

            File.WriteAllText($"{path}/{list.EnumName}.cs", code);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
