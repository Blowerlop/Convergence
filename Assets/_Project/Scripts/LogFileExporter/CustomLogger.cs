using System;
using System.Globalization;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace Project
{
    [DefaultExecutionOrder(-1)]
    public class CustomLogger : MonoBehaviour
    {
        #region Variables
        private const string LogSaverDefaultFilePath = "/_Project/Data/Log.txt";
        private static readonly string LogSaverFilePath = $"{Application.dataPath}{LogSaverDefaultFilePath}";

        private readonly StringBuilder _stringBuilder = new StringBuilder();
        #endregion
        
        
        #region Updates
        private void Awake()
        {
            ClearLogFile();
            
            Application.logMessageReceived += WriteLogToFile;
        }
        
        private void OnDestroy()
        { 
            Application.logMessageReceived -= WriteLogToFile;
        }
        #endregion

        
        #region Methods
        [InitializeOnLoadMethod]
        private static void CreateLogFile()
        {
            TxtFile.CreateFile(LogSaverFilePath, false);
        }

        private static void ClearLogFile()
        {
            TxtFile.Clear(LogSaverFilePath);
        }
         
        private void WriteLogToFile(string condition, string trace, LogType type)
        {
            _stringBuilder.AppendLine($"[{StripMilliseconds(DateTime.Now.TimeOfDay).ToString()}] {type.ToString()}");
            _stringBuilder.AppendLine($"{condition} \n");
            _stringBuilder.AppendLine($"{trace}");
            TxtFile.Write(LogSaverFilePath, _stringBuilder.ToString(), false);
            
            _stringBuilder.Clear();
        }
        
        [ConsoleCommand("export_logs", "Send the current logs to the discord")]
        [Button]
        public static void ExportLogToDiscord()
        {
            if (File.Exists(LogSaverFilePath) == false)
            {
                Debug.LogError("There is no log file to send");
                return;
            }

            Discord.SendFile(DateTime.Now.ToString(CultureInfo.GetCultureInfoByIetfLanguageTag("fr")), Discord.TxtFileFormat,
                LogSaverFilePath, Discord.TxtFileFormat);
        }
        
        private TimeSpan StripMilliseconds(TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }
        
        #endregion
    }
}