
using Newtonsoft.Json;
public class ConLeechSeed : BaseDebuff
{

    [JsonProperty]
    public int targetToHealID = -1;


    Chara targetToHeal;


    void UpdateHealTarget()
    {
        if (targetToHeal == null)
        {
            targetToHeal = _zone.FindChara(targetToHealID);
        }
        
        if(targetToHeal == null)
        {
            Kill();
        }
    }

    public override void Tick()
    {
        UpdateHealTarget();

        if (IsKilled)
        {
            return;
        }

        base.Tick();

        owner.DamageHP(power / 10, AttackSource.None);
        targetToHeal.HealHP(power/10);
        Effect effect = Effect.Get("heal");
        effect.Play(targetToHeal.pos);

        effect = Effect.Get("Element/ball_Acid");
        effect.Play(owner.pos);
    }
}