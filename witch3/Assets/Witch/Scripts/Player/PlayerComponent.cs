using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleAnimation))]
public class PlayerComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent<SimpleAnimation>(out _animation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// シンプルアニメーション
    /// </summary>
    private SimpleAnimation _animation = null;
}
