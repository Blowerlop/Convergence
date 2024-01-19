using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Project.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Project
{
    public class ConsoleCommandPrediction : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly] private TMP_Text _inputFieldPredictionPlaceHolder;
        [CanBeNull] [ShowInInspector, ReadOnly] public string currentPrediction { get; private set; }

        public bool HasAPrediction() => !string.IsNullOrEmpty(currentPrediction);
        
        public void Predict(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                ClearPrediction();
                return;
            }

            string[] splitInput = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            string commandInput = splitInput[0];

            List<string> allCommandsName = new List<string>();

            Console.instance.commandsName.ForEach((commandName, index) =>
            {
                if (commandName.StartsWith(commandInput, true, CultureInfo.InvariantCulture))
                {
                    allCommandsName.Add(Console.instance.commandsName[index]);
                }
            });

            // allCommandsName.Debug();

            if (allCommandsName.Any() == false)
            {
                ClearPrediction();
                return;
            }
            
            currentPrediction = allCommandsName.First();
            #if UNITY_EDITOR
            // Just to make Rider happy :)
            if (currentPrediction == null)
            {
                Debug.LogError("Current prediction is null, it should never happen");
                ClearPrediction();
                return;
            }
            #endif
            
            int inputLength = commandInput.Length;

            string preWriteCommandName = currentPrediction.Substring(0, inputLength);
            string nonWriteCommandName = currentPrediction.Substring(inputLength);

            if (string.IsNullOrEmpty(nonWriteCommandName))
            {
                _inputFieldPredictionPlaceHolder.text = $"<color=#00000000>{input}</color>";
            }
            else
            {
                _inputFieldPredictionPlaceHolder.text = $"<color=#00000000>{preWriteCommandName}</color>{nonWriteCommandName}";
            }
            
            for (int i = 0; i < Console.instance.commands[currentPrediction].parametersInfo.Length; i++)
            {
                if (splitInput.Length > i + 1) continue;
                
                ParameterInfo parameterInfo = Console.instance.commands[currentPrediction].parametersInfo[i];
                if (parameterInfo.HasDefaultValue)
                {
                    // _inputFieldPredictionPlaceHolder.text += $" <{parameterType.Name}>(Optional)";
                    _inputFieldPredictionPlaceHolder.text += $" {parameterInfo.Name}(Optional)";
                }
                else
                {
                    // _inputFieldPredictionPlaceHolder.text += $" <{parameterType.Name}>";
                    _inputFieldPredictionPlaceHolder.text += $" {parameterInfo.Name}";
                }
            }
        }
        
        private void ClearPrediction()
        {
            if (currentPrediction == null) return;
            
            currentPrediction = null;
            _inputFieldPredictionPlaceHolder.text = string.Empty;
        }
    }
}
