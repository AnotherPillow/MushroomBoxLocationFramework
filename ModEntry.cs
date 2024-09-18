using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace MushroomBoxLocationFramework
{
    internal sealed class ModEntry : Mod {
        private static IMonitor _monitor = null!;
        private static string UniqueID = null!;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);

            // example patch, you'll need to edit this for your patch
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmCave), nameof(FarmCave.setUpMushroomHouse)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(SetUpMushroomHouse_Prefix))
            );
            
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            ModEntry._monitor = Monitor;
            ModEntry.UniqueID = this.ModManifest.UniqueID;

        }
        public void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo($"Mods/{this.ModManifest.UniqueID}/Boxes"))
            {
                Dictionary<string, bool> positions = new()
                    {
                        { "5,4", true},
                        { "5,6", true},
                        { "5,8", true},
                        { "7,4", true},
                        { "7,6", true},
                        { "7,8", true},
                    };

            e.LoadFrom(() => positions, AssetLoadPriority.Exclusive);
            }
        }


        internal static bool SetUpMushroomHouse_Prefix(StardewValley.Locations.FarmCave __instance)
        {   
            try
            {
                IDictionary<string, bool> positions = Game1.content.Load<IDictionary<string, bool>>($"Mods/{ModEntry.UniqueID}/Boxes");
                foreach (KeyValuePair<string, bool> entry in positions)
                {

                    // Continue if disabled
                    if (entry.Value != true) continue;
                    int[] numbers = entry.Key.Split(',').Select(int.Parse).ToArray();
                    
                    // Confirm the correct length
                    if (numbers.Length != 2)
                    {
                        _monitor.Log($"Invalid mushroom box location {entry.Key}. Will not spawn.", LogLevel.Warn);
                    }

                    SObject mushroomBox = ItemRegistry.Create<SObject>("(BC)128");
                    mushroomBox.fragility.Value = 2;
                    // The first (0th) element is the y, and the second (1st) element is the x.
                    __instance.setObject(new Vector2(numbers[1], numbers[0]), mushroomBox);

                }

                __instance.setObject(new Vector2(10f, 5f), ItemRegistry.Create<SObject>("(BC)Dehydrator"));
                


                return false;
                //return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetUpMushroomHouse_Prefix)}:\n{ex}", LogLevel.Error);
                return true;

            }
        }

    }
}