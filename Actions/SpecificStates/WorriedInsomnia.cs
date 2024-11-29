using UnityEngine;

public class ConInsomnia : BaseDebuff
{
    public override bool WillOverride => true;

    public override bool CanManualRemove => true;

    public override BaseNotification CreateNotification()
    {
        return new NotificationCondition
        {
            condition = this,
            //text = ""
        };
    }

    public override void Tick()
    {
        base.Tick();

        if (IsKilled)
        {
            return;
        }

        CC.RemoveCondition<ConSleep>();
        if (CC.IsPC)
        {
            Stats.Sleepiness.value = Mathf.Min(Stats.Sleepiness.value, Mathf.RoundToInt(Stats.Sleepiness.max * 0.5f) );
        }
    }

    public override int GetPhase()
    {
        return 0;
    }
}