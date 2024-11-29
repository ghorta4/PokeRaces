using BepInEx;
using PokeRaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ActEvolve : Act
{
    public override TargetType TargetType => TargetType.Self;

    public override CursorInfo CursorIcon => CursorSystem.Notice;

    public override bool Perform()
    {
        Debug.Log("Evolution start!");
        var isPokemon = owner.Chara.race.tag.Contains("pokemon");

        if (isPokemon == false) return false;

        string userPokemon = owner.Chara.race.id;

        var databaseRow = PokemonDatabase.GetPokemonInfoForSpecies(userPokemon);

        if (databaseRow == null) return false;

        if (databaseRow.evolution.IsNullOrWhiteSpace()) return false; //if there is no evolution.

        owner.Chara.AddCondition(new EvolvingState() { id = 60000 }, true);

        owner.Chara.PlaySound("spell_bolt", 3.1f);

        return true;
    }
}

