using System.Threading;

public class SwimParam
{
    public int swim = 0;
}

public class SwimAction : FiniteStateAction<FSMTest, SwimParam>
{
    public SwimAction(FSMTest container)
        : base(container)
    {
    }

    public override void OnEnter()
    {
        Util.Log($"OnSwimEnter: param({param.swim})");
    }

    public override void OnExit()
    {
        Util.Log("OnSwimExit");
    }
}

public class DownAction : FiniteStateActionBase
{
    float timer = 0.0f;

    public override void OnEnter()
    {
        Util.Log("OnDownEnter");
    }

    public override void OnExit()
    {
        Util.Log("OnDownExit");
    }

    public override void OnUpdate(float deltaSeconds)
    {
        timer += deltaSeconds;

        // 1초마다 처리하고 싶을 때
        if (timer >= 1.0f)
        {
            Util.Log("OnDownUpdate");
            timer = 0.0f;
        }
    }
}

public class FSMTest
{
    FiniteStateHandler stateHandler;
    float battleTimer = 0.0f;
    bool run = false;

    enum State
    {
        Idle,
        Battle,
        Die,
        Climb,
        Swim,
        Down,
        End,
        Count
    }

    enum Input
    {
        StartIdle,
        StartBattle,
        StartDie,
        StartClimb,
        StartSwim,
        StartDown,
        StartEnd,
    }

    public class ClimbParam
    {
        public int Climb = 0;
    }

    void Init()
    {
        stateHandler = new FiniteStateHandler((uint)State.Count);

        // delegate 없는 상태
        stateHandler.AddState<FSMTest>((int)State.Idle, this);

        // delegate 있는 상태
        stateHandler.AddState((int)State.Battle, this, OnBattleEnter, OnBattleExit, OnBattleUpdate);

        // delegate 일부만 있는 상태
        stateHandler.AddState((int)State.Die, this, OnDieEnter, OnDieExit, null);

        // parameter가 있는 delegate
        stateHandler.AddState<FSMTest, ClimbParam>((int)State.Climb, this, OnClimbEnter, null, null);

        // parameter가 있는 action
        stateHandler.AddStateWithAction<FSMTest, SwimParam, SwimAction>((int)State.Swim, this);

        // parameter가 없는 action
        stateHandler.AddStateWithAction<DownAction>((int)State.Down);
        stateHandler.AddStateWithAction<DownAction>((int)State.End);


        // Idle 일 때
        stateHandler.AddTransition((int)State.Idle, (int)Input.StartBattle, (int)State.Battle);
        stateHandler.AddTransition((int)State.Idle, (int)Input.StartDie, (int)State.Die);
        stateHandler.AddTransition((int)State.Idle, (int)Input.StartSwim, (int)State.Swim);

        // Battle 일 때
        stateHandler.AddTransition((int)State.Battle, (int)Input.StartDie, (int)State.Die);

        // Die 일 때
        stateHandler.AddTransition((int)State.Die, (int)Input.StartClimb, (int)State.Climb);

        // Climb 일 때
        stateHandler.AddTransition((int)State.Climb, (int)Input.StartIdle, (int)State.Idle);

        // Swim 일 때
        stateHandler.AddTransition((int)State.Swim, (int)Input.StartDown, (int)State.Down);

        // down 일 때 (3초 뒤에 End로)
        stateHandler.AddTransition((int)State.Down, (int)Input.StartEnd, (int)State.End, 3.0f);
        // 상태 유지 시간은 언제든 변경 가능. 3초에서 5초로
        stateHandler.SetStateDuration((int)State.Down, (int)Input.StartEnd, 5.0f);

        // 처음 상태
        stateHandler.SetState((int)State.Idle);
    }

    public void Start()
    {
        Init();

        PrintCurrentState();

        ChangState(Input.StartBattle);
        ChangState(Input.StartDie);

        // 파라미터가 있을 경우는 TransitState 대신 TransitStateWithParam 사용
        ClimbParam climbParam = new ClimbParam()
        {
            Climb = 100
        };
        stateHandler.TransitStateWithParam<FSMTest, ClimbParam>((int)Input.StartClimb, climbParam);
        PrintCurrentState();

        ChangState(Input.StartIdle);

        // 파라미터가 있을 경우는 TransitState 대신 TransitStateWithParam 사용
        var swimParam = new SwimParam()
        {
            swim = 200
        };
        stateHandler.TransitStateWithParam<FSMTest, SwimParam>((int)Input.StartSwim, swimParam);
        PrintCurrentState();

        ChangState(Input.StartDown);
        run = true;

        Update();
    }

    void Update()
    {
        while (run)
        {
            Thread.Sleep(1000);

            stateHandler.UpdateState(1.0f);

            if (stateHandler.GetCurrentStateId() == (int)State.End)
            {
                Stop();
            }
        }
    }

    public void Stop()
    {
        run = false;
    }

    private void PrintCurrentState()
    {
        var state = (State)stateHandler.GetCurrentStateId();
        var stateName = "invalid";

        switch (state)
        {
            case State.Idle:
                stateName = "idle";
                break;
            case State.Battle:
                stateName = "battle";
                break;
            case State.Die:
                stateName = "die";
                break;
            case State.Climb:
                stateName = "climb";
                break;
            case State.Swim:
                stateName = "swim";
                break;
            case State.Down:
                stateName = "down";
                break;
            case State.End:
                stateName = "end";
                break;
        }

        Util.Log($"Current state: {stateName}");
    }

    private void ChangState(Input input)
    {
        stateHandler.TransitState((int)input);
        PrintCurrentState();
    }

    private void OnBattleEnter()
    {
        Util.Log("OnBattleEnter");
    }

    private void OnBattleUpdate(float deltaSeconds)
    {
        battleTimer += deltaSeconds;

        if (battleTimer >= 1.0f)
        {
            Util.Log("OnBattleUpdate");
            battleTimer = 0.0f;
        }
    }

    private void OnBattleExit()
    {
        Util.Log("OnBattleExit");
    }

    private void OnDieEnter()
    {
        Util.Log("OnDieEnter");
    }

    private void OnDieExit()
    {
        Util.Log("OnDieExit");
    }

    private void OnClimbEnter(ClimbParam param)
    {
        Util.Log($"OnClimbEnter: param({param.Climb})");
    }
}
