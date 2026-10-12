#if IL2CPP
using Il2CppInterop.Runtime.InteropTypes;
#else
using System.Reflection;
using HarmonyLib;
#endif

namespace MorePots
{
    // The few places where Mono and IL2CPP code must differ.
    internal static class Compat
    {
#if IL2CPP
        // IL2CPP wrappers can't be cast with `as`/`is`; ask the native object instead.
        public static T As<T>(Il2CppObjectBase obj) where T : Il2CppObjectBase
        {
            return obj == null ? null : obj.TryCast<T>();
        }

        // Private game members are public in the generated interop assemblies.
        public static float GetMoistureDrain(GrowContainer c) { return c._moistureDrainPerHour; }
        public static void SetMoistureDrain(GrowContainer c, float perHour) { c._moistureDrainPerHour = perHour; }
        public static void CreateListingUI(ShopInterface shop, ShopListing listing) { shop.CreateListingUI(listing); }
        public static void RefreshShownItems(ShopInterface shop) { shop.RefreshShownItems(); }
#else
        private static readonly FieldInfo DrainField = AccessTools.Field(typeof(GrowContainer), "_moistureDrainPerHour");

        public static T As<T>(object obj) where T : class
        {
            return obj as T;
        }

        public static float GetMoistureDrain(GrowContainer c) { return (float)DrainField.GetValue(c); }
        public static void SetMoistureDrain(GrowContainer c, float perHour) { DrainField.SetValue(c, perHour); }

        public static void CreateListingUI(ShopInterface shop, ShopListing listing)
        {
            AccessTools.Method(typeof(ShopInterface), "CreateListingUI").Invoke(shop, new object[] { listing });
        }

        public static void RefreshShownItems(ShopInterface shop)
        {
            AccessTools.Method(typeof(ShopInterface), "RefreshShownItems").Invoke(shop, null);
        }
#endif
    }
}
