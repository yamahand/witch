using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(SimpleAnimation))]
public class PlayerComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent<SimpleAnimation>(out _animation);
        if(_animation != null)
        {
            _animation.Play("Run");
            var state = _animation.GetState("Run");
            if(state != null)
            {
                //state.wrapMode = WrapMode.PingPong;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnGUI()
    {
        var states = _animation.GetStates();
        foreach (var state in states)
        {
            using (new GUILayout.VerticalScope("box"))
            {
                if( GUILayout.Button(state.name))
                {
                    _animation.Play(state.name);
                }
                //GUILayout.Label("name           : " + state.name.ToString());
                //GUILayout.Label("enabled        : " + state.enabled.ToString());
                //GUILayout.Label("isValid        : " + state.isValid.ToString());
                //GUILayout.Label("normalizedTime : " + state.normalizedTime.ToString());
                //GUILayout.Label("time           : " + state.time.ToString());
                //GUILayout.Label("length         : " + state.length.ToString());
                //GUILayout.Label("speed          : " + state.speed.ToString());
                GUILayout.Label("Duration          : " + (float)state.playable.GetDuration());
                GUILayout.Label("Time          : " + (float)state.playable.GetTime());
                GUILayout.Label("PlayState          : " + state.playable.GetPlayState());
                GUILayout.Label("GetPropagateSetTime          : " + state.playable.GetPropagateSetTime());
                GUILayout.Label("IsDone          : " + state.playable.IsDone());
                
            }
        }
    }

    /// <summary>
    /// シンプルアニメーション
    /// </summary>
    private SimpleAnimation _animation = null;
}
