using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    [System.Serializable]
    public class Command
    {
        [ShowInInspector] public string name { get; private set; }
        public ParameterInfo[] parametersInfo { get; private set; }
        private MethodInfo _methodInfo;
        public uint parametersWithDefaultValue { get; private set; }
        public string description { get; private set; }
        

        private Command(string name, string description)
        {
            this.name = name.Replace(" ", "");
            this.description = description;
        }
        
        public Command(string name, string description, Action method) : this(name, description)
        {
            _methodInfo = method.GetMethodInfo();
            parametersInfo = _methodInfo.GetParameters();
            HasParametersInfoADefaultValue();
        }

        public Command(string name, string description, MethodInfo methodInfo) : this(name, description)
        {
            _methodInfo = methodInfo;
            parametersInfo = methodInfo.GetParameters();
            HasParametersInfoADefaultValue();
        }

        public void InvokeMethod(object[] parameters)
        {
            _methodInfo.Invoke(null, parameters);
        }
        
        private void HasParametersInfoADefaultValue()
        {
            for (int i = 0; i < parametersInfo.Length; i++)
            {
                if (parametersInfo[i].HasDefaultValue)
                {
                    parametersWithDefaultValue++;
                }
            }
        }
    }
}
