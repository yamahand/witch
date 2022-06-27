using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Text;

public static class Initializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        int a = 10;
        var str = ZString.Format("aaaa{}", a);
        Debug.Log(str);
    }
}
