using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.IO;
using PokeRaces;
using System;


[BepInPlugin("PokeRaces", "PokeRaces", "1.0.0.0")]
public class PokeRacesCore : BaseUnityPlugin
{
    private void Start()
    {
        new Harmony("PokeRaces").PatchAll();
    }

    private void OnStartCore()
    {

        Debug.Log("Poke races adding start...");
        var dir = Path.GetDirectoryName(Info.Location);
        SharedPokemonSprites.loadPath = dir + "/Texture/";
        CustomEffects.loadPath = dir + "/Particles/";
        var excel = dir + "/PokeRaces.xlsx";
        var sources = Core.Instance.sources;

        ClassCache.caches.Create<ActEvolve>("ActEvolve", "PokeRaces");

        //   ModUtil.ImportExcel(excel, "Chara", sources.charas);
        //   ModUtil.ImportExcel(excel, "CharaText", sources.charaText);

        ModUtil.ImportExcel(excel, "Elements", sources.elements);
        ModUtil.ImportExcel(excel, "Race", sources.races);
        ModUtil.ImportExcel(excel, "Stats", sources.stats);

        PokemonDatabase.pokemon_data = ScriptableObject.CreateInstance<SourcePokemon>();

        ModUtil.ImportExcel(excel, "Pokemon", PokemonDatabase.pokemon_data);

        Debug.Log("Loading Pokemon sprites...");

        SharedPokemonSprites.Init();

        Debug.Log("Adding icons to set...");


        string[] spriteDir = Directory.GetFiles(dir + "/Icons");
        foreach (string path in spriteDir)
        {
            Texture2D tex = IO.LoadPNG(path);
            tex.name = Path.GetFileNameWithoutExtension(path);
            Sprite newSprite = Sprite.Create(tex,new Rect(0,0,tex.width, tex.height), Vector2.one * 0.5f);
            newSprite.name = tex.name;
            SpriteSheet.Add(newSprite);
        }

        //Manually adding Pokemon type colors to the library.
        EClass.setting.elements["pTypeNormal"] = new ElementRef() { colorSprite = Color.white, colorTrail = new Color(1f,1f,1f, 0.03f) };
        EClass.setting.elements["pTypeGrass"] = new ElementRef() { colorSprite = Color.white, colorTrail = new Color(0.12f, 0.96f, 0.06f, 0.03f) };
        EClass.setting.elements["pTypePoison"] = new ElementRef() { colorSprite = Color.white, colorTrail = new Color(1f, 1f, 1f, 0.03f) };

        CustomEffects.InitCustomFFX();

        Debug.Log("Poke races added!");
    }
}

[HarmonyPatch(typeof(Chara), nameof(Chara.GetSprite))]
class PokeTextureOverride{
    //Changes sprites to instead use ones based on race instad.
    static void Prefix(Chara __instance)
    {
        SharedPokemonSprites.TryApplyPokemonSprites(__instance);
    }
}

[HarmonyPatch(typeof(Chara), nameof(Chara.OnCreate))]
class PokeTextureOverride2
{
    //Changes sprites to instead use ones based on race instad.
    static void Prefix(Chara __instance)
    {
        var isPokemon = __instance.race.tag.Contains("pokemon");

        if(isPokemon)
        {
            SharedPokemonSprites.TryApplyPokemonSprites(__instance);
            PokemonDatabase.TeachPokemonLevelupMoves(__instance);
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
//used to map the effects of Pokemon moves without changing their elements. Until I make custom ffx for them, of course...
/*[HarmonyPatch(typeof(Effect), nameof(Effect.Get) ,new Type[] { typeof(string) })]
class EffectStyleOverride
{
    //Changes sprites to instead use ones based on race instad.
    static void Prefix(ref string id)
    {
        switch (id)
        {
            case "pTypeNormal":
                id = "eleImpact";
                break;
            case "pTypeGrass":
                id = "eleAcid";
                break;
            case "pTypePoison":
                id = "elePoison";
                break;
        }
    }
}*/

//testomg
[HarmonyPatch(typeof(EffectManager.EffectList), nameof(EffectManager.EffectList.Get))]
class EffectStyleOverride
{
    //Changes sprites to instead use ones based on race instad.
    static void Prefix(ref string id)
    {
        id = id.Replace("peNormal", "Sound");
        id = id.Replace("peGrass", "Acid");
        id = id.Replace("pePoison", "Poison");
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

        SharedPokemonSprites.ApplyPokemonSpritesToVariation(__result.variation, pokemonName);
        __result.Build(); //rebuild to push the new sprites
    }
}

[HarmonyPatch(typeof(Chara), nameof(Chara.OnSleep), new Type[] { typeof(int), typeof(int) })]
class LevelupCheck
{
    static void Prefix(Chara __instance)
    {
        var isPokemon = __instance.race.tag.Contains("pokemon");
         if (isPokemon) {
            PokemonDatabase.TeachPokemonLevelupMoves(__instance);
        }
    }
}
