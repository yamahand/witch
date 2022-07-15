using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public partial class EasyAnimationPlayable : PlayableBehaviour
{
    private Playable self { get { return _actualPlayable; } }
    public Playable playable { get { return self; } }
    public PlayableGraph graph { get { return self.GetGraph(); } }

    public System.Action onDone = null;

    public EasyAnimationPlayable()
    {
    }

    public void Initialize(int stateCount)
    {
        _states = new StateManagement(stateCount);
    }

    public bool AddClip(AnimationClip clip, string name, WrapMode wrapMode)
    {
        StateInfo state = _states.FindState(name);
        if (state != null)
        {
            Debug.LogError(string.Format("Cannot add state with name {0}, because a state with that name already exists", name));
            return false;
        }

        DoAddClip(clip, name, wrapMode);

        return true;
    }

    public Playable GetInput(int index)
    {
        if (index >= _mixer.GetInputCount())
            return Playable.Null;

        return _mixer.GetInput(index);
    }

    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        _actualPlayable = playable;

        _mixer = AnimationMixerPlayable.Create(graph, Mathf.Max(1, _states.count));

        self.SetInputCount(1);
        self.SetInputWeight(0, 1);
        graph.Connect(_mixer, 0, self, 0);
    }

    public IState GetState(string name)
    {
        StateInfo state = _states.FindState(name);
        if (state == null)
            return null;

        return new StateHandle(this, state.index, state.playable);
    }

    public IState Play(string name)
    {
        StateInfo state = _states.FindState(name);
        if (state == null)
        {
            Debug.LogError(string.Format("Cannot play state with name {0} because there is no state with that name", name));
            return null;
        }

        return Play(state.index);
    }

    public IState Play(int index)
    {
        IState playState = null;
        for (int i = 0; i < _states.count; i++)
        {
            StateInfo state = _states[i];
            if(state.index == index && state != null)
            {
                state.Enable();
                state.ForceWeight(1.0f);
                playState = new StateHandle(this, state.index, state.playable);
            }
            else
            {
                DoStop(i);
            }
        }
        return playState;
    }

    public void Rewind()
    {
        for (int i = 0; i < _states.count; i++)
        {
            if (_states[i] != null)
                _states.SetStateTime(i, 0f);
        }
    }

    public void Rewind(string name)
    {
        StateInfo state = _states.FindState(name);
        if (state == null)
        {
            Debug.LogError(string.Format("Cannot Rewind state with name {0} because there is no state with that name", name));
            return;
        }
        Rewind(state.index);
    }

    private void Rewind(int index)
    {
        _states.SetStateTime(index, 0f);
    }

    public bool StopAll()
    {
        for (int i = 0; i < _states.count; i++)
        {
            DoStop(i);
        }

        playable.SetDone(true);

        return true;
    }

    public bool IsPlaying()
    {
        return _states.AnyStatePlaying();
    }

    public bool IsPlaying(string stateName)
    {
        StateInfo state = _states.FindState(stateName);
        if (state == null)
            return false;

        return state.enabled;
    }

    public bool Stop(string name)
    {
        StateInfo state = _states.FindState(name);
        if(state == null)
        {
            Debug.LogError(string.Format("Cannot stop state with name {0} because there is no state with that name", name));
            return false;
        }

        DoStop(state.index);

        UpdateDoneStatus();
        return true;
    }

    public bool Stop(int index)
    {
        if( !_states.IsValidIndex(index))
        {
            Debug.LogError(string.Format("Cannot stop state with index {0} because there is no state with that index", index));
            return false;
        }

        DoStop(index);

        UpdateDoneStatus();
        return true;
    }

    private void DoStop(int index)
    {
        StateInfo state = _states[index];
        if (state == null)
            return;
        _states.StopState(index);
    }

    public override void PrepareFrame(Playable owner, FrameData data)
    {
        InvalidateStateTimes();
        UpdateStates(data.deltaTime);
        UpdateDoneStatus();
    }

    private StateInfo DoAddClip(AnimationClip clip, string name, WrapMode wrapMode)
    {
        StateInfo newState = _states.AddState();
        newState.Initialize(name, clip, wrapMode);
        int index = newState.index;

        if (index == _mixer.GetInputCount())
        {
            _mixer.SetInputCount(index + 1);
        }

        var clipPlayable = AnimationClipPlayable.Create(graph, clip);
        clipPlayable.SetApplyFootIK(false);
        clipPlayable.SetApplyPlayableIK(false);
        if (!clip.isLooping || newState.wrapMode == WrapMode.Once)
        {
            clipPlayable.SetDuration(clip.length);
        }
        newState.SetPlayable(clipPlayable);
        newState.Pause();

        ConnectInput(newState.index);

        return newState;
    }

    private void UpdateDoneStatus()
    {
        if (!_states.AnyStatePlaying())
        {
            bool wasDone = playable.IsDone();
            playable.SetDone(true);
            if (!wasDone && onDone != null)
            {
                onDone();
            }
        }
    }

    private void UpdateStates(float deltaTime)
    {
        bool mustUpdateWeights = false;
        float totalWeight = 0f;
        for (int i = 0; i < _states.count; i++)
        {
            var state = _states[i];
            if (state == null)
                continue;

            if (state.fading)
            {
                state.SetWeight(Mathf.MoveTowards(state.weight, state.targetWeight, state.fadeSpeed * deltaTime));
                if (Mathf.Approximately(state.weight, state.targetWeight))
                {
                    state.ForceWeight(state.targetWeight);
                    if (state.weight == 0f)
                    {
                        state.Stop();
                    }
                }
            }

            if (state.enabledDirty)
            {
                if (state.enabled)
                    state.Play();
                else
                    state.Stop();
            }

            if(state.enabled)
            {
                if(state.wrapMode == WrapMode.Once)
                {
                    bool stateIdDone = state.isDone;
                    float speed = state.speed;
                    float time = state.GetTime();
                    float duration = state.playableDuration;

                    stateIdDone |= speed < 0f && time < 0f;
                    stateIdDone |= speed >= 0f && time >= duration;
                    if(stateIdDone)
                    {
                        state.Stop();
                        state.Disable();
                    }
                }
            }

            totalWeight += state.weight;
            if(state.weightDirty)
            {
                mustUpdateWeights = true;
            }
            state.ResetDirtyFlags();
        }

        if(mustUpdateWeights)
        {
            bool hasAnyWeight = totalWeight > 0.0f;
            for (int i = 0; i < _states.count; i++)
            {
                var state = _states[i];
                if (state == null)
                    continue;

                float weight = hasAnyWeight ? state.weight / totalWeight : 0.0f;
                _mixer.SetInputWeight(state.index, weight);
            }
        }
    }

    private void InvalidateStateTimes()
    {
        int count = _states.count;
        for (int i = 0; i < count; i++)
        {
            StateInfo state = _states[i];
            if (state == null)
                continue;
            
            state.InvalidateTime();
        }
    }

    private void ConnectInput(int index)
    {
        var state = _states[index];
        graph.Connect(state.playable, 0, _mixer, state.index);
    }

    private void Disconnectinput(int index)
    {
        graph.Disconnect(_mixer, index);
    }


    private bool ValidateInput(int index, Playable input)
    {
        if (!ValidateIndex(index))
            return false;

        StateInfo state = _states[index];
        if (state == null || !state.playable.IsValid() || state.playable.GetHandle() != input.GetHandle())
            return false;

        return true;
    }

    private bool ValidateIndex(int index)
    {
        return index >= 0 && index < _states.count;
    }

    private Playable _actualPlayable;
    private AnimationMixerPlayable _mixer;
    private StateManagement _states;

}
