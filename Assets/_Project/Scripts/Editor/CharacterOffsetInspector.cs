using UnityEngine;
using UnityEditor;
using System.IO;

namespace MiniMarketTycoon.EditorTools
{
    public static class CharacterOffsetInspector
    {
        [MenuItem("MiniMarket/Fix Character Height & Delivery Boxes")]
        public static void FixCharacterAndBoxes()
        {
            RunAutoFix();
        }

        private static void RunAutoFix()
        {
            // 1. Inspect Kenney Character Model
            GameObject charAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/characterMedium.fbx");
            float calculatedFeetOffset = 0f;
            if (charAsset != null)
            {
                var smr = charAsset.GetComponentInChildren<SkinnedMeshRenderer>();
                var mf = charAsset.GetComponentInChildren<MeshFilter>();
                Bounds b = (smr != null && smr.sharedMesh != null) 
                    ? smr.sharedMesh.bounds 
                    : ((mf != null && mf.sharedMesh != null) ? mf.sharedMesh.bounds : new Bounds());
                Debug.Log($"[CharacterOffsetInspector] Raw characterMedium mesh bounds: center={b.center}, min={b.min}, max={b.max}, size={b.size}");
                calculatedFeetOffset = b.min.y;
            }

            // 2. Fix PF_Customer_NPC Prefab
            string prefabPath = "Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                string path = AssetDatabase.GetAssetPath(prefab);
                GameObject contents = PrefabUtility.LoadPrefabContents(path);

                Transform modelChild = contents.transform.Find("Model");
                if (modelChild != null)
                {
                    float scaleY = modelChild.localScale.y;
                    // If calculatedFeetOffset is negative (e.g. -1.88m in FBX space),
                    // in local space with scale 0.465, the feet are at -1.88 * 0.465 = -0.874m.
                    // To bring the feet up to Y=0, Model localPosition.y must be +0.874m!
                    float desiredModelY = Mathf.Abs(calculatedFeetOffset * scaleY);
                    if (desiredModelY < 0.1f) desiredModelY = 0.88f; // Safe fallback

                    modelChild.localPosition = new Vector3(0f, desiredModelY, 0f);
                    Debug.Log($"[CharacterOffsetInspector] Set PF_Customer_NPC Model localPosition.y to {desiredModelY:F3} (scale={scaleY}, rawMinY={calculatedFeetOffset})");
                }

                // Adjust NavMeshAgent
                var agent = contents.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.baseOffset = 0f;
                    agent.height = 1.75f;
                    agent.radius = 0.28f;
                }

                // Ensure CapsuleCollider is centered properly
                var col = contents.GetComponent<CapsuleCollider>();
                if (col != null)
                {
                    col.center = new Vector3(0f, 0.88f, 0f);
                    col.height = 1.75f;
                    col.radius = 0.28f;
                }

                PrefabUtility.SaveAsPrefabAsset(contents, path);
                PrefabUtility.UnloadPrefabContents(contents);
                Debug.Log("[CharacterOffsetInspector] Saved PF_Customer_NPC prefab with corrected height offset.");
            }

            // 3. Fix CustomerVisual.cs runtime instantiation logic
            // (We will also patch CustomerVisual.cs to apply this offset whenever a model is spawned)
        }
    }
}
