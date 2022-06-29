using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Witch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        _test = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(_test.ToString());
    }

    public int Test => _test;
    private int _test = 0;
}
