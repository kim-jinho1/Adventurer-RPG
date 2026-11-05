using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Agents.FSM.Editor
{
    /// <summary>
    /// StateSO 인스펙터입니다. 상태 클래스를 드롭다운으로 고르게 합니다.
    /// </summary>
    [CustomEditor(typeof(StateSO))]
    public class StateSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset stateView = default;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            stateView.CloneTree(root);

            DropdownField dropdownField = root.Q<DropdownField>("ClassDropdownField");

            CreateDropDownList(dropdownField);

            dropdownField.RegisterValueChangedCallback(HandleDropdownFieldChange);
            return root;
        }

        private void HandleDropdownFieldChange(ChangeEvent<string> evt)
        {
            serializedObject.Update();
            serializedObject.FindProperty("<ClassName>k__BackingField").stringValue = evt.newValue;
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }

        private void CreateDropDownList(DropdownField targetField)
        {
            targetField.choices.Clear();
            // AgentState가 들어 있는 어셈블리에서 상태 클래스를 찾는다
            Assembly stateAssembly = Assembly.GetAssembly(typeof(AgentState));

            List<Type> derivedTypes = stateAssembly.GetTypes()
                .Where(type => type.IsClass
                               && type.IsAbstract == false
                               && type.IsSubclassOf(typeof(AgentState)))
                .ToList();

            derivedTypes.ForEach(type => targetField.choices.Add(type.FullName));

            if(targetField.choices.Count > 0)
                targetField.SetValueWithoutNotify(derivedTypes[0].FullName);
        }
    }
}