using System;
using System.Collections;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(MorePots.MorePotsMod), "More Pots", "1.0.0", "Ashton")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace MorePots
{
    // Adds the Terracotta Pot and the Modern Pot: slightly better versions of the
    // Moisture-Preserving Pot and the Air Pot, sold only by Oscar at the warehouse.
    public class MorePotsMod : MelonMod
    {
        internal static MelonLogger.Instance Log;

        internal static readonly PotSpec[] Pots =
        {
            new PotSpec
            {
                Id = "morepots_terracottapot",
                Name = "Terracotta Pot",
                Flavor = "Thick fired-clay pot that keeps the soil cool and damp.",
                PrefabName = "TerracottaPot_Built",
                Resource = "MorePots.terracotta_pot.bin",
            },
            new PotSpec
            {
                Id = "morepots_modernpot",
                Name = "Modern Pot",
                Flavor = "Glazed ceramic planter on an oak stand, with a built-in drainage reservoir.",
                PrefabName = "ModernPot_Built",
                Resource = "MorePots.modern_pot.bin",
            },
        };

        public override void OnInitializeMelon()
        {
            Log = LoggerInstance;
            MelonPreferences_Category cat = MelonPreferences.CreateCategory("MorePots", "More Pots");
            // Terracotta: a step up from the Moisture-Preserving Pot ($50, 40% slower drain).
            Pots[0].CreateEntries(cat, "Terracotta", 65f, 1.05f, 0.55f, 1f);
            // Modern: a step up from the Air Pot ($120, +15% growth, +30% drain).
            Pots[1].CreateEntries(cat, "Modern", 150f, 1.20f, 1.15f, 1f);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Main")
                return;
            try
            {
                // Registry drops runtime items on every scene change, so re-register each time
                // Main loads. This runs before the save's loaders spawn placed pots.
                PotFactory.EnsureRegistered();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to register the new pots: " + ex);
                return;
            }
            MelonCoroutines.Start(AddToOscarWhenReady());
        }

        private static IEnumerator AddToOscarWhenReady()
        {
            float timeout = Time.realtimeSinceStartup + 300f;
            while (Time.realtimeSinceStartup < timeout)
            {
                bool loading = Singleton<LoadManager>.InstanceExists && Singleton<LoadManager>.Instance.IsLoading;
                if (!loading && OscarShop.TryAddListings())
                    yield break;
                yield return new WaitForSeconds(1f);
            }
            Log.Warning("Gave up waiting for Oscar's shop; the new pots aren't for sale this session.");
        }
    }
}
