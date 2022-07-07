using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System;

public partial class SimpleAnimationPlayable : PlayableBehaviour
{
    private int _statesVersion = 0;

    private void InvalidateStates() { _statesVersion++; }
    private class StateEnumerable: IEnumerable<IState>
    {
        private SimpleAnimationPlayable _owner;
        public StateEnumerable(SimpleAnimationPlayable owner)
        {
            _owner = owner;
        }

        public IEnumerator<IState> GetEnumerator()
        {
            return new StateEnumerator(_owner);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new StateEnumerator(_owner);
        }

        class StateEnumerator : IEnumerator<IState>
        {
            private int _index = -1;
            private int _version;
            private SimpleAnimationPlayable _owner;
            public StateEnumerator(SimpleAnimationPlayable owner)
            {
                _owner = owner;
                _version = _owner._statesVersion;
                Reset();
            }

            private bool IsValid() { return _owner != null && _version == _owner._statesVersion; }

            IState GetCurrentHandle(int index)
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");

                if (index < 0 || index >= _owner._states.count)
                    throw new InvalidOperationException("Enumerator is invalid");

                StateInfo state = _owner._states[index];
                if (state == null)
                    throw new InvalidOperationException("Enumerator is invalid");

                return new StateHandle(_owner, state.index, state.playable);
            }

            object IEnumerator.Current { get { return GetCurrentHandle(_index); } }

            IState IEnumerator<IState>.Current { get { return GetCurrentHandle(_index); } }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");

                do
                { _index++; } while (_index < _owner._states.count && _owner._states[_index] == null);

                return _index < _owner._states.count;
            }

            public void Reset()
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");
                _index = -1;
            }
        }
    }
    
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
        Playable playable { get; }
    }

    public class StateHandle : IState
    {
        public StateHandle(SimpleAnimationPlayable s, int index, Playable target)
        {
            _parent = s;
            _index = index;
            _target = target;
        }

        public bool IsValid()
        {
            return _parent.ValidateInput(_index, _target);
        }

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

        public Playable playable { get { return _target; } }

        public int index { get { return _index; } }

        private SimpleAnimationPlayable _parent;
        private int _index;
        private Playable _target;
    }

    private class StateInfo
    {
        public void Initialize(string name, AnimationClip clip, WrapMode wrapMode)
        {
            _stateName = name;
            _clip = clip;
            if (!clip.isLooping && clip.wrapMode == WrapMode.Default)
            {
                _wrapMode = WrapMode.Once;
            }
            else
            {
                _wrapMode = wrapMode;
            }
        }

        public float GetTime()
        {
            if (_timeIsUpToDate)
                return _time;

            _time = (float)_playable.GetTime();
            _timeIsUpToDate = true;
            return _time;
        }

        public void SetTime(float newTime)
        {
            _time = newTime;
            _playable.ResetTime(_time);
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
            if (_enabled == false)
                return;

            _enabledDirty = true;
            _enabled = false;
        }

        public void Pause()
        {
            //m_Playable.SetPlayState(PlayState.Paused);
            _playable.Pause();
        }

        public void Play()
        {
            //m_Playable.SetPlayState(PlayState.Playing);
            _playable.Play();
        }

        public void Stop()
        {
            _fadeSpeed = 0f;
            ForceWeight(0.0f);
            Disable();
            SetTime(0.0f);
            _playable.SetDone(false);
            if (isClone)
            {
                _readyForCleanup = true;
            }
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

        public void FadeTo(float weight, float speed)
        {
            _fading = Mathf.Abs(speed) > 0f;
            _fadeSpeed = speed;
            _targetWeight = weight;
        }

        public void DestroyPlayable()
        {
            if (_playable.IsValid())
            {
                _playable.GetGraph().DestroySubgraph(_playable);
            }
        }

        public void SetAsCloneOf(StateHandle handle)
        {
            _parentState = handle;
            _isClone = true;
        }

        public bool enabled
        {
            get { return _enabled; }
        }

        private bool _enabled;

        public int index
        {
            get { return _index; }
            set
            {
                Debug.Assert(_index == 0, "Should never reassign Index");
                _index = value;
            }
        }

        private int _index;

        public string stateName
        {
            get { return _stateName; }
            set { _stateName = value; }
        }

        private string _stateName;

        public bool fading
        {
            get { return _fading; }
        }

        private bool _fading;


        private float _time;

        public float targetWeight
        {
            get { return _targetWeight; }
        }

        private float _targetWeight;

        public float weight
        {
            get { return _weight; }
        }

        float _weight;

        public float fadeSpeed
        {
            get { return _fadeSpeed; }
        }

        float _fadeSpeed;

        public float speed
        {
            get { return (float)_playable.GetSpeed(); }
            set { _playable.SetSpeed(value); }
        }

        public float playableDuration
        {
            get { return (float)_playable.GetDuration(); }
        }

        public AnimationClip clip
        {
            get { return _clip; }
        }

        private AnimationClip _clip;

        public void SetPlayable(Playable playable)
        {
            _playable = playable;
        }

        public bool isDone { get { return _playable.IsDone(); } }

        public Playable playable
        {
            get { return _playable; }
        }

        private Playable _playable;

        public WrapMode wrapMode
        {
            get { return _wrapMode; }
        }

        private WrapMode _wrapMode;

        //Clone information
        public bool isClone
        {
            get { return _isClone; }
        }

        private bool _isClone;

        public bool isReadyForCleanup
        {
            get { return _readyForCleanup; }
        }

        private bool _readyForCleanup;

        public StateHandle parentState
        {
            get { return _parentState; }
        }

        public StateHandle _parentState;

        public bool enabledDirty { get { return _enabledDirty; } }
        public bool weightDirty { get { return _weightDirty; } }

        public void ResetDirtyFlags()
        { 
            _enabledDirty = false;
            _weightDirty = false;
        }

        private bool _weightDirty;
        private bool _enabledDirty;

        public void InvalidateTime() { _timeIsUpToDate = false; }
        private bool _timeIsUpToDate;
    }

    private StateHandle StateInfoToHandle(StateInfo info)
    {
        return new StateHandle(this, info.index, info.playable);
    }

    private class StateManagement
    {
        private List<StateInfo> _states;

        public int count { get { return _count; } }

        private int _count;

        public StateInfo this[int i]
        {
            get
            {
                return _states[i];
            }
        }

        public StateManagement()
        {
            _states = new List<StateInfo>();
        }

        public StateInfo InsertState()
        {
            StateInfo state = new StateInfo();

            int firstAvailable = _states.FindIndex(s => s == null);
            if (firstAvailable == -1)
            {
                firstAvailable = _states.Count;
                _states.Add(state);
            }
            else
            {
                _states.Insert(firstAvailable, state);
            }

            state.index = firstAvailable;
            _count++;
            return state;
        }
        public bool AnyStatePlaying()
        {
            return _states.FindIndex(s => s != null && s.enabled) != -1;
        }

        public void RemoveState(int index)
        {
            StateInfo removed = _states[index];
            _states[index] = null;
            removed.DestroyPlayable();
            _count = _states.Count;
        }

        public bool RemoveClip(AnimationClip clip)
        {
            bool removed = false;
            for (int i = 0; i < _states.Count; i++)
            {
                StateInfo state = _states[i];
                if (state != null &&state.clip == clip)
                {
                    RemoveState(i);
                    removed = true;
                }
            }
            return removed;
        }

        public StateInfo FindState(string name)
        {
            int index = _states.FindIndex(s => s != null && s.stateName == name);
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

        public bool IsCloneOf(int potentialCloneIndex, int originalIndex)
        {
            StateInfo potentialClone = _states[potentialCloneIndex];
            return potentialClone.isClone && potentialClone.parentState.index == originalIndex;
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

        public void StopState(int index, bool cleanup)
        {
            if (cleanup)
            {
                RemoveState(index);
            }
            else
            {
                _states[index].Stop();
            }
        }

        public Playable GetPlayable(int index)
        {
            return _states[index].playable;
        }
    }

    private struct QueuedState
    {
        public QueuedState(StateHandle s, float t)
        {
            _state = s;
            _fadeTime = t;
        }

        public StateHandle _state;
        public float _fadeTime;
    }

}
