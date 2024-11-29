using BepInEx;
using PokeRaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

internal class EvolvingState : Condition
{
    public override bool ConsumeTurn => true;

    public override bool TimeBased => true;

    public override bool CanManualRemove => true;

    public override void Tick()
    {
        base.Tick();
        var turnsLeft = value;

        if (turnsLeft > 10)
        {
            owner.stamina.Mod(-1);
            owner.PlaySound("boost", 1.4f);
            owner.PlayEffect("revive");
        }
        else if (turnsLeft > 5)
        {
            owner.stamina.Mod(-1);
            owner.PlaySound("boost", 2.0f);
            owner.PlaySound("debuff", 0.7f);
            owner.PlayEffect("buff");
            owner.PlayEffect("revive");
        }
        else if (turnsLeft > 1)
        {
            owner.stamina.Mod(-2);
            owner.PlaySound("boost", 2.5f);
            owner.PlaySound("debuff", 1.4f);
            owner.PlayEffect("buff");
            owner.PlayEffect("revive");
            owner.PlayEffect("intonation");
        }
        else if (turnsLeft == 1)
        {
            owner.PlaySound("boost", 2.5f);
            owner.PlaySound("debuff", 1.4f);
            owner.PlaySound("godbless", 1.0f);
            owner.PlayEffect("buff");
            owner.PlayEffect("holyveil");
            owner.PlayEffect("intonation");
            owner.PlayEffect("revive");

            var isPokemon = owner.race.tag.Contains("pokemon");

            if (isPokemon == false) return;

            string userPokemon = owner.race.id;

            var databaseRow = PokemonDatabase.GetPokemonInfoForSpecies(userPokemon);

            if (databaseRow == null) return;

            if (databaseRow.evolution.IsNullOrWhiteSpace()) return; //if there is no evolution.

            Debug.Log("Evolving from " + userPokemon + " to " + databaseRow.evolution + "...!");

            owner.ChangeRace(databaseRow.evolution);

            SharedPokemonSprites.TryApplyPokemonSprites(owner);
            owner.renderer.RefreshSprite();

            owner.HealAll();
            owner.elements.Remove(62020);
            if (owner.IsPC)
            {
                LayerAbility.Redraw();
            }

            PokemonDatabase.TeachPokemonLevelupMoves(owner);
            return;
        }

        if (owner.hp <= 0 && turnsLeft > 1) Kill();
    }

}
