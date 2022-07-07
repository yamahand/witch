using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public partial class SimpleAnimation: MonoBehaviour
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
        Playable playable { get; }

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
        set { _cullingMode = value;  animator.cullingMode = _cullingMode; }
    }

    public bool isPlaying { get { return _playable.IsPlaying(); } }

    public bool playAutomatically
    {
        get { return _playAutomatically; }
        set { _playAutomatically = value; }
    }

    public AnimationClip clip
    {
        get { return _clip; }
        set
        {
            LegacyClipCheck(value);
            _clip = value;
        }  
    }

    public WrapMode wrapMode
    {
        get { return _wrapMode; }
        set { _wrapMode = value; }
    }

    public void AddClip(AnimationClip clip, string newName)
    {
        LegacyClipCheck(clip);
        AddState(clip, newName);
    }

    public void Blend(string stateName, float targetWeight, float fadeLength)
    {
        _animator.enabled = true;
        Kick();
        _playable.Blend(stateName, targetWeight,  fadeLength);
    }

    public void CrossFade(string stateName, float fadeLength)
    {
        _animator.enabled = true;
        Kick();
        _playable.Crossfade(stateName, fadeLength);
    }

    public void CrossFadeQueued(string stateName, float fadeLength, QueueMode queueMode)
    {
        _animator.enabled = true;
        Kick();
        _playable.CrossfadeQueued(stateName, fadeLength, queueMode);
    }

    public int GetClipCount()
    {
        return _playable.GetClipCount();
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

    public void Sample()
    {
        _graph.Evaluate();
    }

    public bool Play()
    {
        _animator.enabled = true;
        Kick();
        if (_clip != null && _playAutomatically)
        {
            _playable.Play(_defaultStateName);
        }
        return false;
    }

    public void AddState(AnimationClip clip, string name)
    {
        LegacyClipCheck(clip);
        Kick();
        if (_playable.AddClip(clip, name))
        {
            RebuildStates();
        }
        
    }

    public void RemoveState(string name)
    {
        if (_playable.RemoveClip(name))
        {
            RebuildStates();
        }
    }

    public bool Play(string stateName)
    {
        _animator.enabled = true;
        Kick();
        return _playable.Play(stateName);
    }

    public void PlayQueued(string stateName, QueueMode queueMode)
    {
        _animator.enabled = true;
        Kick();
        _playable.PlayQueued(stateName, queueMode);
    }

    public void RemoveClip(AnimationClip clip)
    {
        if (clip == null)
            throw new System.NullReferenceException("clip");

        if ( _playable.RemoveClip(clip) )
        {
            RebuildStates();
        }
       
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

    public IState GetState(string stateName)
    {
        SimpleAnimationPlayable.IState state = _playable.GetState(stateName);
        if (state == null)
            return null;

        return new StateImpl(state, this);
    }

    public IEnumerable<IState> GetStates()
    {
        return new StateEnumerable(this);
    }

    public IState this[string name]
    {
        get { return GetState(name); }
    }

}
