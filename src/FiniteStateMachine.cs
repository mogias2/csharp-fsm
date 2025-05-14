using System;
using System.Collections.Generic;

public class FiniteStateMachine
{
    public string Name { get; private set; }

    FiniteState currentState = null;

    DateTime currentStateTime;

    private readonly Dictionary<int, FiniteState> stateMap = new Dictionary<int, FiniteState>();

    public FiniteStateMachine(string name = "")
    {
        Name = name;
    }

    ~FiniteStateMachine()
    {
        stateMap.Clear();
    }

    FiniteState GetState(int stateId)
    {
        if (stateMap.TryGetValue(stateId, out FiniteState state))
        {
            return state;
        }

        return null;
    }

    public FiniteState AddState(int stateId)
    {
        var state = GetState(stateId);
        if (state == null)
        {
            state = new FiniteState(stateId);
            stateMap.Add(stateId, state);
        }

        return state;
    }

    public bool AddStateTransition(int stateId, int inputEvent, int outputStateId)
    {
        var state = AddState(stateId);
        if (state == null)
        {
            return false;
        }

        return state.AddTransition(inputEvent, outputStateId);
    }

    void DeleteStateTransition(int stateId, int inputEvent)
    {
        FiniteState state = GetState(stateId);
        if (state == null)
        {
            Util.Assert(false);
            return;
        }

        state.DeleteTransition(inputEvent);
        if (state.GetStateCounts() == 0)
        {
            stateMap.Remove(state.StateId);
        }
    }

    public bool CanTransitState(int stateId, int inputEvent)
    {
        var state = GetState(stateId);
        if (state == null)
        {
            Util.Assert(false);
            return false;
        }

        return state.FindOutputState(inputEvent);
    }

    public bool SetCurrentState(int stateId)
    {
        var state = GetState(stateId);
        if (state == null)
        {
            Util.Log($"({Name}) cannot find state({stateId})");
            return false;
        }

        currentState = state;
        currentStateTime = DateTime.Now;
        return true;
    }

    public bool TransitState(int inputEvent)
    {
        if (currentState == null)
        {
            Util.Log($"{Name} current state is null");
            return false;
        }

        var (outputStateId, ok) = currentState.GetOutputState(inputEvent);
        if (!ok)
        {
            Util.Log($"{Name}({currentState.StateId}) cannot find output state for input({inputEvent})");
            return false;
        }

        return SetCurrentState(outputStateId);
    }

    public int GetCurrentStateId()
    {
        if (currentState == null)
        {
            Util.Assert(false);
            return FiniteState.sInvalidStateId;
        }

        return currentState.StateId;
    }

    public int FindOutputStateId(int inputEvent)
    {
        var state = GetState(GetCurrentStateId());
        if (state == null)
        {
            var curStateId = GetCurrentStateId();
            Util.Log($"({Name}) cannot find state({curStateId})");
            return FiniteState.sInvalidStateId;
        }

        var (outputStateId, _)  = state.GetOutputState(inputEvent);
        return outputStateId;
    }
}
