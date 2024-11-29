using BepInEx;
using PokeRaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ActPlan;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class PokemonMove : Act
{
    public override TargetType TargetType { get { 


        switch (source.proc[0])
            {
                case "Self":
                    return TargetType.Self;
                case "BeamRanged":
                case "SingleRanged":
                    return TargetType.Ground;
                case "EnemiesInRange":
                case "TilesInRange":
                    return TargetType.Any;
                default:
                    return TargetType.SelfAndNeighbor;
            }
        } }//TargetType.SelfAndNeighbor;

    public override CursorInfo CursorIcon => CursorSystem.IconMelee;

    public override bool CanAutofire => true;

    public override bool CanPressRepeat => true;

    public override bool CanRapidFire => true;

    public override bool ShowRelativeAttribute => true;
    public override bool HideRightInfo => base.HideRightInfo;

    public override bool Perform() //Perform will come with some base properties that helps make really generic moves.
    {
        if (TargetType.Range == TargetRange.Self && !forcePt)
        {
            TC = owner.Chara;
            TP.Set(owner.Chara.pos);
        }

        string text = source.aliasRef.IsEmpty(owner.Chara.MainElement.source.alias);

        int power2 = owner.Chara.elements.GetOrCreateElement(source.id).GetPower(owner.Chara) * powerMod / 100;

        bool isMiss = rnd(100) > source.encFactor;

        if (isMiss)
        {
            owner.Chara.PlayEffect("fizzle");
            owner.Chara.PlaySound("fizzle");
            owner.Chara.Say("fizzle", owner.Chara);
            return true;
        }

        bool isPhysical = source.tag.Contains("phys");
        bool isSpecial = source.tag.Contains("spcl");

        foreach (Condition c in owner.Chara.conditions)
        {
            if (c is PokeAttackMod && isPhysical)
            {
                power2 = Mathf.RoundToInt((float)(PokemonStatCon.ConvertAttackLevelToDamageMultiplier(c.power) * power2));
            }


            if (c is PokeSpAttackMod && isSpecial)
            {
                power2 = Mathf.RoundToInt((float)(PokemonStatCon.ConvertAttackLevelToDamageMultiplier(c.power) * power2));
            }
        }

        if (source.tag.Contains("delay"))
        {
            ConDelayedMove applied = (ConDelayedMove)owner.Chara.AddCondition("ConDelayedMove", GetPower(owner.Chara));
            applied.power = power2;
            applied.text = text;
            applied.moveToUse = id;
            Effect effect = Effect.Get("cast");
            effect.Play(owner.Chara.pos);
        }
        else
        {
            CallProc(power2, text);
        }
        return true;
    }

    public void CallProc(int power2, string text)
    {
        CustomProc(source.proc[0].ToEnum<PokemonProcEffects>(), power2, BlessedState.Normal, owner.Chara, TC, TP, source.tag.Contains("neg"), new ActRef
        {
            n1 = source.proc.TryGet(1, returnNull: true),
            aliasEle = text,
            act = this
        });
    }

    public override int GetPower(Card c)
    {
        float a = Value * source.cost[0] + source.lvFactor;

        a *= c.Evalue(65000) * 0.01f + 1; //move use level bonus
        //  a = EClass.curve(a, 400, 100);

        int final = Mathf.FloorToInt(a);

        return final;
    }

    enum PokemonProcEffects
    {
        Self,
        Single,
        SingleRanged,
        EnemiesInRange,
        BeamRanged,
        TilesInRange
    }

    void CustomProc(PokemonProcEffects id, int power, BlessedState state, Card cc, Card tc, Point tp, bool isNeg, ActRef actRef = default(ActRef))
    {
        Chara CC = cc.Chara;
        bool flag = state <= BlessedState.Cursed;
        bool flag2 = isNeg || flag;

        bool isStatusOnly = source.tag.Contains("stat");
        bool isEnemyOnly = source.tag.Contains("enemyOnly");

        Element element = Element.Create(actRef.aliasEle.IsEmpty("eleFire"), power / 10);

        List<Point> hitList = new List<Point>();

        float range = 6.0f;

        int usedPower = power;

        int repeats = 1;

        foreach (string tag in source.tag)
        {
            if (tag.StartsWith("range"))
            {
                string[] outs = tag.Split('/');
                range = float.Parse(outs[1]);
            }
            if (tag.StartsWith("recoil"))
            {
                string[] outs = tag.Split('/');
                int recoilDamage = Mathf.RoundToInt(float.Parse(outs[1]) * usedPower);

                CC.DamageHP(recoilDamage, AttackSource.Fatigue);
            }
            if (tag.StartsWith("repeat"))
            {
                string[] outs = tag.Split('/');
                repeats = Rand.Range(int.Parse(outs[1]), int.Parse(outs[2]));
            }
            if (tag.StartsWith("heal"))
            {
                string[] outs = tag.Split('/');

                string healType = outs[1];

                switch (healType)
                {
                    case "sun":
                        float healFrac = 0.5f;


                        var weather = GetWeather();
                        switch (weather)
                        {
                            case WeatherType.HarshSun:
                                healFrac = 0.67f;
                                Effect effect2 = Effect.Get("revive");
                                effect2.Play(CC.pos);
                                break;
                            case WeatherType.Night:
                                healFrac = 0.35f;
                                break;
                            case WeatherType.Indoors:
                                healFrac = 0.25f;
                                break;
                        }
                        int toHeal = Mathf.RoundToInt(CC.MaxHP * healFrac);
                        CC.HealHP(toHeal, HealSource.Magic);
                        Effect effect = Effect.Get("heal");
                        effect.Play(CC.pos);
                        break;
                }


            }
        }
        switch (id)
        {
            case PokemonProcEffects.Self:
            case PokemonProcEffects.SingleRanged:
            case PokemonProcEffects.Single:

                hitList = new List<Point>();
                hitList.Add(tp.Copy());
                break;
            case PokemonProcEffects.BeamRanged:
                hitList = EClass._map.ListPointsInLine(CC.pos, tp, 10);
                hitList.RemoveAll(x => x.x == CC.pos.x && x.z == CC.pos.z);
                if (hitList.Count == 0)
                {
                    hitList.Add(CC.pos.Copy());
                }
                break;
            case PokemonProcEffects.EnemiesInRange:
                List<Chara> chars = _map.ListCharasInCircle(CC.pos, range);

                foreach (Chara targ in chars)
                {
                    if ((isEnemyOnly && !targ.IsHostile(pc) || targ == CC))
                    {
                        continue;
                    }
                    hitList.Add(targ.pos);
                }
                break;
            case PokemonProcEffects.TilesInRange:
                List<Point> validPoints = _map.ListPointsInCircle(CC.pos, range);
                hitList = validPoints;
                hitList.RemoveAll(x => x.x == CC.pos.x && x.z == CC.pos.z);
                break;
        }

        bool hit = false;
        for (int i = 0; i < repeats; i++)
        {
            Wait(0.5f, CC);
            CC.PlaySound("spell_hand");


            if (isStatusOnly == false)
            {
                CustomDamageEle(CC, PokemonProcEffects.Single, usedPower, element, hitList, actRef, "spell_hand");
            }
            foreach (Point p in hitList)
            {
                foreach (Card item2 in p.ListCards().ToList())
                {
                    bool success = ApplyFFX(item2);
                    if (success) hit = true;
                }
            }
        }
        if (!hit)
        {
            CC.Say("spell_hand_miss", CC, element.Name.ToLower());
        }

        foreach (string tag in source.tag)
        {
            if (tag.StartsWith("confuseSelf"))
            {
                CC.AddCondition<ConConfuse>(50, true);
            }
        }
        return;
    }

    bool CustomDamageEle(Card CC, PokemonProcEffects id, int power, Element e, List<Point> points, ActRef actref, string lang = null)
    {
        if (points.Count == 0)
        {
            CC.SayNothingHappans();
            return false;
        }
        ElementRef elementRef = EClass.setting.elements[e.source.alias];

        power /= 10;

        int num = actref.act?.ElementPowerMod ?? 50;
        int num2 = 0;
        Point point = CC.pos.Copy();
        List<Card> list = new List<Card>();
        bool flag = false;
        string text = id.ToString();
        string text2 = (EClass.sources.calc.map.ContainsKey(text) ? text : (text.ToLower() + "_"));
        foreach (Point p in points) {
            Effect effect = null;
            Effect effect2 = Effect.Get("trail1");
            Point from = p;
            switch (id)
            {
                default:
                    {
                        effect = Effect.Get("Element/ball_" + ((e.id == 0) ? "Void" : e.source.alias.Remove(0, 3)));
                        if (effect == null)
                        {
                            effect = Effect.Get("Element/ball_Fire");
                        }

                        float startDelay = (0.04f) * CC.pos.Distance(p);
                        effect.SetStartDelay(startDelay);
                        effect2.SetStartDelay(startDelay);
                        break;
                    }
            }
            if (effect2 != null)
            {
                effect2.SetParticleColor(elementRef.colorTrail, changeMaterial: true, "_TintColor").Play(from);
            }
            if (effect != null)
            {
                effect.Play(p).Flip(p.x > CC.pos.x);

            }
            bool flag3 = false;
            if (CC.IsPCFactionOrMinion && (CC.HasElement(1651) || EClass.pc.Evalue(1651) >= 2))
            {
                bool flag4 = false;
                foreach (Card item in p.ListCards())
                {
                    if (item.isChara)
                    {
                        if (item.IsPCFactionOrMinion)
                        {
                            flag4 = true;
                        }
                    }
                    else if (e.id != 910 || !item.IsFood || !item.category.IsChildOf("foodstuff"))
                    {
                        flag4 = true;
                    }
                }

                flag3 = flag4;
            }
            foreach (Card item2 in p.ListCards().ToList())
            {
                Card c = item2;
                if ((!c.isChara && !c.trait.CanBeAttacked) || (c.IsMultisize && item2 == CC) || (c.isChara && (c.Chara.host == CC || c.Chara.parasite == CC || c.Chara.ride == CC)))
                {
                    continue;
                }
                if (c.isChara && CC.isChara)
                {
                    c.Chara.RequestProtection(CC.Chara, delegate (Chara a)
                    {
                        c = a;
                    });
                }
                int num4 = 0;
                bool isChara = CC.isChara;

                Dice dice = GetDice(power);
                num4 = dice.Roll();

                if ((actref.noFriendlyFire && !CC.Chara.IsHostile(c as Chara)) || (flag && c == CC))
                {
                    continue;
                }

                if (isChara && points.Count > 1 && c != null && c.isChara && CC.isChara && CC.Chara.IsFriendOrAbove(c.Chara))
                {
                    int num5 = CC.Evalue(302);
                    if (!CC.IsPC && CC.IsPCFactionOrMinion)
                    {
                        num5 += EClass.pc.Evalue(302);
                    }

                    if (num5 > 0)
                    {
                        if (num5 * 10 > EClass.rnd(num4 + 1))
                        {
                            CC.ModExp(302, CC.IsPC ? 10 : 50);
                            continue;
                        }

                        num4 = EClass.rnd(num4 * 100 / (100 + num5 * 10 + 1));
                        CC.ModExp(302, CC.IsPC ? 20 : 100);
                        if (num4 == 0)
                        {
                            continue;
                        }
                    }
                    if (CC.HasElement(1214) || (!CC.IsPC && (CC.IsPCFaction || CC.IsPCFactionMinion) && EClass.pc.HasElement(1214) && EClass.rnd(5) != 0))
                    {
                        continue;
                    }
                }
                if (!lang.IsEmpty())
                {
                    if (lang == "spell_hand")
                    {
                        string[] list2 = Lang.GetList("attack" + (CC.isChara ? CC.Chara.race.meleeStyle.IsEmpty("Touch") : "Touch"));
                        string @ref = "_elehand".lang(e.source.GetAltname(2), list2[4]);
                        CC.Say(c.IsPCParty ? "cast_hand_ally" : "cast_hand", CC, c, @ref, c.IsPCParty ? list2[1] : list2[2]);
                    }
                    else
                    {
                        CC.Say(lang + "_hit", CC, c, e.Name.ToLower());
                    }
                }

                Chara chara = (CC.isChara ? CC.Chara : EClass._map.FindChara(actref.refThing.c_uidRefCard));

                int critChance = 0;

                if (source.tag.Contains("HighCritRate")) critChance++;

                if (critChance < 0)
                {
                    critChance = 0;
                }
                else
                {
                    int[] chanceArray = new int[] { 5, 13, 50, 9999 };
                    critChance = chanceArray[Mathf.Clamp(critChance, 0, chanceArray.Length)];
                }

                int usedPower = power;
                if (Rand.Range(0, 100) <= critChance)
                {
                    usedPower = Mathf.RoundToInt(1.5f * usedPower);
                    CC.Say("Critical hit!");
                }

                c.DamageHP(num4, e.id, usedPower * num / 100, AttackSource.None, chara ?? CC);
                if (chara != null && chara.IsAliveInCurrentZone)
                {
                    chara.DoHostileAction(c);
                }

                num2++;
            }
            if (!EClass._zone.IsPCFaction && false) //------------------------------------------------------=========================dig moves here
            {
                int num6 = actref.refThing.material.hardness;
                bool flag5 = EClass._zone.HasLaw && !EClass._zone.IsPCFaction && CC.IsPC && !(EClass._zone is Zone_Vernis);
                if (p.HasObj && p.cell.matObj.hardness <= num6)
                {
                    EClass._map.MineObj(p);
                    if (flag5)
                    {
                        EClass.player.ModKarma(-1);
                    }
                }

                if (!p.HasObj && p.HasBlock && p.matBlock.hardness <= num6)
                {
                    EClass._map.MineBlock(p);
                    if (flag5)
                    {
                        EClass.player.ModKarma(-1);
                    }
                }
            }
        }
        if (ActEffect.RapidCount == 0)
        {
            foreach (Card item3 in list)
            {
                if (item3.ExistsOnMap)
                {
                    ActEffect.RapidCount += 2;
                    CustomProc(id, power, BlessedState.Normal, item3, null, item3.pos, isNeg: true, actref);
                }
            }
        }

        return num2 > 0;
    }

    bool ApplyFFX(Card target)
    {
        if (target.isChara == false)
        {
            return false;
        }
        Chara targetChar = target.Chara;
        bool didItWork = false;

        foreach(string tag in source.tag)
        {
            if (tag.StartsWith("apply"))
            {
                string[] outs = tag.Split('/');
                var strength = int.Parse(outs[2]);
                var newCondition = Condition.Create(outs[1], strength);

                //duration skill bonus
                int durationBonus = Value;
                Condition alreadyHasCondition = owner.Chara.GetCondition<PokeAttackMod>();
                if (alreadyHasCondition != null)
                {
                    durationBonus /= Mathf.Abs(alreadyHasCondition.power);
                }
                newCondition.value = 60 + durationBonus * 2;
                targetChar.AddCondition(newCondition, true);
                didItWork = true;

                Effect effect = null;
                if (strength > 0)
                {
                    effect = Effect.Get("buff");
                }
                else
                {
                    effect = Effect.Get("debuff");
                }
                if (effect != null)
                {
                    effect.SetStartDelay(0.2f);
                    effect.Play(target.pos);
                }
            }
            if (tag.StartsWith("status"))
            {
                string[] outs = tag.Split('/');
                Effect effect = null;
                if (outs[1] == "Poison")
                {
                    Condition applied = targetChar.AddCondition("ConPoison", GetPower(owner.Chara));

                    if (applied != null)
                    {
                        didItWork = true;
                    }

                    effect = Effect.Get("Element/ball_Poison");
                    effect.Play(target.pos);
                }
                if (outs[1] == "LeechSeed")
                {
                    ConLeechSeed applied = (ConLeechSeed) targetChar.AddCondition("ConLeechSeed", GetPower(owner.Chara));

                    if (applied != null)
                    {
                        didItWork = true;
                        applied.targetToHealID = owner.Chara.uid;
                    }
                    effect = Effect.Get("Element/ball_Acid");
                    effect.Play(target.pos);
                }
                if (outs[1] == "Sleep")
                {
                    Condition applied = targetChar.AddCondition("ConSleep", GetPower(owner.Chara));

                    if (applied != null)
                    {
                        didItWork = true;
                    }
                    effect = Effect.Get("smoke");
                    effect.Play(target.pos);
                }
                if (outs[1] == "Insomnia")
                {
                    Condition applied = targetChar.AddCondition("ConInsomnia", GetPower(owner.Chara) * 3);

                    if (applied != null)
                    {
                        didItWork = true;
                    }
                    effect = Effect.Get("smoke");
                    effect.Play(target.pos);
                }
            }
        }

        return didItWork;
    }

    PokeDice GetDice(int power)
    {
        return new PokeDice { num = 1, sides = Mathf.RoundToInt(power * 0.15f), bonus = Mathf.RoundToInt(power * .85f), card = owner.Chara };
    }

    enum WeatherType
    {
        Indoors,
        Day,
        Night,
        Wind,
        HarshSun
    }

    WeatherType GetWeather()
    {
        if (_map.IsIndoor) {
            return WeatherType.Indoors;
        }

        if(world.date.IsDay)
        {
            return WeatherType.Day;
        }
        else
        {
            return WeatherType.Night;
        }
    }
}
