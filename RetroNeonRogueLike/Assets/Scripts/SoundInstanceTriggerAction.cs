using UnityEngine;

public class SoundInstanceTriggerAction : TriggerAction
{
    [SerializeField]private SoundInstanceType InstanceType;
    [SerializeField]private string ParameterName;
    [Space]
    [SerializeField]private bool changeOnEnter;
    [SerializeField]private float EnterParameterValue;
    [Space]
    [SerializeField]private bool changeOnStay;
    [SerializeField]private float StayParameterValue;
    [Space]
    [SerializeField]private bool changeOnExit;
    [SerializeField]private float ExitParameterValue;

    protected override void OnEnter()
    {
        base.OnEnter();

        if(!changeOnEnter)
            return;
        
        FModAudioManager.SetInstanceParameter(InstanceType, ParameterName, EnterParameterValue);
    }

    protected override void OnStay()
    {
        base.OnStay();
        
        if(!changeOnStay)
            return;
        
        FModAudioManager.SetInstanceParameter(InstanceType, ParameterName, StayParameterValue);
    }

    protected override void OnExit()
    {
        base.OnExit();
        
        if(!changeOnExit)
            return;
        
        FModAudioManager.SetInstanceParameter(InstanceType, ParameterName, ExitParameterValue);
    }
}
