using System;
using Cysharp.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class Player
{
    private class DebugText : MonoBehaviour
    {
        private void Start()
        {
            _text = GetComponent<TextMesh>();
            if (_text == null)
            {
                _text = gameObject.AddComponent<TextMesh>();
                _text.text = "";
                _text.color = Color.red;
                _text.fontSize = 8;
                _text.anchor = TextAnchor.LowerLeft;
            }
        }

        private void FixedUpdate()
        {
            if (_text != null)
            {
                _text.text = "";
            }
        }

        private TextMesh _text;
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void InitDebugText()
    {
        gameObject.AddComponent<DebugText>();
    }
    
    [System.Diagnostics.Conditional("DEBUG")]
    private void AddDebugString(string str)
    {
        var text = GetComponent<TextMesh>();
        if (text == null)
            return;
        if ( string.IsNullOrEmpty(str))
            return;

        var callStack = StackTraceUtility.ExtractStackTrace();

        var s = callStack + " " + str;
        
        if (string.IsNullOrEmpty(text.text))
        {
            text.text = s;
        }
        else
        {
            text.text = text.text + "\n" + s;
        }
    }
}
