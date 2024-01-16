using System;
using System.Reflection;
using Sirenix.OdinInspector;

namespace Project
{
    [Serializable]
    public class ConsoleCommand
    {
        [ShowInInspector] public string name { get; private set; }
        public ParameterInfo[] parametersInfo { get; private set; }
        private MethodInfo _methodInfo;
        public uint parametersWithDefaultValue { get; private set; }
        public string description { get; private set; }
        

        private ConsoleCommand(string name, string description)
        {
            this.name = name.Replace(" ", "");
            this.description = description;
        }
        
        public ConsoleCommand(string name, string description, Action method) : this(name, description)
        {
            SetupFinalParameters(method.GetMethodInfo());
        }

        public ConsoleCommand(string name, string description, MethodInfo methodInfo) : this(name, description)
        {
            SetupFinalParameters(methodInfo);
        }
        

        private void SetupFinalParameters(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            parametersInfo = methodInfo.GetParameters();
            HasParametersInfoHaveDefaultValue();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            description += $" ({_methodInfo.ReflectedType})";
            #endif
        }

        public void InvokeMethod(object[] parameters)
        {
            _methodInfo.Invoke(null, parameters);
        }
        
        private void HasParametersInfoHaveDefaultValue()
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
