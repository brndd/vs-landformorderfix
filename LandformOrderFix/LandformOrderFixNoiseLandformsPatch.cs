using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods.NoObf;

namespace LandformOrderFix;

[HarmonyPatch]
public class LandformOrderFixNoiseLandformsPatch
{
    // public static Type TargetType()
    // {
    //     return AccessTools.TypeByName("Vintagestory.ServerMods.NoiseLandforms");
    // }

    public static MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("Vintagestory.ServerMods.NoiseLandforms");
        return AccessTools.Method(type, "LoadLandforms");
    }
    
    public static bool Prefix()
    {
        ICoreServerAPI api = LandformOrderFixModSystem.Api;
        var landformsField = AccessTools.Field(AccessTools.TypeByName("Vintagestory.ServerMods.NoiseLandforms"), "landforms");
        if (landformsField == null)
        {
            api.Logger.Debug("Could not find landforms field so bailing");
            return true;
        }

        IAsset asset = api.Assets.Get("worldgen/landforms.json");
        var landforms = asset.ToObject<LandformsWorldProperty>();

        //Sort the damn thing
        Array.Sort(landforms.Variants, (x, y) =>
        {
            string xCode = x?.Code ?? string.Empty;
            string yCode = y?.Code ?? string.Empty;
            return string.CompareOrdinal(xCode, yCode);
        });
        
        int quantityMutations = 0;

        for (int i = 0; i < landforms.Variants.Length; i++)
        {
            LandformVariant variant = landforms.Variants[i];
            variant.index = i;
            variant.Init(api.WorldManager, i);

            if (variant.Mutations != null)
            {
                quantityMutations += variant.Mutations.Length;
            }
        }

        landforms.LandFormsByIndex = new LandformVariant[quantityMutations + landforms.Variants.Length];

        // Mutations get indices after the parent ones
        for (int i = 0; i < landforms.Variants.Length; i++)
        {
            landforms.LandFormsByIndex[i] = landforms.Variants[i];
        }

        int nextIndex = landforms.Variants.Length;
        for (int i = 0; i < landforms.Variants.Length; i++)
        {
            LandformVariant variant = landforms.Variants[i];
            if (variant.Mutations != null)
            {
                for (int j = 0; j < variant.Mutations.Length; j++)
                {
                    LandformVariant variantMut = variant.Mutations[j];

                    if (variantMut.TerrainOctaves == null)
                    {
                        variantMut.TerrainOctaves = variant.TerrainOctaves;
                    }

                    if (variantMut.TerrainOctaveThresholds == null)
                    {
                        variantMut.TerrainOctaveThresholds = variant.TerrainOctaveThresholds;
                    }

                    if (variantMut.TerrainYKeyPositions == null)
                    {
                        variantMut.TerrainYKeyPositions = variant.TerrainYKeyPositions;
                    }

                    if (variantMut.TerrainYKeyThresholds == null)
                    {
                        variantMut.TerrainYKeyThresholds = variant.TerrainYKeyThresholds;
                    }


                    landforms.LandFormsByIndex[nextIndex] = variantMut;
                    variantMut.Init(api.WorldManager, nextIndex);
                    nextIndex++;
                }
            }
        }

        landformsField.SetValue(null, landforms);
        return false; //skip original
    }
    
}