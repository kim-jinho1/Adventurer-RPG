using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Modules
{
    /// <summary>
    /// 자식의 모듈(IModule)을 모아 초기화하고 GetModule로 찾아 주는 추상 클래스입니다.
    /// </summary>
    public abstract class ModuleOwner : MonoBehaviour
    {
        protected Dictionary<Type, IModule> moduleDict;

        protected virtual void Awake()
        {
            moduleDict = GetComponentsInChildren<IModule>().ToDictionary(module => module.GetType());
            InitializeComponents();
            AfterInitializeComponents();
        }

        protected virtual void InitializeComponents()
        {
            foreach (IModule module in moduleDict.Values)
                module.Initialize(this);
        }

        protected virtual void AfterInitializeComponents()
        {
            foreach (IAfterInitModule module in moduleDict.Values.OfType<IAfterInitModule>())
                module.AfterInitialize();
        }

        /// <summary> T 모듈을 찾습니다. 같은 타입이 없으면 T를 구현한 모듈을, 그것도 없으면 기본값을 돌려줍니다. </summary>
        public T GetModule<T>()
        {
            if (moduleDict.TryGetValue(typeof(T), out IModule module))
                return (T)module;

            IModule findModule = moduleDict.Values.FirstOrDefault(moduleType => moduleType is T);
            if(findModule is T castedModule)
                return castedModule;

            return default;
        }
    }
}
