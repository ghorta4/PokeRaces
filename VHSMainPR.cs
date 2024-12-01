using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.IO;
using PokeRaces;
using System;
using static UnityEngine.UI.GridLayoutGroup;


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

        ModUtil.ImportExcel(excel, "Chara", sources.charas);
        ModUtil.ImportExcel(excel, "CharaText", sources.charaText);

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

/*[HarmonyPatch(typeof(CharaActorPCC), nameof(CharaActorPCC.RefreshSprite))]
class PokeTextureLoadFix
{
    static void Prefix(CharaActorPCC __instance)
    {
        var chara = __instance.owner;
        var isPokemon = chara.race.tag.Contains("pokemon");
        if (isPokemon && !__instance.pcc.isBuilt)
        {
            //Late build for PCC in case something spawns without it.
            if (__instance.pcc == null)
            {
                __instance.pcc = PCC.Get((chara.renderer as CharaRenderer).pccData);
                SharedPokemonSprites.TryApplyPokemonSprites(chara);
            }
            if(!__instance.pcc.isBuilt)
            {
                __instance.pcc.Build();
            }
        }
    }
}*/

/*[HarmonyPatch(typeof(SpriteVariation), nameof(SpriteVariation.Kill))]
class AntiPokeTextureKill
{
    static void Prefix(SpriteVariation __instance)
    {
        Debug.LogWarning("tex was killed: " + __instance.tex.name);
    }
}*/



[HarmonyPatch(typeof(Card), nameof(Card.ResistLvFrom))]
class AIResistDetectionOverride //Fix resistance detection.
{
    static bool Prefix(ref int ele, ref int __result)
    {
        ele = PokemonDatabase.ConvertPokemonTypeToBaseDamageType(ele);
        if (ele == 0)
        {
            __result = 0;
            return false;
        }
        return true;
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
