using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RpgAdventure
{
    public class Clickable : MonoBehaviour
    {
        public Texture2D questionCursor;
        public CursorMode cursorMode = CursorMode.Auto;

        private void OnMouseEnter()
        {
            //Debug.Log(" On Mouse Enter");
            Vector2 hotspot = new Vector2(questionCursor.width / 2, questionCursor.height / 2);
            Cursor.SetCursor(questionCursor, hotspot, cursorMode);
        }

        private void OnMouseExit()
        {
            //Debug.Log("on Mouse Exit");
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }
}
