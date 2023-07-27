using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

namespace Project
{
    [DefaultExecutionOrder(-1)]
    public class Console : Singleton<Console>
    {
        #region Variables

        [Title("Console State")]
        [ShowInInspector, ReadOnly] public static bool isConsoleEnabled { get; private set; }
        [ShowInInspector, ReadOnly] public bool isInputFieldFocus => _inputInputField != null && _inputInputField.isFocused;
        [ShowInInspector, ReadOnly] private int _currentNumberOfMessages;
        [CanBeNull] [ShowInInspector, ReadOnly] private string _currentPrediction;

        [Title("Parameters")]
        [SerializeField] private Vector2 _fontSizeRange = new Vector2(20, 60);
        [SerializeField] private int _maxMessages = 100;
        [SerializeField] private int _maxCommandHistory = 50;
        
        private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>();
        private string[] _commandsName;
        private List<string> _commandHistory;
        private int _commandHistoryIndex = 0;
        private int _currentIndex = -1;

        [Title("Log Colors")]
        [SerializeField] private Color _logColor;
        [SerializeField] private Color _logWarningColor;
        [SerializeField] private Color _logErrorColor;
        
        [Title("Events")]
        private Event _onConsoleCommandExecutedEvent = new Event(nameof(_onConsoleCommandExecutedEvent), false);

        [TitleGroup("References")]
        [SerializeField, ChildGameObjectsOnly] private ScrollRect _logScrollRect;
        [SerializeField, ChildGameObjectsOnly] private TMP_InputField _logInputField;
        [SerializeField, ChildGameObjectsOnly] private TMP_InputField _inputInputField;
        [SerializeField, ChildGameObjectsOnly] private TMP_Text _inputFieldPredictionPlaceHolder;
        
        #endregion
        
        
        #region Updates

        protected override void Awake()
        {
            ClearConsoleLogs();
            Application.logMessageReceived += LogConsole;
            RetrieveCommandAttribute();
        }

        private void Start()
        {
            _commandsName = new string[_commands.Count];
            int index = 0;
            foreach (var kvp in _commands)
            {
                _commandsName[index] = kvp.Key;
                index++;
            }
            _commandsName.Sort();

            _commandHistory = new List<string>(_maxCommandHistory);
            
            DisableConsoleForced();
            ClearInputField();
        }

        private void OnEnable()
        {
            _inputInputField.onSubmit.AddListener(ExecuteCommand);
            _inputInputField.onValueChanged.AddListenerExtended(CommandPrediction);
        }

        private void OnDisable()
        {
            _inputInputField.onSubmit.RemoveListener(ExecuteCommand);
            _inputInputField.onValueChanged.RemoveListenerExtended(CommandPrediction);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            Application.logMessageReceived -= LogConsole;
        }

        private void Update()
        {
            if (InputManager.instance.console)
            {
                if (isConsoleEnabled) DisableConsole();
                else EnableConsole();
            }

            if (isConsoleEnabled == false) return;
            
            
            if (Input.GetKey(KeyCode.LeftControl))
            {
                GameObject currentCurrentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                if (currentCurrentSelectedGameObject == _logScrollRect.gameObject ||
                    currentCurrentSelectedGameObject == _logInputField.gameObject)
                {
                    IncreaseOrDecreaseLogTextSize();
                }
                else if (Input.GetKeyDown(KeyCode.Backspace) && isInputFieldFocus)
                {
                    DeleteWordShortcut();
                }
            }


            // InputField Related
            if (isInputFieldFocus == false) return;
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_currentPrediction != null)
                {
                    AutoCompleteTextWithThePrediction();
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // if (string.IsNullOrEmpty(_inputInputField.text) == false)
                // {
                //     MoveCaretToTheEndOfTheText();
                //     return;
                // }
                
                GotToTheOlderInHistory();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // if (string.IsNullOrEmpty(_inputInputField.text) == false)
                // {
                //     MoveCaretToTheEndOfTheText();
                //     return;
                // }
                
                GotToTheRecentInHistory();
            }
        }

        

        #endregion


        #region Methods

        #region  Command Relative

        private void ExecuteCommand(string rawInput)
        {
            if (string.IsNullOrEmpty(rawInput)) return;

            string trimString = rawInput.TrimEnd();
            string[] splitInput = trimString.Split(" ");

            Debug.Log($">> {trimString}");
            AddToCommandHistory(trimString);

            // Check if the command exist
            if (_commands.TryGetValue(splitInput[0], out Command command))
            {
                // Check if the command have the same number of parameters that the player input
                object[] parameters = new object[command.parametersInfo.Length];
                
                if (splitInput.Length - 1 > command.parametersInfo.Length || (splitInput.Length - 1 < command.parametersInfo.Length && splitInput.Length - 1 < command.parametersInfo.Length - command.parametersWithDefaultValue))
                {
                    int commandParametersLength = command.parametersInfo.Length;

                    if (commandParametersLength == 0)
                    {
                        Debug.LogError($"This command has no parameter, you pass {parameters.Length}");
                    }
                    else if (commandParametersLength == 1)
                    {
                        Debug.LogError($"This command has {commandParametersLength} parameter, you pass {parameters.Length}");
                    }
                    else
                    {
                        Debug.LogError($"This command has {commandParametersLength} parameters, you pass {parameters.Length}");
                    }
                    
                    goto end;
                }
                
                
                // Loop through the player input and try parse the parameters
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (TryParseParameter(i, out object parameterResult) == false) goto end;

                    parameters[i] = parameterResult;
                }

                try
                {
                    command.InvokeMethod(parameters);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Unexpected behaviour : {e}");
                }
            }
            else
            {
                Debug.LogError($"Unknown command '{splitInput[0]}'");
            }
            
            end:
            ClearInputField();
            FocusOnInputField();
            // ClearCommandPrediction();
            _onConsoleCommandExecutedEvent.Invoke(this, false);
            

            
            
            bool TryParseParameter(int i, out object parameterResult)
            {
                Type parameterType = command.parametersInfo[i].ParameterType;
                
                if (i + 1 >= splitInput.Length)
                {
                    parameterResult = command.parametersInfo[i].DefaultValue;
                    return true;
                }

                string inputParameter = splitInput[i + 1];

                try
                {
                    // The is the only parameter type that is force check because C# does not Parse 0 and 1 as boolean
                    if (parameterType == typeof(bool))
                    {
                        if (bool.TryParse(inputParameter, out bool boolValue))
                        {
                            parameterResult = boolValue;
                        }
                        else
                        {
                            int valueInt = int.Parse(inputParameter);
                            if (valueInt == 0) parameterResult = false;
                            else if (valueInt == 1) parameterResult = true;
                            else throw new FormatException();
                        }
                    }
                    else
                    {
                        parameterResult = Convert.ChangeType(inputParameter, parameterType, CultureInfo.CurrentCulture);
                    }
                }
                catch
                {
                    Debug.LogError($"Unknown parameter '{inputParameter}'");
                    parameterResult = null;
                    return false;
                }

                return true;
            }
        }
        
        private void AddToCommandHistory(string input)
        {
            if (_commandHistoryIndex >= _maxCommandHistory)
            {
                _commandHistory.RemoveAt(_maxCommandHistory);
            }
            
            _commandHistory.Insert(0, input);
            _commandHistoryIndex++;

            _currentIndex = -1;
        }

        public static void AddCommand(Command command)
        {
            instance._commands.Add(command.name, command);
        }
        
        private void CommandPrediction(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                ClearCommandPrediction();
                return;
            }
            
            for (int i = 0; i < _commandsName.Length; i++)
            {
                _currentPrediction = _commandsName[i];
                
                if (_currentPrediction.StartsWith(input, true, CultureInfo.InvariantCulture))
                {
                    int inputLength = input.Length;

                    string preWriteCommandName = _currentPrediction.Substring(0, inputLength);
                    string nonWriteCommandName = _currentPrediction.Substring(inputLength);
                    _inputFieldPredictionPlaceHolder.text = $"<color=#00000000>{preWriteCommandName}</color>{nonWriteCommandName}";

                    // Enforce the input with the case of the command name
                    _inputInputField.text = _inputInputField.text.FollowCasePattern(preWriteCommandName);
                
                    for (int j = 0; j < _commands[_currentPrediction].parametersInfo.Length; j++)
                    {
                        Type parameterType = _commands[_currentPrediction].parametersInfo[j].ParameterType;
                        _inputFieldPredictionPlaceHolder.text += $" <{parameterType.Name}>";
                    }
                    return;
                }
            }
            
            ClearCommandPrediction();
        }

        #endregion
        
        #region Used by shortcut
        private void GotToTheOlderInHistory()
        {
            if (_currentIndex + 1 >= _commandHistory.Count)
            {
                MoveCaretToTheEndOfTheText();
                return;
            }

            _currentIndex++;
            
            WriteTextToInputInputField(_commandHistory[_currentIndex]);
            MoveCaretToTheEndOfTheText();
        }
        
        private void GotToTheRecentInHistory()
        {
            if (_currentIndex <= -1)
            {
                return;
            }
            if (_currentIndex <= 0)
            {
                WriteTextToInputInputField(string.Empty);
                _currentIndex = -1;
                return;
            }

            _currentIndex--;
            
            WriteTextToInputInputField(_commandHistory[_currentIndex]);
            MoveCaretToTheEndOfTheText();
        }
        
        private void IncreaseOrDecreaseLogTextSize()
        {
            _logInputField.pointSize = Mathf.Clamp(_logInputField.pointSize + Input.mouseScrollDelta.y, _fontSizeRange.x,
                _fontSizeRange.y);
        }
        
        private void DeleteWordShortcut()
        {
            int startWordPosition = 0;
            for (int i = _inputInputField.caretPosition - 1; i >= 0; i--)
            { 
                if (_inputInputField.text[i] == ' ')
                {
                    startWordPosition = i;
                    break;
                }
            }
            
            WriteTextToInputInputField(_inputInputField.text.Remove(startWordPosition, _inputInputField.caretPosition - startWordPosition));
            MoveCaretToPosition(startWordPosition);
        }

        private void AutoCompleteTextWithThePrediction()
        {
            WriteTextToInputInputField(_currentPrediction);
            // ClearCommandPrediction();
            MoveCaretToTheEndOfTheText();
        }
        #endregion
        
        #region Utilities
        private void WriteTextToInputInputField(string text)
        {
            _inputInputField.text = text;
        }
        
        private void MoveCaretToTheStartOfTheText()
        {
            _inputInputField.MoveTextStart(false);
        }
        
        private void MoveCaretToTheEndOfTheText()
        {
            _inputInputField.MoveTextEnd(false);
        }
        
        private void MoveCaretToPosition(int position)
        {
            _inputInputField.caretPosition = position;
        }
        
        private void ClearInputField() => _inputInputField.text = (string.Empty);
        
        private void FocusOnInputField()
        {
            _inputInputField.ActivateInputField();
        }
        
        private void ClearCommandPrediction()
        {
            _currentPrediction = null;
            _inputFieldPredictionPlaceHolder.text = string.Empty;
        }
        #endregion

        #region Custom Commands
        [ConsoleCommand("clear", "Wipe all the logs in the console")]
        private static void ClearConsoleLogs()
        {
            Debug.Log("Console cleared");
            instance._logInputField.text = string.Empty;
            instance._currentNumberOfMessages = 0;
        }

        [ConsoleCommand(new string[] {"help", "commands"}, "Display all the commands")]
        private static void DisplayAllCommands()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Here the list of all available commands :\n");

            foreach (var kvp in instance._commands)
            {
                stringBuilder.Append($"{kvp.Key} --- {kvp.Value.description}\n");
            }
            
            instance.LogConsole(stringBuilder.ToString(), string.Empty, LogType.Log);
        }
        #endregion
        
        [ButtonGroup]
        [ConsoleCommand("enable", "Enable the console")]
        public static void EnableConsole()
        {
            if (isConsoleEnabled) return;

            EnableConsoleForced();
        }

        private static void EnableConsoleForced()
        {
            instance.transform.GetChild(0).gameObject.SetActive(true);
            isConsoleEnabled = true;
        }

        [ButtonGroup]
        [ConsoleCommand("disable", "Disable the console")]
        public static void DisableConsole()
        {
            if (isConsoleEnabled == false) return;
            
            DisableConsoleForced();
        }

        private static void DisableConsoleForced()
        {
            instance.transform.GetChild(0).gameObject.SetActive(false);
            isConsoleEnabled = false;
        }
        
        [Button]
        private void LogConsole(string condition, string stacktrace, [EnumToggleButtons] LogType logType)
        {
            bool setAtBottom = _logScrollRect.verticalNormalizedPosition <= 0.01f;

            Color logColor;
            switch (logType)
            {
                case LogType.Log:
                    logColor = _logColor;
                    break;
                
                case LogType.Warning:
                    logColor = _logWarningColor;
                    break;
                
                // All the other LogType
                default:
                    logColor = _logErrorColor;
                    break;
            }
            _logInputField.text += $"<color=#{ColorUtility.ToHtmlStringRGB(logColor)}>{condition}</color>\n";

            if (_currentNumberOfMessages >= _maxMessages)
            {
                _logInputField.text = _logInputField.text.RemoveFirstLine();
            }
            else
            {
                _currentNumberOfMessages++;
            }
            

            #if UNITY_EDITOR
            if (setAtBottom && gameObject.activeInHierarchy) // Only to disable the StartCoroutine logError in editor
            #else
            if (setAtBottom)
            #endif
            {
                StartCoroutine(Utilities.WaitForEndOfFrameAndDoActionCoroutine(() =>
                {
                    {
                        Canvas.ForceUpdateCanvases();
                        _logScrollRect.verticalNormalizedPosition = 0.0f;
                        Canvas.ForceUpdateCanvases();
                    }
                })); 
            }
        }

        
        
        
        
        
        private void RetrieveCommandAttribute()
        {
            // https://github.com/yasirkula/UnityIngameDebugConsole/blob/master/Plugins/IngameDebugConsole/Scripts/DebugLogConsole.cs
            // Implementation of finding attributes sourced from yasirkula's code

            Profiler.BeginSample("ConsoleAttributeRetrieving");

#if UNITY_EDITOR || !NETFX_CORE
            string[] ignoredAssemblies = new string[]
            {
                "Unity",
                "System",
                "Mono.",
                "mscorlib",
                "netstandard",
                "TextMeshPro",
                "Microsoft.GeneratedCode",
                "I18N",
                "Boo.",
                "UnityScript.",
                "ICSharpCode.",
                "ExCSS.Unity",
#if UNITY_EDITOR
				"Assembly-CSharp-Editor",
                "Assembly-UnityScript-Editor",
                "nunit.",
                "SyntaxTree.",
                "AssetStoreTools"
#endif
            };
#endif
#if UNITY_EDITOR || !NETFX_CORE
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
#else
            foreach (Assembly assembly in new Assembly[] { GetType().Assembly })
#endif
            {
#if (NET_4_6 || NET_STANDARD_2_0) && (UNITY_EDITOR || !NETFX_CORE)
                if (assembly.IsDynamic)
                    continue;
#endif

                string assemblyName = assembly.GetName().Name;

#if UNITY_EDITOR || !NETFX_CORE
                if (ignoredAssemblies.Any(a => assemblyName.ToLower().StartsWith(a.ToLower())))
                {
                    continue;
                }
#endif

                try
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        {
                            foreach (object attribute in method.GetCustomAttributes(typeof(ConsoleCommandAttribute), false))
                            {
                                ConsoleCommandAttribute commandAttribute = (ConsoleCommandAttribute)attribute;
                                if (commandAttribute != null)
                                {
                                    for (int i = 0; i < commandAttribute.commandNames.Length; i++)
                                    {
                                        AddCommand(new Command(commandAttribute.commandNames[i], commandAttribute.description, method));
                                    }
                                    
                                }
                            }
                        }
                    }
                }
                catch (NotSupportedException) { }
                catch (System.IO.FileNotFoundException) { }
                catch (Exception e)
                {
                    Debug.LogError("Error whilst searching for developer console attributes in assembly(" + assemblyName + "): " + e.Message + ".");
                }
            }
            
            Profiler.EndSample();
        }

        #endregion
    }
}
