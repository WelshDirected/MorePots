using UnityEngine;

namespace MorePots
{
    // Lists the pots in Oscar's warehouse shop, in person only (not in the phone delivery app).
    internal static class OscarShop
    {
        public static bool TryAddListings()
        {
            Oscar oscar = Object.FindObjectOfType<Oscar>();
            if (oscar == null || oscar.ShopInterface == null)
                return false;
            ShopInterface shop = oscar.ShopInterface;
            bool added = false;
            foreach (PotSpec spec in MorePotsMod.Pots)
            {
                BuildableItemDefinition def = PotFactory.GetDefinition(spec);
                if (def == null || shop.GetListing(spec.Id) != null)
                    continue;
                ShopListing listing = new ShopListing();
                listing.name = spec.Name;
                listing.Item = def;
                listing.CanBeDelivered = false;
                shop.Listings.Add(listing);
                Compat.CreateListingUI(shop, listing);
                listing.Initialize(shop);
                added = true;
                MorePotsMod.Log.Msg("Added " + spec.Name + " to " + shop.ShopName + " for $" + listing.Price + ".");
            }
            if (added)
                Compat.RefreshShownItems(shop);
            return true;
        }
    }
}
