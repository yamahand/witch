using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public partial class EasyAnimation : MonoBehaviour
{
    public interface IState
    {
        bool enabled { get; set; }
        bool isValid { get; }
        float time { get; set; }
        float normalizedTime { get; set; }
        float speed { get; set; }
        string name { get; set; }
        float weight { get; set; }
        float length { get; }
        AnimationClip clip { get; }
        WrapMode wrapMode { get; set; }
    }

    public Animator animator
    {
        get
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
            return _animator;
        }
    }

    public bool animatePhysics
    {
        get { return _animatePhysics; }
        set { _animatePhysics = value; animator.updateMode = _animatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal; }
    }

    public AnimatorCullingMode cullingMode
    {
        get { return animator.cullingMode; }
        set { _cullingMode = value; animator.cullingMode = _cullingMode; }
    }

    public IState GetState(string stateName)
    {
        EasyAnimationPlayable.IState state = _playable.GetState(stateName);
        if (state == null)
            return null;

        return new StateImpl(state, this);

    }

    public void Sample()
    {
        _graph.Evaluate();
    }

    public IState Play()
    {
        _animator.enabled = true;
        Kick();
        if (_playAutomatically)
        {
            if (_states.Length > 0 && _states[0] != null)
            {
                var state = _playable.Play(0);
                return new StateImpl(state, this);
            }

        }
        return null;
    }

    public IState Play(string stateName)
    {
        _animator.enabled = true;
        Kick();
        return ImplementState(_playable.Play(stateName));
    }

    public void Rewind()
    {
        Kick();
        _playable.Rewind();
    }

    public void Rewind(string stateName)
    {
        Kick();
        _playable.Rewind(stateName);
    }

    public bool IsPlaying(string stateName)
    {
        return _playable.IsPlaying(stateName);
    }

    public void Stop()
    {
        _playable.StopAll();
    }

    public void Stop(string stateName)
    {
        _playable.Stop(stateName);
    }
}
