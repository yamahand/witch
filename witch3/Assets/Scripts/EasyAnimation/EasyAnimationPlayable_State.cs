using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public partial class EasyAnimationPlayable
{
    public interface IState
    {
        bool IsValid();

        bool enabled { get; set; }

        float time { get; set; }

        float normalizedTime { get; set; }

        float speed { get; set; }

        string name { get; set; }

        float weight { get; set; }

        float length { get; }

        AnimationClip clip { get; }

        WrapMode wrapMode { get; }
    }

    public class StateHandle : IState
    {
        public EasyAnimationPlayable parent { get { return _parent; } }
        public int index { get { return _index; } }
        public Playable target { get { return _target; } }

        public bool enabled
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states[_index].enabled;
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value)
                    _parent._states.EnableState(_index);
                else
                    _parent._states.DisableState(_index);
            }
        }
        public float time
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states.GetStateTime(_index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                _parent._states.SetStateTime(_index, value);
            }
        }

        public float normalizedTime
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");

                float length = _parent._states.GetClipLength(_index);
                if (length == 0f)
                    length = 1f;

                return _parent._states.GetStateTime(_index) / length;
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");

                float length = _parent._states.GetClipLength(_index);
                if (length == 0f)
                    length = 1f;

                _parent._states.SetStateTime(_index, value *= length);
            }
        }

        public float speed
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states.GetStateSpeed(_index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                _parent._states.SetStateSpeed(_index, value);
            }
        }

        public string name
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states.GetStateName(_index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value == null)
                    throw new System.ArgumentNullException("A null string is not a valid name");
                _parent._states.SetStateName(_index, value);
            }
        }

        public float weight
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states[_index].weight;
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value < 0)
                    throw new System.ArgumentException("Weights cannot be negative");

                _parent._states.SetInputWeight(_index, value);
            }
        }

        public float length
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states.GetStateLength(_index);
            }
        }

        public AnimationClip clip
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states.GetStateClip(_index);
            }
        }

        public WrapMode wrapMode
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return _parent._states.GetStateWrapMode(_index);
            }
        }

        public StateHandle(EasyAnimationPlayable parent, int index, Playable target)
        {
            _parent = parent;
            _index = index;
            _target = target;
        }

        private EasyAnimationPlayable _parent;
        private int _index;
        private Playable _target;

        public bool IsValid()
        {
            throw new System.NotImplementedException();
        }
    }

    private class StateInfo
    {
        public void Initialize(string name, AnimationClip clip, WrapMode wrapMode)
        {
            _stateName = name;
            _clip = clip;
            _wrapMode = wrapMode;
        }

        public float GetTime()
        {
            if (_timeIsUpToDate)
                return _time;

            _time = (float)_playable.GetTime();
            _timeIsUpToDate = true;
            var duration = (float)_playable.GetDuration();
            while (_time > duration)
            {
                _time -= duration;
            }
            return _time;
        }

        public void SetTime(float newTime)
        {
            _time = newTime;
            _playable.SetTime(newTime);
            _playable.SetDone(_time >= _playable.GetDuration());
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabledDirty = true;
            _enabled = true;
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabledDirty = true;
            _enabled = false;
        }

        public void Pause()
        {
            _playable.Pause();
        }

        public void Play()
        {
            _playable.Play();
        }

        public bool IsPlaying()
        {
            return _playable.GetPlayState() == PlayState.Playing;
        }

        public void Stop()
        {
            _fadeSpeed = 0f;
            ForceWeight(0f);
            Disable();
            SetTime(0f);
            _playable.SetDone(false);
        }

        public void ForceWeight(float weight)
        {
            _targetWeight = weight;
            _fading = false;
            _fadeSpeed = 0f;
            SetWeight(weight);
        }

        public void SetWeight(float weight)
        {
            _weight = weight;
            _weightDirty = true;
        }

        public void FadeTo(float weight, float fadeSpeed)
        {
            _fading = Mathf.Abs(fadeSpeed) > 0f;
            _fadeSpeed = fadeSpeed;
            _targetWeight = weight;
        }

        public void DestroyPlayable()
        {
            if (_playable.IsValid())
            {
                _playable.GetGraph().DestroySubgraph(_playable);
            }
        }

        public bool enabled { get { return _enabled; } }
        public int index
        {
            get { return _index; }
            set
            {
                Debug.Assert(_index == 0, "Should nevar reassing Index");
                _index = value;
            }
        }

        public string stateName
        {
            get { return _stateName; }
            set { _stateName = value; }
        }

        public bool fading { get { return _fading; } }
        public float fadeSpeed { get { return _fadeSpeed; } }

        public float targetWeight { get { return _targetWeight; } }
        public float weight { get { return _weight; } }

        public float speed
        {
            get { return (float)_playable.GetSpeed(); }
            set { _playable.SetSpeed(value); }
        }

        public float playableDuration
        {
            get { return (float)_playable.GetDuration(); }
        }

        public AnimationClip clip { get { return _clip; } }
        public bool isDone { get { return _playable.IsDone(); } }

        public bool enabledDirty { get { return _enabledDirty; } }
        public bool weightDirty { get { return _weightDirty; } }
        public Playable playable { get { return _playable; } }
        public WrapMode wrapMode { get { return _wrapMode; } }

        public void ResetDirtyFlags()
        {
            _enabledDirty = false;
            _weightDirty = false;
        }

        public void SetPlayable(Playable playable)
        {
            _playable = playable;
        }

        public void InvalidateTime() { _timeIsUpToDate = false; }

        private int _index;
        private string _stateName;
        private bool _fading;
        private bool _timeIsUpToDate;
        private float _time;
        private float _targetWeight;
        private float _weight;
        private bool _weightDirty;
        private AnimationClip _clip;
        private WrapMode _wrapMode;
        private bool _enabled;
        private bool _enabledDirty;
        private Playable _playable;
        private float _fadeSpeed;
    }

    private StateHandle StateInfoToHandle(StateInfo info)
    {
        return new StateHandle(this, info.index, info.playable);
    }

    private class StateManagement
    {
        public int count { get { return _count; } }
        public StateInfo this[int i] { get { return _states[i]; } }

        public StateManagement(int count)
        {
            Debug.Assert(count > 0, "count must be greater than 0.");
            _states = new StateInfo[count];
        }
        public StateInfo AddState()
        {
            if (_states == null)
            {
                Debug.LogError("State list not initialized.");
                return null;
            }

            if (_count >= _states.Length)
            {
                Debug.LogError("Too many states to add.");
                return null;
            }

            StateInfo state = new StateInfo();
            state.index = _count;
            _states[_count] = state;
            _count++;
            return state;
        }

        public bool AnyStatePlaying()
        {
            return Array.FindIndex(_states, s => s != null && s.enabled) != -1;
        }

        public StateInfo FindState(string name)
        {
            int index = Array.FindIndex(_states, s => s != null && s.stateName == name);

            if (index == -1)
                return null;

            return _states[index];
        }

        public void EnableState(int index)
        {
            StateInfo state = _states[index];
            state.Enable();
        }

        public void DisableState(int index)
        {
            StateInfo state = _states[index];
            state.Disable();
        }

        public void SetInputWeight(int index, float weight)
        {
            StateInfo state = _states[index];
            state.SetWeight(weight);

        }

        public void SetStateTime(int index, float time)
        {
            StateInfo state = _states[index];
            state.SetTime(time);
        }

        public float GetStateTime(int index)
        {
            StateInfo state = _states[index];
            return state.GetTime();
        }

        public float GetStateSpeed(int index)
        {
            return _states[index].speed;
        }
        public void SetStateSpeed(int index, float value)
        {
            _states[index].speed = value;
        }

        public float GetInputWeight(int index)
        {
            return _states[index].weight;
        }

        public float GetStateLength(int index)
        {
            AnimationClip clip = _states[index].clip;
            if (clip == null)
                return 0f;
            float speed = _states[index].speed;
            if (speed == 0f)
                return Mathf.Infinity;

            return clip.length / speed;
        }

        public float GetClipLength(int index)
        {
            AnimationClip clip = _states[index].clip;
            if (clip == null)
                return 0f;

            return clip.length;
        }

        public float GetStatePlayableDuration(int index)
        {
            return _states[index].playableDuration;
        }

        public AnimationClip GetStateClip(int index)
        {
            return _states[index].clip;
        }

        public WrapMode GetStateWrapMode(int index)
        {
            return _states[index].wrapMode;
        }

        public string GetStateName(int index)
        {
            return _states[index].stateName;
        }

        public void SetStateName(int index, string name)
        {
            _states[index].stateName = name;
        }

        public void StopState(int index)
        {
            _states[index].Stop();
        }

        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < _count;
        }

        private StateInfo[] _states = null;
        private int _count = 0;
    }
}
