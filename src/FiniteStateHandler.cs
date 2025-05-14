using System;

public class FiniteStateHandler
{
    FiniteStateMachine fsm = null;
    FiniteStateActionBase currentAction = null;
    FiniteStateActionBase[] actionList = null;

    public FiniteStateHandler(uint stateCount, string name = "")
    {
        fsm = new FiniteStateMachine(name);
        actionList = new FiniteStateActionBase[stateCount];
    }

    ~FiniteStateHandler() => Array.Clear(actionList, 0, actionList.Length);

    bool AddStateWithAction(int stateId, FiniteStateActionBase action)
    {
        if (action == null)
        {
            return false;
        }

        Util.Assert(IsValidStateId(stateId));
        Util.Assert(actionList[stateId] == null);

        actionList[stateId] = action;
        fsm.AddState(stateId);
        return true;
    }

    public bool AddTransition(int stateId, int inputEvent, int outputStateId, float duration = 0.0f)
    {
        var state = GetStateAction(stateId);
        if (state == null)
        {
            Util.Assert(false);
            return false;
        }

        var outState = GetStateAction(outputStateId);
        if (outState == null)
        {
            Util.Assert(false);
            return false;
        }

        state.SetDuration(duration, inputEvent);
        return fsm.AddStateTransition(stateId, inputEvent, outputStateId);
    }

    public void SetStateDuration(int stateId, int inputEvent, float duration)
    {
        var state = GetStateAction(stateId);
        if (state != null && fsm.CanTransitState(stateId, inputEvent))
        {
            state.SetDuration(duration, inputEvent);
        }
    }

    bool IsValidStateId(int stateId) => (stateId >= 0) && (stateId < actionList.Length);

    FiniteStateActionBase GetStateAction(int stateId)
    {
        if (!IsValidStateId(stateId))
        {
            Util.Assert(false);
            return null;
        }

        return actionList[stateId];
    }

    bool SetCurrentStateAction(int stateId)
    {
        if (!IsValidStateId(stateId) || actionList[stateId] == null)
        {
            Util.Assert(false);
            return false;
        }

        currentAction = actionList[stateId];
        return true;
    }

    FiniteStateActionBase GetCurrentStateAction()
    {
        return GetStateAction(GetCurrentStateId());
    }

    public void SetState(int stateId)
    {
        if (!SetCurrentStateAction(stateId))
        {
            return;
        }

        fsm.SetCurrentState(stateId);
        currentAction.OnEnter();
    }

    FiniteStateActionBase FindStateAction(int inputEvent)
    {
        var outputStateId = fsm.FindOutputStateId(inputEvent);
        return GetStateAction(outputStateId);
    }

    public bool TransitState(int inputEvent)
    {
        if (currentAction == null)
        {
            var curStateId = GetCurrentStateId();
            Util.Log($"Invalid current state: {fsm.Name}({curStateId})");
            return false;
        }

        var oldStateId = GetCurrentStateId();
        if (!fsm.TransitState(inputEvent))
        {
            return false;
        }

        if (oldStateId != GetCurrentStateId())
        {
            currentAction.deltaSeconds = 0.0f;
            currentAction.OnExit();
            SetCurrentStateAction(GetCurrentStateId());
            currentAction.OnEnter();
        }
        else
        {
            currentAction.deltaSeconds = 0.0f;
        }

        return true;
    }

    public int GetCurrentStateId()
    {
        return fsm.GetCurrentStateId();
    }

    public void UpdateState(float deltaSeconds)
    {
        if (currentAction == null)
        {
            var curStateId = GetCurrentStateId();
            Util.Log($"Invalid current state: {fsm.Name}({curStateId})");
            return;
        }

        currentAction.deltaSeconds += deltaSeconds;

        if (currentAction.IsExpire())
        {
            currentAction.deltaSeconds = 0.0f;
            TransitState(currentAction.inputEvent);
            return;
        }

        currentAction.OnUpdate(deltaSeconds);
    }

    bool HasState(int stateId)
    {
        if (!IsValidStateId(stateId))
        {
            return false;
        }

        return actionList[stateId] != null;
    }

    int GetStateCount() => actionList.Length;

    public bool AddState<T>(int stateId, T container, FSMDelegate onEnter, FSMDelegate onExit, FSMDelegateWithFloatParam onUpdate)
    {
        if (!IsValidStateId(stateId))
        {
            Util.Assert(false);
            return false;
        }

        if (actionList[stateId] != null)
        {
            Util.Assert(false);
            return false;
        }

        var action = new FiniteStateAction<T, FiniteStateNoneParam>(container, onEnter, onExit, onUpdate);
        actionList[stateId] = action;
        fsm.AddState(stateId);
        return true;
    }

    public bool AddState<T>(int stateId, T container) => AddState<T>(stateId, container, null, null, null);

    public bool AddState<T, P>(int stateId, T container, FiniteStateAction<T, P>.FSMDelegateWithParam OnEnterWithParam, FSMDelegate onExit, FSMDelegateWithFloatParam onUpdate)
    {
        if (!IsValidStateId(stateId))
        {
            Util.Assert(false);
            return false;
        }

        if (actionList[stateId] != null)
        {
            Util.Assert(false);
            return false;
        }

        var action = new FiniteStateAction<T, P>(container, OnEnterWithParam, onExit, onUpdate);
        actionList[stateId] = action;

        fsm.AddState(stateId);
        return true;
    }

    public bool TransitStateWithParam<T, P>(int inputEvent, P param)
    {
        if (FindStateAction(inputEvent) is FiniteStateAction<T, P> action)
        {
            action.SetParam(param);
        }

        return TransitState(inputEvent);
    }

    public bool AddStateWithAction<T, P, ACTION>(int stateId, T container) where ACTION : FiniteStateAction<T, P>
    {
        if (!IsValidStateId(stateId))
        {
            Util.Assert(false);
            return false;
        }

        if (actionList[stateId] != null)
        {
            Util.Assert(false);
            return false;
        }

        ACTION action = (ACTION)Activator.CreateInstance(typeof(ACTION), container);

        if (!AddStateWithAction(stateId, action))
        {
            return false;
        }

        return true;
    }

    public bool AddStateWithAction<ACTION>(int stateId) where ACTION : FiniteStateActionBase
    {
        if (!IsValidStateId(stateId))
        {
            Util.Assert(false);
            return false;
        }

        if (actionList[stateId] != null)
        {
            Util.Assert(false);
            return false;
        }

        var action = (ACTION)Activator.CreateInstance(typeof(ACTION));

        if (!AddStateWithAction(stateId, action))
        {
            return false;
        }

        return true;
    }
}
