#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MiniMarketTycoon.Gameplay;
using MiniMarketTycoon.Store;

namespace MiniMarketTycoon.Editor
{
    /// <summary>
    /// Stage 13: Real Supermarket Overhaul - Player Manager, Physical Delivery & Supply Loop.
    /// Sets up:
    /// 1. Player Character (PlayerManagerController, CharacterController, Smooth WASD/Shift run, CarrySocket).
    /// 2. Interaction System (PlayerInteractionDetector + 3D WorldSpace InteractionPromptUI).
    /// 3. Dual Camera Controller (Player 3rd Person Follow <-> Isometric Tycoon with [C] key).
    /// 4. Manager Office Terminal (Desk + Computer + Wholesale Order System).
    /// 5. Exterior Recycling Dumpster (Disposes empty boxes with recycling cash bonus).
    /// 6. Delivery Drop Zone & Starter Delivery Boxes (Milk, Water, Soda boxes ready to pick up and restock).
    /// </summary>
    public static class Stage13RealSupermarketSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/03_Game.unity";

        [MenuItem("MiniMarket/Stage 13: Real Supermarket - Player & Supply Delivery ★")]
        public static void Execute()
        {
            Debug.Log("[Stage13] Setting up Real Supermarket Player & Supply Delivery System...");

            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Stage13] Could not open scene {SCENE_PATH}");
                return;
            }

            RuntimeAnimatorController rac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/AC_Customer.controller");
            if (rac == null)
            {
                rac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Resources/AC_Customer.controller");
            }

            // 1. Setup 3D Floating Interaction Prompt UI
            SetupInteractionPromptUI();

            // 2. Setup Player Manager Character
            var player = SetupPlayerManager(rac);

            // 3. Setup Camera Controller to follow player
            SetupCameraController(player);

            // 4. Setup Manager Office Desk & Computer
            SetupOfficeComputer();

            // 5. Setup Recycling Dumpster
            SetupRecyclingDumpster();

            // 6. Setup Delivery Service & Starter Boxes
            SetupDeliveryDropZone();

            // 7. Setup 3D Shelf Price Tags for Dynamic Pricing
            SetupShelfPriceTags();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Stage13] Real Supermarket Setup COMPLETE! Press Play in Unity to control your store manager.");
        }

        private static void SetupInteractionPromptUI()
        {
            var oldPrompt = GameObject.Find("Interaction_Prompt_UI");
            if (oldPrompt != null) Object.DestroyImmediate(oldPrompt);

            GameObject promptGo = new GameObject("Interaction_Prompt_UI");
            promptGo.AddComponent<InteractionPromptUI>();

            Debug.Log("[Stage13] Created Interaction_Prompt_UI in scene.");
        }

        private static PlayerManagerController SetupPlayerManager(RuntimeAnimatorController rac)
        {
            var oldPlayer = GameObject.Find("Player_Manager");
            if (oldPlayer != null) Object.DestroyImmediate(oldPlayer);

            GameObject empAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/character-employee.fbx");
            if (empAsset == null) empAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/characterMedium.fbx");

            GameObject playerGo = new GameObject("Player_Manager");
            playerGo.transform.position = new Vector3(0f, 0f, -3.5f); // Near market entrance
            playerGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            // Instantiate Model as child elevated by 0.82m
            GameObject modelGo;
            if (empAsset != null)
            {
                modelGo = (GameObject)PrefabUtility.InstantiatePrefab(empAsset);
                modelGo.name = "Model";
                modelGo.transform.SetParent(playerGo.transform, false);
                modelGo.transform.localPosition = new Vector3(0f, 0.82f, 0f);
                modelGo.transform.localRotation = Quaternion.identity;
                modelGo.transform.localScale = Vector3.one * 0.465f;

                // Remove existing colliders on model parts
                foreach (var col in modelGo.GetComponentsInChildren<Collider>(true))
                {
                    Object.DestroyImmediate(col);
                }
            }
            else
            {
                modelGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                modelGo.name = "Model";
                modelGo.transform.SetParent(playerGo.transform, false);
                modelGo.transform.localPosition = new Vector3(0f, 0.88f, 0f);
                modelGo.transform.localScale = new Vector3(0.6f, 0.88f, 0.6f);
                foreach (var col in modelGo.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(col);
            }

            // Character Controller on root
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.75f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.88f, 0f);
            cc.stepOffset = 0.25f;

            // Animator on Model
            var anim = modelGo.GetComponent<Animator>();
            if (anim == null) anim = modelGo.AddComponent<Animator>();
            if (rac != null) anim.runtimeAnimatorController = rac;
            anim.applyRootMotion = false;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            // Carry Socket
            GameObject socketGo = new GameObject("CarrySocket");
            socketGo.transform.SetParent(playerGo.transform, false);
            socketGo.transform.localPosition = new Vector3(0f, 0.95f, 0.55f);

            // Player Scripts
            var playerCtrl = playerGo.AddComponent<PlayerManagerController>();
            playerGo.AddComponent<PlayerInteractionDetector>();

            EditorUtility.SetDirty(playerGo);
            Debug.Log("[Stage13] Created Player_Manager character at (0, 0, -3.5).");
            return playerCtrl;
        }

        private static void SetupCameraController(PlayerManagerController player)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            var camCtrl = cam.GetComponent<CameraController>();
            if (camCtrl == null) camCtrl = cam.GetComponentInParent<CameraController>();

            if (camCtrl != null)
            {
                camCtrl.SetCameraMode(CameraController.CameraMode.FollowPlayer);
                EditorUtility.SetDirty(camCtrl);
                Debug.Log("[Stage13] Configured CameraController to FollowPlayer mode (Press [C] in-game to toggle).");
            }
        }

        private static void SetupOfficeComputer()
        {
            var oldOffice = GameObject.Find("Office_Desk_Terminal");
            if (oldOffice != null) Object.DestroyImmediate(oldOffice);

            GameObject officeGo = new GameObject("Office_Desk_Terminal");
            officeGo.transform.position = new Vector3(-4.2f, 0f, -3.2f);
            officeGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            // Desk table
            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "DeskMesh";
            desk.transform.SetParent(officeGo.transform, false);
            desk.transform.localScale = new Vector3(1.4f, 0.8f, 0.8f);
            desk.transform.localPosition = new Vector3(0f, 0.4f, 0f);

            // Computer Monitor
            GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monitor.name = "MonitorMesh";
            monitor.transform.SetParent(officeGo.transform, false);
            monitor.transform.localScale = new Vector3(0.6f, 0.45f, 0.08f);
            monitor.transform.localPosition = new Vector3(0f, 1.05f, 0.15f);

            // Screen glow light
            GameObject glow = new GameObject("ScreenGlow");
            glow.transform.SetParent(monitor.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            var l = glow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.3f, 0.7f, 1.0f);
            l.intensity = 1.2f;
            l.range = 2.5f;

            // Box Collider on root for interaction
            BoxCollider col = officeGo.AddComponent<BoxCollider>();
            col.size = new Vector3(1.6f, 1.2f, 1.2f);
            col.center = new Vector3(0f, 0.6f, 0f);
            col.isTrigger = false;

            officeGo.AddComponent<StoreOfficeComputer>();

            EditorUtility.SetDirty(officeGo);
            Debug.Log("[Stage13] Created Office_Desk_Terminal at (-4.2, 0, -3.2).");
        }

        private static void SetupRecyclingDumpster()
        {
            var oldDumpster = GameObject.Find("Recycling_Dumpster");
            if (oldDumpster != null) Object.DestroyImmediate(oldDumpster);

            GameObject dumpsterGo = new GameObject("Recycling_Dumpster");
            dumpsterGo.transform.position = new Vector3(-3.5f, 0f, -6.2f); // Outside near entrance
            dumpsterGo.transform.rotation = Quaternion.Euler(0f, 15f, 0f);

            // Dumpster Mesh
            GameObject bin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bin.name = "DumpsterMesh";
            bin.transform.SetParent(dumpsterGo.transform, false);
            bin.transform.localScale = new Vector3(1.5f, 1.1f, 1.0f);
            bin.transform.localPosition = new Vector3(0f, 0.55f, 0f);

            // Material: Dark recycling green
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(s);
            mat.color = new Color(0.08f, 0.38f, 0.18f); // Recycling forest green
            bin.GetComponent<Renderer>().sharedMaterial = mat;

            // Label
            GameObject lblGo = new GameObject("DumpsterLabel");
            lblGo.transform.SetParent(dumpsterGo.transform, false);
            lblGo.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            TextMesh tm = lblGo.AddComponent<TextMesh>();
            tm.text = "♻ GERİ DÖNÜŞÜM";
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.05f;
            tm.fontSize = 28;
            tm.color = new Color(0.2f, 0.95f, 0.4f, 1f);

            // Collider
            BoxCollider col = dumpsterGo.AddComponent<BoxCollider>();
            col.size = new Vector3(1.7f, 1.3f, 1.2f);
            col.center = new Vector3(0f, 0.65f, 0f);

            dumpsterGo.AddComponent<TrashDumpsterController>();

            EditorUtility.SetDirty(dumpsterGo);
            Debug.Log("[Stage13] Created Recycling_Dumpster at (-3.5, 0, -6.2).");
        }

        private static void SetupDeliveryDropZone()
        {
            var oldZone = GameObject.Find("Delivery_DropZone");
            if (oldZone != null) Object.DestroyImmediate(oldZone);

            GameObject dropZoneGo = new GameObject("Delivery_DropZone");
            dropZoneGo.transform.position = new Vector3(2.5f, 0f, -5.8f);

            // Visual pallet / loading bay marker
            GameObject bayMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bayMesh.name = "PalletMarker";
            bayMesh.transform.SetParent(dropZoneGo.transform, false);
            bayMesh.transform.localScale = new Vector3(2.2f, 0.06f, 1.8f);
            bayMesh.transform.localPosition = new Vector3(0f, 0.03f, 0f);

            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material bayMat = new Material(s);
            bayMat.color = new Color(0.85f, 0.68f, 0.15f); // Loading yellow
            bayMesh.GetComponent<Renderer>().sharedMaterial = bayMat;
            var c = bayMesh.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            var deliveryService = dropZoneGo.AddComponent<DeliveryService>();

            // Spawn starter boxes so the player has immediate inventory to stock
            deliveryService.DeliverBox("prod_water", 10, true);
            deliveryService.DeliverBox("prod_milk", 8, true);
            deliveryService.DeliverBox("prod_chips", 10, true);

            EditorUtility.SetDirty(dropZoneGo);
            Debug.Log("[Stage13] Created Delivery_DropZone with 3 starter wholesale boxes.");
        }

        private static void SetupShelfPriceTags()
        {
            var shelves = Object.FindObjectsByType<ShelfVisualController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;
            foreach (var s in shelves)
            {
                if (s == null) continue;
                var existing = s.GetComponentInChildren<ShelfPriceTag>();
                if (existing == null)
                {
                    GameObject tagGo = new GameObject("Shelf_PriceTag");
                    tagGo.transform.SetParent(s.transform, false);
                    tagGo.transform.localPosition = new Vector3(0.35f, 0.95f, 0.42f);
                    tagGo.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                    // Add collider on price tag for easy raycast/player detection
                    var col = tagGo.AddComponent<BoxCollider>();
                    col.size = new Vector3(0.4f, 0.2f, 0.1f);
                    col.isTrigger = true;

                    var tag = tagGo.AddComponent<ShelfPriceTag>();
                    tag.UpdateTagVisuals();
                    count++;
                }
            }
            Debug.Log($"[Stage13] Added/Configured {count} ShelfPriceTags on supermarket shelves.");
        }
    }
}
#endif
