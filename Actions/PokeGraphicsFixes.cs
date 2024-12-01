using HarmonyLib;
using PokeRaces;

class PokeGraphicsFixes
{

    [HarmonyPatch(typeof(Chara), nameof(Chara.OnCreate))]
    class PokeTextureOverride2
    {
        //Changes sprites to instead use ones based on race instad.
        static void Postfix(Chara __instance)
        {
            var isPokemon = __instance.race.tag.Contains("pokemon");

            if (isPokemon)
            {
                SharedPokemonSprites.TryApplyPokemonSprites(__instance);
                PokemonDatabase.TeachPokemonLevelupMoves(__instance, true);
            }
        }
    }

    [HarmonyPatch(typeof(Chara), nameof(Chara.ChangeRace))]
    class PokeTextureOverride4
    {
        //Changes sprites to instead use ones based on race instad.
        static void Postfix(Chara __instance)
        {
            var character = __instance;
            var isPokemon = character.race.tag.Contains("pokemon");
            if (isPokemon)
            {
                SharedPokemonSprites.TryApplyPokemonSprites(character);
            }
        }
    }

    [HarmonyPatch(typeof(UICharaMaker), nameof(UICharaMaker.Refresh))] //Updates char editor with new player sprites.
    class PlayerRaceChangeSpriteUpdate
    {
        //Changes sprites to instead use ones based on race instad.
        static void Postfix(UICharaMaker __instance)
        {
            __instance.portrait.SetChara(__instance.chara);
        }
    }


    [HarmonyPatch(typeof(EffectManager.EffectList), nameof(EffectManager.EffectList.Get))] //Seriously gotta find a way to add custom effects to the game...
    class EffectStyleOverride
    {
        static void Prefix(ref string id)
        {
            id = id.Replace("peNormal", "Sound");
            id = id.Replace("peGrass", "Acid");
            id = id.Replace("pePoison", "Poison");
            id = id.Replace("peFighting", "Sound");
            id = id.Replace("peBug", "Sound");
            id = id.Replace("peFlying", "Sound");
            id = id.Replace("peSteel", "Impact");
            id = id.Replace("peGround", "Impact");
            id = id.Replace("peGhost", "Nether");
            id = id.Replace("peFire", "Fire");
            id = id.Replace("peWater", "Cold");
            id = id.Replace("peIce", "Cold");
            id = id.Replace("peElectric", "Lightning");
            id = id.Replace("pePsychic", "Mind");
            id = id.Replace("peDark", "Darkness");
            id = id.Replace("peFairy", "Ether");
        }
    }


    //Below is the hook used so we can force a variation load.
    [HarmonyPatch(typeof(PCC), nameof(PCC.Get))]
    class PokeTextureOverride3
    {
        //Changes sprites to instead use ones based on race instad.
        static void Postfix(ref PCC __result)
        {
            if (__result.data == null) return;

            var isPokemon = __result.data.map.ContainsKey("pokemon");

            if (isPokemon == false) return;

            var pokemonName = __result.data.map["pokemon"][0];

            if (__result.variation == null) { __result.Build(true); }

            var doNotChange = __result.variation == null || __result.variation.tex.name == pokemonName; // avoid infinite loops AND make things a teeny bit more efficient!
            if (doNotChange) return;

            Debug.Log("Overloading PCC with pokemon sprites.");

            if (__result.variation == null) return;

            SharedPokemonSprites.ApplyPokemonSpritesToVariation(__result.variation, pokemonName, true);
            __result.Build(); //rebuild to push the new sprites
        }
    }
}

