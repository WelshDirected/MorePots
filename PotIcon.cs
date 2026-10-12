using System;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MorePots
{
    // Renders a pot's shop/inventory icon with the same preview renderer the game's own
    // IconGenerator uses, so it matches the other item icons.
    internal static class PotIcon
    {
        private const int Size = 512;

        public static Sprite Render(Transform modelRoot, string name, Sprite fallback)
        {
            if (modelRoot == null)
                return fallback;
            GameObject proxy = null;
            AmbientMode oldMode = RenderSettings.ambientMode;
            Color oldAmbient = RenderSettings.ambientLight;
            try
            {
                proxy = BuildVisualProxy(modelRoot);
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Color.white;
                PreviewGenerator.PreviewDirection = new Vector3(-0.5f, -0.55f, 1f);
                PreviewGenerator.Padding = 0.06f;
                PreviewGenerator.BackgroundColor = new Color(0f, 0f, 0f, 0f);
                PreviewGenerator.OrthographicMode = false;
                PreviewGenerator.MarkTextureNonReadable = false;
                Texture2D tex = PreviewGenerator.GenerateModelPreview(proxy.transform, Size, Size, false, true);
                if (tex == null)
                    return fallback;
                tex.name = name + " Icon";
                Object.DontDestroyOnLoad(tex);
                Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                s.name = name + " Icon";
                return s;
            }
            catch (Exception ex)
            {
                MorePotsMod.Log.Warning("Icon render for " + name + " failed, using the Plastic Pot icon: " + ex.Message);
                return fallback;
            }
            finally
            {
                RenderSettings.ambientMode = oldMode;
                RenderSettings.ambientLight = oldAmbient;
                if (proxy != null)
                    Object.Destroy(proxy);
            }
        }

        // An active, script-free copy of the model (the template itself is inactive).
        private static GameObject BuildVisualProxy(Transform modelRoot)
        {
            GameObject root = new GameObject("MorePotsIconProxy");
            root.transform.position = new Vector3(0f, -500f, 0f);
            foreach (MeshRenderer mr in modelRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                    continue;
                GameObject part = new GameObject(mr.name);
                part.transform.SetParent(root.transform, false);
                Matrix4x4 m = modelRoot.worldToLocalMatrix * mr.transform.localToWorldMatrix;
                part.transform.localPosition = m.MultiplyPoint3x4(Vector3.zero);
                part.transform.localRotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
                part.transform.localScale = new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
                part.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                MeshRenderer r = part.AddComponent<MeshRenderer>();
                r.sharedMaterials = mr.sharedMaterials;
                r.shadowCastingMode = ShadowCastingMode.Off;
            }
            return root;
        }
    }
}
