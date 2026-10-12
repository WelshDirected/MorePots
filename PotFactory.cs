using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MorePots
{
    // Builds each new pot at runtime from the game's own Plastic Pot: the item definition, the
    // placeable prefab (kept as an inactive template so its scripts never run) and the icon.
    internal static class PotFactory
    {
        private const string PlasticPrefab = "CheapPlasticPot_Built";

        private static GameObject holder;
        private static readonly Dictionary<string, Pot> templates = new Dictionary<string, Pot>();
        private static readonly Dictionary<string, BuildableItemDefinition> definitions = new Dictionary<string, BuildableItemDefinition>();

        public static BuildableItemDefinition GetDefinition(PotSpec spec)
        {
            BuildableItemDefinition d;
            return definitions.TryGetValue(spec.Id, out d) ? d : null;
        }

        public static void EnsureRegistered()
        {
            BuildableItemDefinition plastic = null;
            foreach (PotSpec spec in MorePotsMod.Pots)
            {
                if (!definitions.ContainsKey(spec.Id))
                {
                    if (plastic == null)
                        plastic = FindPlasticPotDefinition();
                    if (plastic == null)
                        throw new InvalidOperationException("Could not find the Plastic Pot definition.");
                    Build(spec, plastic);
                }

                if (Registry.GetItem(spec.Id) == null)
                    Singleton<Registry>.Instance.AddToRegistry(definitions[spec.Id]);

                // Register the prefab with FishNet so the pot spawns correctly on every client.
                // Every peer runs this in the same order, so the prefab IDs line up.
                NetworkObject nob = templates[spec.Id].GetComponent<NetworkObject>();
                InstanceFinder.NetworkManager.SpawnablePrefabs.AddObject(nob, true);
                MorePotsMod.Log.Msg(spec.Name + " registered (prefab id " + nob.PrefabId + ").");
            }
        }

        private static void Build(PotSpec spec, BuildableItemDefinition plasticDef)
        {
            Pot plasticPrefab = Compat.As<Pot>(plasticDef.BuiltItem);
            if (holder == null)
            {
                holder = new GameObject("MorePots_Templates");
                holder.SetActive(false);
                Object.DontDestroyOnLoad(holder);
            }

            GameObject go = Object.Instantiate(plasticPrefab.gameObject, holder.transform);
            go.name = spec.PrefabName;
            Pot pot = go.GetComponent<Pot>();
            if (!PotModel.Apply(pot, spec))
            {
                Object.Destroy(go);
                throw new InvalidOperationException("model for " + spec.Name + " could not be built");
            }

            // Stats, relative to the Plastic Pot.
            pot.GrowSpeedMultiplier = plasticPrefab.GrowSpeedMultiplier * spec.GrowSpeed.Value;
            pot.YieldMultiplier = plasticPrefab.YieldMultiplier * spec.Yield.Value;
            Compat.SetMoistureDrain(pot, Compat.GetMoistureDrain(plasticPrefab) * spec.MoistureDrain.Value);
            templates[spec.Id] = pot;

            BuildableItemDefinition def = Object.Instantiate(plasticDef);
            def.name = spec.PrefabName.Replace("_Built", "");
            def.ID = spec.Id;
            def.Name = spec.Name;
            def.Description = spec.Describe();
            def.BasePurchasePrice = spec.Price.Value;
            def.RequiresLevelToPurchase = false; // Oscar's warehouse is already late-game
            def.BuiltItem = pot;
            def.hideFlags = HideFlags.DontUnloadUnusedAsset;
            Sprite icon = PotIcon.Render(pot.transform.Find("Model/" + PotModel.RootName), spec.Name, plasticDef.Icon);
            if (icon != null)
                def.Icon = icon;
            definitions[spec.Id] = def;

            MorePotsMod.Log.Msg(string.Format("Built {0}: grow speed x{1:0.00}, yield x{2:0.00}, moisture drain {3:0.00}/h (Plastic Pot: x{4:0.00}, x{5:0.00}, {6:0.00}/h).",
                spec.Name, pot.GrowSpeedMultiplier, pot.YieldMultiplier, Compat.GetMoistureDrain(pot),
                plasticPrefab.GrowSpeedMultiplier, plasticPrefab.YieldMultiplier, Compat.GetMoistureDrain(plasticPrefab)));
        }

        private static BuildableItemDefinition FindPlasticPotDefinition()
        {
            BuildableItemDefinition fallback = null;
            var items = Singleton<Registry>.Instance.GetAllItems();
            for (int i = 0; i < items.Count; i++)
            {
                BuildableItemDefinition b = Compat.As<BuildableItemDefinition>(items[i]);
                if (b == null || b.BuiltItem == null || Compat.As<Pot>(b.BuiltItem) == null || b.ID.StartsWith("morepots_"))
                    continue;
                if (b.BuiltItem.gameObject.name == PlasticPrefab)
                    return b;
                // Any pot that still shows the stock plastic mesh will do.
                Transform mesh = b.BuiltItem.transform.Find("Model/Pot");
                if (fallback == null && mesh != null && mesh.gameObject.activeSelf)
                    fallback = b;
            }
            return fallback;
        }
    }
}
