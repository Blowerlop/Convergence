using System.Collections.Generic;
using System.Linq;
using Project.Extensions;
using UnityEngine;

namespace Project
{
    public struct CursorState
    {
        public readonly string id;
        public readonly Texture2D texture2D;
        public readonly Vector2 hotspot;
        public readonly CursorMode cursorMode;
        public readonly CursorLockMode cursorLockMode;
        public readonly bool visible;


        public CursorState(string id, Texture2D texture2D, Vector2 hotspot, CursorMode cursorMode, CursorLockMode cursorLockMode, bool visible)
        {
            this.id = id;
            this.texture2D = texture2D;
            this.hotspot = hotspot;
            this.cursorMode = cursorMode;
            this.cursorLockMode = cursorLockMode;
            this.visible = visible;
        }
    }
    
    public static class CursorManager
    {
        [ClearOnReload(assignNewTypeInstance: true)] private static List<CursorState> _cursorStates = new();
        [ClearOnReload] private static CursorState _currentCursorState;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="hotspot">The offset from the top left of the texture to use as the target point. This must be in the bounds of the cursor</param>
        /// <param name="cursorMode"></param>
        /// <param name="cursorLockMode"></param>
        public static void Request(string id, Texture2D texture2D, Vector2 hotspot, CursorMode cursorMode, CursorLockMode cursorLockMode = CursorLockMode.Confined)
        {
            Request(id, texture2D, hotspot, cursorMode, cursorLockMode, true);
        }
        
        public static void Request(string id, CursorLockMode cursorLockMode, bool visible)
        {
            Request(id, _currentCursorState.texture2D, _currentCursorState.hotspot, _currentCursorState.cursorMode,
                cursorLockMode, visible);
        }

        private static void Request(string id, Texture2D texture2D, Vector2 hotspot, CursorMode cursorMode,
            CursorLockMode cursorLockMode, bool visible)
        {
            if (_cursorStates.FindIndex(item => item.id == id) == -1) return;
            
            CursorState cursorState = new CursorState(id, texture2D, hotspot, cursorMode, cursorLockMode, visible);
            _cursorStates.Add(cursorState);
            
            Cursor.SetCursor(texture2D, hotspot, cursorMode);
            Cursor.lockState = cursorLockMode;
            Cursor.visible = visible;
        }
        
        public static void Release(string id)
        {
            int index = _cursorStates.FindIndex(item => item.id == id);
            if (index == -1) return;
            
            // Trying to release when there was no cursor at all requested
            if (_cursorStates.TryPop(index, out CursorState cursorState) == false) return;
            
            // Set the previous cursor
            if (_cursorStates.TryPeek(out cursorState))
            {
                Cursor.SetCursor(cursorState.texture2D, cursorState.hotspot, cursorState.cursorMode);
                Cursor.lockState = cursorState.cursorLockMode;
                Cursor.visible = cursorState.visible;
            }
            else // Trying to release when there was no more cursor at all requested --> Back to the default one
            {
                //if (sendLog) Debug.Log("No other cursor cached, using the default one");
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
