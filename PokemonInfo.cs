using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

public static class PokemonDatabase
{
    public static SourcePokemon pokemon_data;

    public static SourcePokemon.Row GetPokemonInfoForSpecies(string speciesName)
    {
        foreach (var row in pokemon_data.rows)
        {
            if (row.id == speciesName) return row;
        }

        return null;
    }

    public static void TeachPokemonLevelupMoves(Chara referenceChar, bool silent = false)
    {
        if (referenceChar == null || referenceChar.race == null || referenceChar.race.tag == null) return;
        if (!referenceChar.race.tag.Contains("pokemon")) return;
        int level = referenceChar.Evalue(65000);
        var PokemonInfo = GetPokemonInfoForSpecies(referenceChar.race.id);
        List<int> skillsToKnow = new List<int>();

        if ( PokemonInfo.evolveLevel > 0 && level > PokemonInfo.evolveLevel) { skillsToKnow.Add(62020); } //Adds evolution at the right level.
        foreach (var set in PokemonInfo.levelUpMoves)
        {
            string[] split = set.Split('/');
            int gainLevel = int.Parse(split[0]);
            string moveName = "pAct" + split[1];
            if (level >= gainLevel)
            {
                skillsToKnow.Add(EClass.sources.elements.alias[moveName].id);
            }
        }
        foreach (var skill in skillsToKnow)
        {
            bool alreadyHasAbility = referenceChar.HasElement(skill);
            if (alreadyHasAbility == false)
            {
                if (silent)
                {
                    Element orCreateElement = referenceChar.elements.GetOrCreateElement(skill);
                    if (orCreateElement.ValueWithoutLink == 0)
                    {
                        referenceChar.elements.ModBase(orCreateElement.id, 1);
                    }
                }
                else
                {
                    referenceChar.GainAbility(skill);
                }
            }
        }
    }

    public static int ConvertPokemonTypeToBaseDamageType(int pokemonDamageType)
    {
        switch (pokemonDamageType)
        {
            case 62000: //Normal
            case 62001: //Fighting
            case 62006: //Bug
                return 0; //Void
            case 62002: //Flying
                return 917; //Sound
            case 62003: //Poison
                return 915; //Poison
            case 62004: //Ground
            case 62008: //steel
                return 925; //Impact
            case 62007: //Ghost
                return 916; //Nether
            case 62009: //Fire
                return 910; //Fire
            case 62010: //Water
            case 62014: //Ice
                return 911; //Cold
            case 62011: //Grass
                return 923; //Acid
            case 62012: //Electric
                return 912; //Lightning
            case 62013: //Psychic
                return  914; //Mind
            case 62015: //Dragon
                return 920; //Chaos
            case 62016: //Dark
                return 913; //Darkness
            case 62017: //Fairy
                return 922; //Ether
        }
        return pokemonDamageType;
    }
}

