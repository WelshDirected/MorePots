// Game namespaces differ per branch: Mono keeps the original names, while MelonLoader's
// IL2CPP interop assemblies prefix them with "Il2Cpp". Build with -define:IL2CPP for IL2CPP.
#if IL2CPP
global using Il2CppScheduleOne;
global using Il2CppScheduleOne.DevUtilities;
global using Il2CppScheduleOne.Growing;
global using Il2CppScheduleOne.ItemFramework;
global using Il2CppScheduleOne.NPCs.CharacterClasses;
global using Il2CppScheduleOne.ObjectScripts;
global using Il2CppScheduleOne.Persistence;
global using Il2CppScheduleOne.UI.Shop;
global using Il2CppFishNet;
global using Il2CppFishNet.Object;
global using PreviewGenerator = Il2Cpp.RuntimePreviewGenerator;
#else
global using ScheduleOne;
global using ScheduleOne.DevUtilities;
global using ScheduleOne.Growing;
global using ScheduleOne.ItemFramework;
global using ScheduleOne.NPCs.CharacterClasses;
global using ScheduleOne.ObjectScripts;
global using ScheduleOne.Persistence;
global using ScheduleOne.UI.Shop;
global using FishNet;
global using FishNet.Object;
global using PreviewGenerator = RuntimePreviewGenerator;
#endif
