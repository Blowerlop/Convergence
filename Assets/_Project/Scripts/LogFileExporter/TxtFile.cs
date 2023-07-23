using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Project
{
    public static class TxtFile
    {
        public static void Write(string filePath, string text, bool overwrite = false)
        {
            using StreamWriter writer = new StreamWriter(filePath, !overwrite);
            writer.WriteLine(text);
        }
        
        public static void Write(Stream file, string text, bool overwrite = false)
        {
            using StreamWriter writer = new StreamWriter(file);
            writer.WriteLine(text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">C:\Users\...\Documents\Unity\Projects\Sandbox\Assets\_Project\Data</param>
        /// <param name="text"></param>
        /// <param name="overwriteExistingFile"></param>
        public static void WriteWithFileCreation(string filePath, string text, bool overwriteExistingFile = false)
        {
            CreateFile(filePath, overwriteExistingFile);
            // await Task.Delay(1);
            Write(filePath, text);
        }

        public static string Read(string filePath)
        {
            using StreamReader reader = new StreamReader(filePath);
            return reader.ReadToEnd();
        }

        public static void CreateFile(string filePath, bool overwriteExistingFile = false)
        {
            if (System.IO.File.Exists(filePath))
            {
                if (overwriteExistingFile)
                {
                    System.IO.File.Delete(filePath);
                }
                else return;
            } 
            
            FileStream file = System.IO.File.Create(filePath);
            file.Dispose();
            Debug.Log("New file created at : " + filePath);
        }
 
        public static void GetFile(string filePath)
        {
        }

        public static void Clear(string filePath)
        {
            Write(filePath, null, true);
        }
    }
    
    public class DirectoryAlreadyExistsException : Exception
    {
        public DirectoryAlreadyExistsException()
        {
        }

        public DirectoryAlreadyExistsException(string message)
            : base(message)
        {
        }

        public DirectoryAlreadyExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    
    public class FileAlreadyExistsException : Exception
    {
        public FileAlreadyExistsException()
        {
        }

        public FileAlreadyExistsException(string message)
            : base(message)
        {
        }

        public FileAlreadyExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}