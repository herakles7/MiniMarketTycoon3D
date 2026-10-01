#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MiniMarketTycoon.Editor
{
    [InitializeOnLoad]
    public static class CustomerPrefabGenerator
    {
        static CustomerPrefabGenerator()
        {
            EditorApplication.delayCall += CheckAndGeneratePrefab;
        }

        private static void CheckAndGeneratePrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab";
            if (!System.IO.File.Exists(prefabPath))
            {
                GeneratePrefab();
            }
        }

        [MenuItem("MiniMarket/Generate Customer Prefab")]
        public static void GeneratePrefab()
        {
            Stage11VisualPolishSetup.ExecuteFullOverhaul();
        }
    }
}
#endif
