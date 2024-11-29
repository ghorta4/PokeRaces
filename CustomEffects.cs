using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class CustomEffects
{

    public static string loadPath;

    public static void InitCustomFFX()
    {
        /*
        DirectoryInfo folders = new DirectoryInfo(loadPath);

        DirectoryInfo[] effects = folders.GetDirectories();

        foreach (DirectoryInfo di in effects)
        {
            FileInfo[] images = di.GetFiles();

            Texture2D[] holster = new Texture2D[images.Length];

            for(int i = 0; i < images.Length; i++)
            {
                holster[i] = IO.LoadPNG(images[i].FullName);
            }
            Sprite[] allSprites = new Sprite[images.Length];

            for(int i = 0; i < holster.Length; i++)
            {
                Texture2D tex = holster[i];
                allSprites[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            }

            Effect effectPrefab = Effect.Get("smoke_earthquake"); //use as a base for editing
            effectPrefab.sprites = allSprites;

            string usedName = "Element/" + di.Name;

            effectPrefab.name = usedName;
            effectPrefab.sr = effectPrefab.AddComponent();
            EffectManager.Instance.effects.list.Add(effectPrefab);
            EffectManager.Instance.effects.map.Add(effectPrefab.name, effectPrefab);
        }*/
    }
}

