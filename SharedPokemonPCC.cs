﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;


namespace PokeRaces
{
    public static class SharedPokemonSprites
    {
        public static Dictionary<string, Texture2D> pokemon_sprite_library = new Dictionary<string, Texture2D>();

        public static string loadPath = "";

        static bool initialized = false;


        public static void Init()
        {
            if (initialized) return;

            initialized = true;

            Debug.Log("Loading Pokemon sprites...");

            foreach (SourcePokemon.Row row in PokemonDatabase.pokemon_data.rows)
            {
                var name = row.id;

                Debug.Log("Loading sprites for " + name + "...");
                AddPokemonSpriteToMap(name);
            }
        }

        static Texture2D AddPokemonSpriteToMap(string name)
        {
            string spritePath = loadPath + name + ".png";
            Texture2D tex = IO.LoadPNG(spritePath);
            tex.name = name;
            pokemon_sprite_library[name] = tex;

            return tex;
        }

        public static void TryApplyPokemonSprites(Chara __instance)
        {
            PCCData pccData = __instance.pccData;
            if (pccData == null)
            {
                return;
            }
            if (__instance.race.tag != null && __instance.race.tag.Contains("pokemon"))
            {
                string pokemonName = __instance.race.id;
                
                var pcc = PCC.Get(pccData);
                pccData.map["pokemon"] = new string[] {pokemonName};
                if (pcc == null) return;

                var variation = pcc.variation;

                if (variation == null) return;

                ApplyPokemonSpritesToVariation(pcc.variation, pokemonName);
                
                pcc.Build();
            }
            else
            {
                if (pccData.map.ContainsKey("pokemon")) pccData.map.Remove("pokemon");
                var pcc = PCC.Get(pccData);

                if (pcc == null) return;

                var variation = pcc.variation;

                if (variation == null || variation.tex == null) return;

                if (pokemon_sprite_library.ContainsKey(variation.tex.name))
                {
                    pcc.variation.tex = null; //please dont kill my sprites :(
                    pcc.KillVariation();
                    pcc.Build(true);
                }
            }
        }

        public static void ApplyPokemonSpritesToVariation(SpriteVariation variation, string pokemonName)
        {
            var targetSet = pokemon_sprite_library[pokemonName];

            if (targetSet == null) { targetSet = AddPokemonSpriteToMap(pokemonName); } //Sometimes. they just get deleted. and i unno why. probably memory clearing.

            //Sprite rescaling here in a kinda janky way!
            float oldSize = SpriteVariationManager.current.baseSize;

            SpriteVariationManager.current.baseSize /= PokemonDatabase.GetPokemonInfoForSpecies(pokemonName).spriteScale;

            variation.pivot = new Vector2(0.5f, 0.1f); //this pivot looks better for these sprites than the default one does.

            variation.tex = targetSet;
            variation.loopType = SpriteVariation.LoopType.restart;
            variation.BuildSprites(variation.tex);

            SpriteVariationManager.current.baseSize = oldSize;
        }
    }
}
