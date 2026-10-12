using MelonLoader;

namespace MorePots
{
    // One modded pot: its identity, model and stats. Stats are multipliers on the Plastic Pot,
    // the template every pot is cloned from (the vanilla Air and Moisture-Preserving pots are
    // built the same way).
    internal class PotSpec
    {
        public string Id;
        public string Name;
        public string Flavor;      // first sentence of the description
        public string PrefabName;
        public string Resource;    // embedded baked model

        public MelonPreferences_Entry<float> Price;
        public MelonPreferences_Entry<float> GrowSpeed;   // 1.2 = 20% faster growth
        public MelonPreferences_Entry<float> MoistureDrain; // 0.55 = soil dries 45% slower
        public MelonPreferences_Entry<float> Yield;       // 1.0 = same yield as the Plastic Pot

        public void CreateEntries(MelonPreferences_Category cat, string key, float price, float growSpeed, float drain, float yield)
        {
            Price = cat.CreateEntry(key + "Price", price, Name + " price", "Price at Oscar's warehouse.");
            GrowSpeed = cat.CreateEntry(key + "GrowSpeed", growSpeed, Name + " grow speed",
                "Growth speed multiplier (1.0 = Plastic Pot, 1.15 = Air Pot).");
            MoistureDrain = cat.CreateEntry(key + "MoistureDrain", drain, Name + " moisture drain",
                "Moisture loss multiplier (1.0 = Plastic Pot, 0.6 = Moisture-Preserving Pot, 1.3 = Air Pot).");
            Yield = cat.CreateEntry(key + "Yield", yield, Name + " yield", "Yield multiplier (1.0 = Plastic Pot).");
        }

        // Description in the style of the game's own pots, built from the actual stats.
        public string Describe()
        {
            string s = Flavor;
            s += Effect(GrowSpeed.Value, "Boosts plant growth speed by {0}%.", "Plants grow {0}% slower.");
            s += Effect(MoistureDrain.Value, "Soil loses moisture {0}% faster.", "Soil will lose moisture {0}% slower.");
            s += Effect(Yield.Value, "Increases yield by {0}%.", "Reduces yield by {0}%.");
            return s;
        }

        private static string Effect(float mult, string up, string down)
        {
            int pct = (int)System.Math.Round((mult - 1f) * 100f);
            if (pct == 0)
                return "";
            return " " + string.Format(pct > 0 ? up : down, System.Math.Abs(pct));
        }
    }
}
