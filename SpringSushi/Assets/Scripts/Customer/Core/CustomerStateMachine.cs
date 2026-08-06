using System;

/// <summary>
/// CustomerState / OrderPhase の遷移だけを管理する。
/// 遷移に伴う副作用(移動制御・演出・音)はここでは扱わない。
/// </summary>
public class CustomerStateMachine
{
    public CustomerAI.CustomerState State { get; private set; } = CustomerAI.CustomerState.Spawned;
    public CustomerAI.OrderPhase Phase { get; private set; } = CustomerAI.OrderPhase.None;

    public event Action<CustomerAI.CustomerState> OnStateChanged;
    public event Action<CustomerAI.OrderPhase> OnPhaseChanged;

    public void SetState(CustomerAI.CustomerState newState)
    {
        if (State == newState) return;
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void SetPhase(CustomerAI.OrderPhase newPhase)
    {
        if (Phase == newPhase) return;
        Phase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }
}