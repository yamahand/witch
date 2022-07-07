using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public partial class SimpleAnimation: MonoBehaviour, IAnimationClipSource
{
    const string _kDefaultStateName = "Default";
    private class StateEnumerable : IEnumerable<IState>
    {
        private SimpleAnimation _owner;
        public StateEnumerable(SimpleAnimation owner)
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
            private SimpleAnimation _owner;
            private IEnumerator<SimpleAnimationPlayable.IState> _impl;
            public StateEnumerator(SimpleAnimation owner)
            {
                _owner = owner;
                _impl = _owner._playable.GetStates().GetEnumerator();
                Reset();
            }

            IState GetCurrent()
            {
                return new StateImpl(_impl.Current, _owner);
            }

            object IEnumerator.Current { get { return GetCurrent(); } }

            IState IEnumerator<IState>.Current { get { return GetCurrent(); } }

            public void Dispose() { }

            public bool MoveNext()
            {
                return _impl.MoveNext();
            }

            public void Reset()
            {
                _impl.Reset();
            }
        }
    }
    private class StateImpl : IState
    {
        public StateImpl(SimpleAnimationPlayable.IState handle, SimpleAnimation component)
        {
            _stateHandle = handle;
            _component = component;
        }

        private SimpleAnimationPlayable.IState _stateHandle;
        private SimpleAnimation _component;

        bool IState.enabled
        {
            get { return _stateHandle.enabled; }
            set
            {
                _stateHandle.enabled = value;
                if (value)
                {
                    _component.Kick();
                }
            }
        }

        bool IState.isValid
        {
            get { return _stateHandle.IsValid(); }
        }
        float IState.time
        {
            get { return _stateHandle.time; }
            set { _stateHandle.time = value;
                _component.Kick(); }
        }
        float IState.normalizedTime
        {
            get { return _stateHandle.normalizedTime; }
            set { _stateHandle.normalizedTime = value;
                  _component.Kick();}
        }
        float IState.speed
        {
            get { return _stateHandle.speed; }
            set { _stateHandle.speed = value;
                  _component.Kick();}
        }

        string IState.name
        {
            get { return _stateHandle.name; }
            set { _stateHandle.name = value; }
        }
        float IState.weight
        {
            get { return _stateHandle.weight; }
            set { _stateHandle.weight = value;
                _component.Kick();}
        }
        float IState.length
        {
            get { return _stateHandle.length; }
        }

        AnimationClip IState.clip
        {
            get { return _stateHandle.clip; }
        }

        WrapMode IState.wrapMode
        {
            get { return _stateHandle.wrapMode; }
            set { Debug.LogError("Not Implemented"); }
        }
    }

    [System.Serializable]
    public class EditorState
    {
        public AnimationClip clip;
        public string name;
        public bool defaultState;
    }

    protected void Kick()
    {
        if (!_isPlaying)
        {
            _graph.Play();
            _isPlaying = true;
        }
    }

    protected PlayableGraph _graph;
    protected PlayableHandle _layerMixer;
    protected PlayableHandle _transitionMixer;
    protected Animator _animator;
    protected bool _initialized;
    protected bool _isPlaying;

    protected SimpleAnimationPlayable _playable;

    [SerializeField]
    protected bool _playAutomatically = true;

    [SerializeField]
    protected bool _animatePhysics = false;

    [SerializeField]
    protected AnimatorCullingMode _cullingMode = AnimatorCullingMode.CullUpdateTransforms;

    [SerializeField]
    protected WrapMode _wrapMode;

    [SerializeField]
    protected AnimationClip _clip;

    [SerializeField]
    private EditorState[] _states;

    protected virtual void OnEnable()
    {
        Initialize();
        _graph.Play();
        if (_playAutomatically)
        {
            Stop();
            Play();
        }
    }

    protected virtual void OnDisable()
    {
        if (_initialized)
        {
            Stop();
            _graph.Stop();
        }
    }

    private void Reset()
    {
        if (_graph.IsValid())
            _graph.Destroy();
        
        _initialized = false;
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _animator = GetComponent<Animator>();
        _animator.updateMode = _animatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;
        _animator.cullingMode = _cullingMode;
        _graph = PlayableGraph.Create();
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        SimpleAnimationPlayable template = new SimpleAnimationPlayable();

        var playable = ScriptPlayable<SimpleAnimationPlayable>.Create(_graph, template, 1);
        _playable = playable.GetBehaviour();
        _playable.onDone += OnPlayableDone;
        if (_states == null)
        {
            _states = new EditorState[1];
            _states[0] = new EditorState();
            _states[0].defaultState = true;
            _states[0].name = "Default";
        }


        if (_states != null)
        {
            foreach (var state in _states)
            {
                if (state.clip)
                {
                    _playable.AddClip(state.clip, state.name);
                }
            }
        }

        EnsureDefaultStateExists();

        Play(_animator, _playable.playable, _graph);
        Play();
        Kick();
        _initialized = true;
    }

    private void Play(Animator animator, Playable playable, PlayableGraph graph)
    {
        AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(graph, "AnimationClip", animator);
        playableOutput.SetSourcePlayable(playable, 0);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        graph.Play();
    }

    private void EnsureDefaultStateExists()
    {
        if ( _playable != null && _clip != null && _playable.GetState(_kDefaultStateName) == null )
        {
            _playable.AddClip(_clip, _kDefaultStateName);
            Kick();
        }
    }

    protected virtual void Awake()
    {
        Initialize();
    }

    protected void OnDestroy()
    {
        if (_graph.IsValid())
        {
            _graph.Destroy();
        }
    }

    private void OnPlayableDone()
    {
        _graph.Stop();
        _isPlaying = false;
    }

    private void RebuildStates()
    {
        var playableStates = GetStates();
        var list = new List<EditorState>();
        foreach (var state in playableStates)
        {
            var newState = new EditorState();
            newState.clip = state.clip;
            newState.name = state.name;
            list.Add(newState);
        }
        _states = list.ToArray();
    }

    EditorState CreateDefaultEditorState()
    {
        var defaultState = new EditorState();
        defaultState.name = "Default";
        defaultState.clip = _clip;
        defaultState.defaultState = true;

        return defaultState;
    }

    static void LegacyClipCheck(AnimationClip clip)
    {
        if (clip && clip.legacy)
        {
            throw new ArgumentException(string.Format("Legacy clip {0} cannot be used in this component. Set .legacy property to false before using this clip", clip));
        }
    }
    
    void InvalidLegacyClipError(string clipName, string stateName)
    {
        Debug.LogErrorFormat(this.gameObject,"Animation clip {0} in state {1} is Legacy. Set clip.legacy to false, or reimport as Generic to use it with SimpleAnimationComponent", clipName, stateName);
    }

    private void OnValidate()
    {
        //Don't mess with runtime data
        if (Application.isPlaying)
            return;

        if (_clip && _clip.legacy)
        {
            Debug.LogErrorFormat(this.gameObject,"Animation clip {0} is Legacy. Set clip.legacy to false, or reimport as Generic to use it with SimpleAnimationComponent", _clip.name);
            _clip = null;
        }

        //Ensure at least one state exists
        if (_states == null || _states.Length == 0)
        {
            _states = new EditorState[1];   
        }

        //Create default state if it's null
        if (_states[0] == null)
        {
            _states[0] = CreateDefaultEditorState();
        }

        //If first state is not the default state, create a new default state at index 0 and push back the rest
        if (_states[0].defaultState == false || _states[0].name != "Default")
        {
            var oldArray = _states;
            _states = new EditorState[oldArray.Length + 1];
            _states[0] = CreateDefaultEditorState();
            oldArray.CopyTo(_states, 1);
        }

        //If default clip changed, update the default state
        if (_states[0].clip != _clip)
            _states[0].clip = _clip;


        //Make sure only one state is default
        for (int i = 1; i < _states.Length; i++)
        {
            if (_states[i] == null)
            {
                _states[i] = new EditorState();
            }
            _states[i].defaultState = false;
        }

        //Ensure state names are unique
        int stateCount = _states.Length;
        string[] names = new string[stateCount];

        for (int i = 0; i < stateCount; i++)
        {
            EditorState state = _states[i];
            if (state.name == "" && state.clip)
            {
                state.name = state.clip.name;
            }

#if UNITY_EDITOR
            state.name = ObjectNames.GetUniqueName(names, state.name);
#endif
            names[i] = state.name;

            if (state.clip && state.clip.legacy)
            {
                InvalidLegacyClipError(state.clip.name, state.name);
                state.clip = null;
            }
        }

        _animator = GetComponent<Animator>();
        _animator.updateMode = _animatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;
        _animator.cullingMode = _cullingMode;
    }

    public void GetAnimationClips(List<AnimationClip> results)
    {
        foreach (var state in _states)
        {
            if (state.clip != null)
                results.Add(state.clip);
        }
    }
}
