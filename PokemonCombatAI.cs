using HarmonyLib;
using PokeRaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ActPlan;
using static ObjectPool;
using static UnityEngine.UI.GridLayoutGroup;


[HarmonyPatch(typeof(CharaAbility), nameof(CharaAbility.Refresh))] //Updates the ability list for combat. We use this to add all known moves to the list.
class CombatAIOverride
{
    //Changes sprites to instead use ones based on race instad.
    static void Postfix(CharaAbility __instance)
    {
        var cc = __instance.owner;
        var isPokemon = cc.race.tag.Contains("pokemon");
        if (!isPokemon)
        {
            return;
        }
        cc._listAbility = new List<int>();
        if(cc.elements.list == null)
        {
            cc.elements.list = new List<int>();
            PokemonDatabase.TeachPokemonLevelupMoves(cc, true);
        }
        var elements = cc.elements.ListElements();
        foreach (var move in elements)
        {
            
            bool IDIsPokemonMove = move.id >= 66000 && move.id <= 68000;
            if (!IDIsPokemonMove) continue;

            float desiredMoveWeight = 100;

            desiredMoveWeight *= (3) / (3 + move.source.cost[0]); //More expensive moves are used less often.

            __instance.list.items.Add(new ActList.Item
            {
                act = ACT.dict[move.source.alias],
                chance = move.id > 66001 ? 999999 : 0,
                pt = false
            });
        }
        //make an exception for the evolve ability.
    }
}


