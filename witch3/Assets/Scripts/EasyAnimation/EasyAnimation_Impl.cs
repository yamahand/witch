using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class EasyAnimation
{
    private class StateEnumerable : IEnumerable<IState>
    {
        public StateEnumerable(EasyAnimation owner)
        {
            _owner = owner;
        }

        private EasyAnimation _owner;

        public IEnumerator<IState> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        class StateEnumerator : IEnumerable<IState>
        {
            public StateEnumerator(EasyAnimation owner)
            {
                _owner = owner;
            }

            public IEnumerator<IState> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            private EasyAnimation _owner;
            private IEnumerable<EasyAnimationPlayable.IState> _imp;
        }
    }

    private class StateImpl : IState
    {
        public StateImpl(EasyAnimationPlayable.IState handle, EasyAnimation component)
        {
            _stateHandle = handle;
            _component = component;
        }

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
            set
            {
                _stateHandle.time = value;
                _component.Kick();
            }
        }
        float IState.normalizedTime
        {
            get { return _stateHandle.normalizedTime; }
            set
            {
                _stateHandle.normalizedTime = value;
                _component.Kick();
            }
        }
        float IState.speed
        {
            get { return _stateHandle.speed; }
            set
            {
                _stateHandle.speed = value;
                _component.Kick();
            }
        }

        string IState.name
        {
            get { return _stateHandle.name; }
            set { _stateHandle.name = value; }
        }
        float IState.weight
        {
            get { return _stateHandle.weight; }
            set
            {
                _stateHandle.weight = value;
                _component.Kick();
            }
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

        private EasyAnimationPlayable.IState _stateHandle;
        private EasyAnimation _component;
    }

    [System.Serializable]
    public class EditorState
    {
        public AnimationClip clip;
        public string name;
        public bool defaultState;
        public WrapMode wrapMode;
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

    protected void Kick()
    {
        if (!_isPlaying)
        {
            _graph.Play();
            _isPlaying = true;
        }
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
        EasyAnimationPlayable template = new EasyAnimationPlayable();
        template.Initialize(_states.Length);

        var playable = ScriptPlayable<EasyAnimationPlayable>.Create(_graph, template, 1);
        _playable = playable.GetBehaviour();
        
        if(_states == null)
        {
            _states = new EditorState[1];
            _states[0] = new EditorState();
            _states[0].defaultState = true;
            _states[0].name = "Default";
        }

        if(_states != null)
        {
            foreach (var state in _states)
            {
                if(state.clip)
                {
                    // ƒNƒŠƒbƒv‚ð“o˜^
                    _playable.AddClip(state.clip, state.name, state.wrapMode);
                }
            }
        }

        Kick();
        _initialized = true;
    }

    private void OnPlayableDone()
    {
        _graph.Stop();
        _isPlaying = false;
    }

    private void Play(Animator animator, Playable playable, PlayableGraph graph)
    {
        AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(graph, "AnimationClip", animator);
        playableOutput.SetSourcePlayable(playable, 0);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        graph.Play();
    }

    private static void LegacyClipCheck(AnimationClip clip)
    {
        if (clip && clip.legacy)
        {
            throw new ArgumentException(string.Format("Legacy clip {0} cannot be used in this component. Set .legacy property to false before using this clip", clip));
        }
    }

    private void InvalidLegacyClipError(string clipName, string stateName)
    {
        Debug.LogErrorFormat(this.gameObject, "Animation clip {0} in state {1} is Legacy. Set clip.legacy to false, or reimport as Generic to use it with SimpleAnimationComponent", clipName, stateName);
    }

    EditorState CreateDefaultEditorState()
    {
        var defaultState = new EditorState();
        defaultState.name = "Default";
        defaultState.defaultState = true;
        defaultState.wrapMode = WrapMode.Default;

        return defaultState;
    }

    private StateImpl ImplementState(EasyAnimationPlayable.IState handle)
    {
        if (handle == null) 
            return null;
        return new StateImpl(handle, this);
    }

    private void OnValidate()
    {
        //Don't mess with runtime data
        if (Application.isPlaying)
            return;

        if (_states == null || _states.Length == 0)
        {
            return;
        }


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

    protected PlayableGraph _graph;
    protected PlayableHandle _layerMixer;
    protected PlayableHandle _transitionMixer;
    protected Animator _animator;
    protected bool _initialized;
    protected bool _isPlaying;

    protected EasyAnimationPlayable _playable;

    [SerializeField]
    protected bool _playAutomatically = true;

    [SerializeField]
    protected bool _animatePhysics = false;

    [SerializeField]
    protected AnimatorCullingMode _cullingMode = AnimatorCullingMode.CullUpdateTransforms;

    [SerializeField]
    private EditorState[] _states;
}
