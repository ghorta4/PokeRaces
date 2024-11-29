using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.IO;
using PokeRaces;
using System;

[HarmonyPatch(typeof(Card), nameof(Card.DamageHP), new Type[] { typeof(int), typeof(int), typeof(int), typeof(AttackSource), typeof(Card), typeof(bool) })]
class DamageFunctionPatch
{
    static void Prefix(Card __instance, ref int dmg,ref int ele, int eleP = 100, AttackSource attackSource = AttackSource.None, Card origin = null, bool showEffect = true)
    {
        //Ele is element. For some types- IE, ptypefire and ptypepoison- switch the element to the corresponding base game equivalent after processing. Also account for the type resistance table.
        //ele 0 should be used for base damage types.
        //Elep is the power of the move. Modify this.
        //Origin causes it. This is who we check for conditions.
        Card reciever = __instance;
        bool isPokemonDamageType = ele >= 62000 && ele <= 62019;

        int attackStagesChange = 0;
        bool isElementalDamage = ele >= 910 && ele <= 925;
        if (origin != null && origin.isChara && !isPokemonDamageType) //We check if it's a Pokemon damage type so that we don't apply this on Pokemon moves, which have their own way of tracking damage
        {
            foreach (Condition c in origin.Chara.conditions)
            {
                if (c is PokeAttackMod && ele == 0)
                {
                    attackStagesChange += c.power;
                }

                
                if (c is PokeSpAttackMod && isElementalDamage)
                {
                    attackStagesChange += c.power;
                }
            }
        }

        if (reciever != null && reciever.isChara && !isPokemonDamageType)
        {
            foreach (Condition c in reciever.Chara.conditions)
            {
                if (c is PokeDefenseMod && ele == 0)
                {
                    attackStagesChange += c.power;
                }


                if (c is PokeSpDefenseMod && isElementalDamage)
                {
                    attackStagesChange += c.power;
                }
            }
        }
        if (attackStagesChange != 0)
        {
            dmg = Mathf.RoundToInt((float)(dmg * PokemonStatCon.ConvertAttackLevelToDamageMultiplier(attackStagesChange)));
        }
        //Switch Pokemon damage types to their base game equivalent for the rest of calculation.

        ele = PokemonDatabase.ConvertPokemonTypeToBaseDamageType(ele);
    }
}
