using System.Collections.Generic;

public class FiniteState
{
    public static readonly int sInvalidStateId = -1;
    public int StateId { get; set; }

    private readonly Dictionary<int, int> transitionMap = new Dictionary<int, int>();

    public FiniteState(int stateId) => StateId = stateId;
    ~FiniteState() => transitionMap.Clear();

    public bool AddTransition(int inputEvent, int outputStateId)
    {
        if (FindOutputState(inputEvent))
        {
            Util.Assert(false);
            return false;
        }

        transitionMap.Add(inputEvent, outputStateId);
        return true;
    }

    public int GetStateCounts() => transitionMap.Count;
    public void DeleteTransition(int inputEvent) => transitionMap.Remove(inputEvent);
    public bool FindOutputState(int inputEvent) => transitionMap.ContainsKey(inputEvent);

    public (int outputStateId, bool ok) GetOutputState(int inputEvent)
    {
        if (!transitionMap.TryGetValue(inputEvent, out int outputStateId))
        {
            return (sInvalidStateId, false);
        }

        return (outputStateId, true);
    }
}
