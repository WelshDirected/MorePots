using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace MorePots
{
    // Swaps the cloned Plastic Pot's mesh for a custom model (Models/*.glb, baked by
    // tools/glb_to_pot.py and embedded in the DLL). This is how the game builds its own Air and
    // Moisture-Preserving pots: the plastic mesh is switched off and a new visual goes under
    // "Model". The soil is resized to fit the new pot's cavity.
    internal static class PotModel
    {
        public const string RootName = "MorePotsModel";
        private const float SoilDiscRadius = 0.262f; // radius of the game's dirt mesh at scale 1
        private const float RimGap = 0.04f;          // soil surface sits this far below the rim

        private class Mat { public string Name; public Color Color; public float Metallic, Roughness; public int Flags; }
        private class Sub { public int Material; public Vector3[] Positions; public Vector3[] Normals; public int[] Indices; }
        private class Data
        {
            public List<Mat> Mats = new List<Mat>();
            public List<Sub> Subs = new List<Sub>();
            public float Height, Radius, CavityTopY, CavityTopR, CavityBottomY, CavityBottomR;
        }

        public static bool Apply(Pot pot, PotSpec spec)
        {
            try
            {
                Data data = Load(spec.Resource);
                Transform model = pot.transform.Find("Model");
                Transform oldPot = model != null ? model.Find("Pot") : null;
                Transform soilMin = model != null ? model.Find("Soil Min Transform") : null;
                Transform soilMax = model != null ? model.Find("Soil Max Transform") : null;
                Transform soilContainer = model != null ? model.Find("Soil Container") : null;
                if (oldPot == null || soilMin == null || soilMax == null)
                {
                    MorePotsMod.Log.Error("Plastic Pot layout not recognised; can't build " + spec.Name + ".");
                    return false;
                }

                MeshRenderer oldRenderer = oldPot.GetComponent<MeshRenderer>();
                Material template = oldRenderer != null ? oldRenderer.sharedMaterial : null;
                if (template == null)
                    throw new InvalidOperationException("no material on the Plastic Pot to base the new materials on");

                // Size the pot so the full-soil surface stays where the game expects it (plants,
                // the pour target and the moisture display are all placed relative to it).
                float soilTopY = soilMax.localPosition.y;
                float scale = (soilTopY + RimGap) / data.CavityTopY;

                GameObject root = new GameObject(RootName);
                root.layer = oldPot.gameObject.layer;
                root.transform.SetParent(model, false);
                root.transform.localScale = Vector3.one * scale;

                GameObject body = new GameObject("Body");
                body.layer = oldPot.gameObject.layer;
                body.transform.SetParent(root.transform, false);
                Mesh mesh = BuildMesh(data, spec.Name);
                body.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer r = body.AddComponent<MeshRenderer>();
                Material[] mats = new Material[data.Subs.Count];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = MakeMaterial(template, data.Mats[data.Subs[i].Material], spec.Name);
                r.sharedMaterials = mats;
                r.shadowCastingMode = ShadowCastingMode.On;
                r.receiveShadows = true;

                // Same collider setup as the plastic mesh (a solid one and a trigger).
                foreach (MeshCollider old in oldPot.GetComponents<MeshCollider>())
                {
                    MeshCollider c = body.AddComponent<MeshCollider>();
                    c.sharedMesh = mesh;
                    c.convex = old.convex || old.isTrigger;
                    c.isTrigger = old.isTrigger;
                }
                oldPot.gameObject.SetActive(false);

                // Soil: full at the top of the cavity, empty at its floor, never poking through the walls.
                float topR = Mathf.Lerp(data.CavityBottomR, data.CavityTopR,
                    Mathf.InverseLerp(data.CavityBottomY, data.CavityTopY, soilTopY / scale)) * scale;
                soilMax.localScale = Vector3.one * (topR * 0.96f / SoilDiscRadius);
                soilMin.localPosition = new Vector3(soilMin.localPosition.x, data.CavityBottomY * scale + 0.004f, soilMin.localPosition.z);
                soilMin.localScale = Vector3.one * (data.CavityBottomR * scale * 0.96f / SoilDiscRadius);
                if (soilContainer != null)
                {
                    soilContainer.localPosition = soilMin.localPosition;
                    soilContainer.localScale = soilMin.localScale;
                }

                MorePotsMod.Log.Msg(string.Format("{0} model: scale {1:0.00}, {2:0.00} m tall, {3:0.00} m wide.",
                    spec.Name, scale, data.Height * scale, data.Radius * scale * 2f));
                return true;
            }
            catch (Exception ex)
            {
                MorePotsMod.Log.Error("Building the " + spec.Name + " model failed: " + ex);
                return false;
            }
        }

        private static Mesh BuildMesh(Data data, string name)
        {
            List<Vector3> pos = new List<Vector3>();
            List<Vector3> nrm = new List<Vector3>();
            foreach (Sub s in data.Subs) { pos.AddRange(s.Positions); nrm.AddRange(s.Normals); }

            Mesh mesh = new Mesh();
            mesh.name = name;
            mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;
            mesh.vertices = pos.ToArray();
            mesh.normals = nrm.ToArray();
            mesh.subMeshCount = data.Subs.Count;
            int offset = 0;
            for (int i = 0; i < data.Subs.Count; i++)
            {
                Sub s = data.Subs[i];
                int[] tris = new int[s.Indices.Length];
                for (int k = 0; k < tris.Length; k++)
                    tris[k] = s.Indices[k] + offset;
                mesh.SetTriangles(tris, i);
                offset += s.Positions.Length;
            }
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material MakeMaterial(Material template, Mat m, string potName)
        {
            Material mat = new Material(template);
            mat.name = potName + " " + m.Name;
            mat.hideFlags = HideFlags.DontUnloadUnusedAsset;
            foreach (string tex in new[] { "_BaseMap", "_MainTex", "_BumpMap", "_MetallicGlossMap", "_OcclusionMap", "_EmissionMap", "_DetailAlbedoMap", "_DetailNormalMap" })
                if (mat.HasProperty(tex)) mat.SetTexture(tex, null);
            foreach (string kw in new[] { "_NORMALMAP", "_METALLICSPECGLOSSMAP", "_OCCLUSIONMAP", "_DETAIL_MULX2", "_PARALLAXMAP", "_EMISSION" })
                mat.DisableKeyword(kw);

            Color c = new Color(m.Color.r, m.Color.g, m.Color.b, 1f).gamma; // glTF colours are linear
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", m.Metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 1f - m.Roughness);
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", (m.Flags & 2) != 0 ? 0f : 2f);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black);
            return mat;
        }

        private static readonly Dictionary<string, Data> cache = new Dictionary<string, Data>();

        private static Data Load(string resource)
        {
            Data d;
            if (cache.TryGetValue(resource, out d))
                return d;
            Stream s = typeof(PotModel).Assembly.GetManifestResourceStream(resource);
            if (s == null)
                throw new FileNotFoundException("embedded model missing: " + resource);
            d = new Data();
            using (BinaryReader r = new BinaryReader(s))
            {
                if (new string(r.ReadChars(4)) != "MPOT" || r.ReadInt32() != 1)
                    throw new InvalidDataException("bad pot model data in " + resource);
                for (int n = r.ReadInt32(); n > 0; n--)
                    d.Mats.Add(new Mat { Name = Str(r), Color = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                        Metallic = r.ReadSingle(), Roughness = r.ReadSingle(), Flags = r.ReadInt32() });
                for (int n = r.ReadInt32(); n > 0; n--)
                {
                    Sub sub = new Sub { Material = r.ReadInt32() };
                    int verts = r.ReadInt32();
                    sub.Positions = new Vector3[verts];
                    sub.Normals = new Vector3[verts];
                    for (int i = 0; i < verts; i++) sub.Positions[i] = V3(r);
                    for (int i = 0; i < verts; i++) sub.Normals[i] = V3(r);
                    sub.Indices = new int[r.ReadInt32()];
                    for (int i = 0; i < sub.Indices.Length; i++) sub.Indices[i] = r.ReadInt32();
                    d.Subs.Add(sub);
                }
                d.Height = r.ReadSingle(); d.Radius = r.ReadSingle();
                d.CavityTopY = r.ReadSingle(); d.CavityTopR = r.ReadSingle();
                d.CavityBottomY = r.ReadSingle(); d.CavityBottomR = r.ReadSingle();
            }
            cache[resource] = d;
            return d;
        }

        private static string Str(BinaryReader r) { return System.Text.Encoding.UTF8.GetString(r.ReadBytes(r.ReadInt32())); }
        private static Vector3 V3(BinaryReader r) { return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()); }
    }
}
