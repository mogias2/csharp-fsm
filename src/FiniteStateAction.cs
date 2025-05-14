public class FiniteStateActionBase
{
    public float deltaSeconds = 0.0f;
    public int inputEvent = -1;

    protected float expiration = 0.0f;

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate(float deleteSeconds) { }

    public void SetDuration(float duration, int inputEvent)
    {
        deltaSeconds = 0.0f;
        expiration = duration;
        this.inputEvent = inputEvent;
    }

    public bool IsExpire()
    {
        return expiration != 0.0f && deltaSeconds >= expiration;
    }

    void Clear()
    {
        deltaSeconds = 0.0f;
        expiration = 0.0f;
    }

    int GetInputEvent() => inputEvent;
}

public delegate void FSMDelegate();
public delegate void FSMDelegateWithFloatParam(float deltaSeconds);

public class FiniteStateAction<T, P> : FiniteStateActionBase
{
    public delegate void FSMDelegateWithParam(P param);

    protected T container;
    protected P param;

    private readonly FSMDelegate onEnter = null;
    private readonly FSMDelegateWithParam onEnterWithParam = null;
    private readonly FSMDelegate onExit = null;
    private readonly FSMDelegateWithFloatParam onUpdate = null;

    public FiniteStateAction(T container) => this.container = container;

    public FiniteStateAction(T container, FSMDelegate onEnter, FSMDelegate onExit, FSMDelegateWithFloatParam onUpdate)
    {
        this.container = container;
        this.onEnter = onEnter;
        this.onExit = onExit;
        this.onUpdate = onUpdate;
    }

    public FiniteStateAction(T container, FSMDelegateWithParam onEnterWithParam, FSMDelegate onExit, FSMDelegateWithFloatParam onUpdate)
    {
        this.container = container;
        this.onEnterWithParam = onEnterWithParam;
        this.onExit = onExit;
        this.onUpdate = onUpdate;
    }

    public override void OnEnter()
    {
        if (onEnter != null)
        {
            onEnter();
            return;
        }

        onEnterWithParam?.Invoke(param);
    }

    public override void OnExit()
    {
        onExit?.Invoke();
    }

    public override void OnUpdate(float deltaSeconds)
    {
        onUpdate?.Invoke(deltaSeconds);
    }

    public void SetParam(P param)
    {
        this.param = param;
    }
}
