using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class PokemonStatCon : Condition
{
    public override bool WillOverride => false;

    public override BaseNotification CreateNotification()
    {
        return new NotificationCondition
        {
            condition = this,
            text = ""
        };
    }

    public override string GetText()
    {
        return Name + " is at level " + (power > 0 ? "+" : "") + power +". (" + value +")";
    }

    Condition lastCheckedToStackCondition;
    public override bool CanStack(Condition c)
    {
        lastCheckedToStackCondition = c;
        return true;
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override int EvaluateTurn(int p)
    {
        return value;
    }

    public override void OnStacked(int p)
    {
        if (Mathf.Sign(power) != Mathf.Sign(p)) //destructive application
        {
            value = 15;
        }
        else
        {
            value = Math.Max(value, lastCheckedToStackCondition.value);
        }
        

        power += p;

        power = Mathf.Clamp(power, -10, 10);

        if (p == 0)
        {
            Kill();
        }

       
    }

    public override int GetPhase()
    {
        return 0;
    }

    public override void OnValueChanged()
    {
        if (value <= 0)
        {
            if (power >= 1)
            {
                power -= 1;
            }
            else if (power <= 1)
            {
                power += 1;
            }
            if (power == 0)
            {
                Kill();
            }
            value = 15;
        }
    }

    public static double ConvertAttackLevelToDamageMultiplier(int attackModifierLevel)
    {
        if (attackModifierLevel < 0)
        {
            var absVal = Mathf.Abs(attackModifierLevel);

            return Math.Pow(0.9, absVal + 1) + 0.1;
        }
        return 1 + Math.Sqrt(attackModifierLevel) / 3;
    }
}

