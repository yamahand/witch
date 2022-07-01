using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        _inputActions = new WitchInput();
        _inputActions.Enable();
        _inputActions.Player.Fire.performed += context => Debug.Log("fire performed");
        _inputActions.Player.Fire.canceled += context => Debug.Log("fire canceled");
        _inputActions.Player.Fire.started += context => Debug.Log("fire started");
        _inputActions.Player.Jump.performed += context => Debug.Log("jump");
    }

    // Update is called once per frame
    void Update()
    {
        const float speed = 1f;

        var dir = _inputActions.Player.Move.ReadValue<Vector2>();
        transform.Translate(dir * speed * Time.deltaTime);
    }

    WitchInput _inputActions = null;
}
