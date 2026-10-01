#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Gameplay;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Economy;

namespace MiniMarketTycoon.Editor
{
    /// <summary>
    /// Master automation for Stage 11: Kenney Low-Poly Asset, Humanoid Rig, Animator & Isometric Camera Polish.
    /// 1. Configures Kenney character and animations (idle, run) with Unity Humanoid Rig & Loop settings.
    /// 2. Creates and links AC_Customer Animator Controller (Idle <-> Walk transitions based on IsWalking & Speed).
    /// 3. Generates 12 diverse NPC skins/face materials (men, women, business, skater, student, grandpa, etc.).
    /// 4. Builds clean Kenney Customer Prefab with integrated Animator, Humanoid Avatar, and NavMeshAgent.
    /// 5. Configures Main Camera in sweet isometric mobile tycoon angle: Position (0, 8, -6), Rotation (45, 0, 0).
    /// 6. Places 2 fully stocked product shelves on the market floor directly in camera view with neat product prefabs.
    /// </summary>
    [InitializeOnLoad]
    public static class Stage11VisualPolishSetup
    {
        static Stage11VisualPolishSetup()
        {
            EditorApplication.delayCall += CheckAndExecuteOverhaul;
        }

        private static void CheckAndExecuteOverhaul()
        {
            string marker = "Assets/_Project/Art/.stage11_anim_polish_v2";
            if (!File.Exists(marker))
            {
                ExecuteFullOverhaul();
                File.WriteAllText(marker, System.DateTime.UtcNow.ToString("o"));
            }
        }

        [MenuItem("MiniMarket/Execute Full Visual Overhaul (Kenney 3D & Camera)")]
        public static void ExecuteFullOverhaul()
        {
            Debug.Log("[Stage11VisualPolish] Starting comprehensive 3D Visual, Animation & Camera Overhaul...");

            AssetDatabase.Refresh();

            // 1. Setup Humanoid Rig on Character and Animations
            SetupHumanoidRig();

            // 2. Build Animator Controller (Idle <-> Walk)
            RuntimeAnimatorController animatorController = EnsureCustomerAnimatorController();

            // 3. Ensure Environment and Character Face/Skin Materials (12 Variations)
            Material kenneyMarketMat = EnsureKenneyMarketMaterial();
            Material charMaleMat = EnsureAllFaceMaterials();

            // 4. Build Human Customer Prefab with Humanoid Animator & 1.75m Scale
            BuildKenneyCustomerPrefab(charMaleMat, animatorController);

            // 5. Build Environment Prefabs (Kenney Mini Market Models)
            BuildShelfPrefab(kenneyMarketMat);
            BuildCheckoutPrefab(kenneyMarketMat);
            BuildRefrigeratorPrefab(kenneyMarketMat);
            BuildShoppingBasketsPrefab(kenneyMarketMat);
            BuildStorefrontSignPrefab();

            // 6. Update Game Scene: Camera Angle (0, 8, -6) (45, 0, 0) + 2 Stocked Product Shelves
            UpdateGameScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Stage11VisualPolish] Visual & Animation Overhaul completed successfully! Humanoid Animator wired, 12 NPC faces active, camera in isometric tycoon view, and 2 stocked shelves placed.");
        }

        public static void SetupHumanoidRig()
        {
            string charPath = "Assets/_Project/Art/Characters/characterMedium.fbx";
            ModelImporter charImporter = AssetImporter.GetAtPath(charPath) as ModelImporter;
            if (charImporter != null)
            {
                bool dirty = false;
                if (charImporter.animationType != ModelImporterAnimationType.Human || charImporter.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                {
                    charImporter.animationType = ModelImporterAnimationType.Human;
                    charImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    dirty = true;
                }
                if (dirty)
                {
                    charImporter.SaveAndReimport();
                    Debug.Log("[Stage11VisualPolish] Reimported characterMedium.fbx as Humanoid Avatar.");
                }
            }

            // Find character avatar
            Avatar charAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(charPath);
            if (charAvatar == null)
            {
                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(charPath))
                {
                    if (obj is Avatar av) { charAvatar = av; break; }
                }
            }

            // Reconfigure idle.fbx with take Root|Idle (33 frames, loop enabled)
            string idlePath = "Assets/_Project/Art/Characters/Animations/idle.fbx";
            if (File.Exists(idlePath))
            {
                ModelImporter animImporter = AssetImporter.GetAtPath(idlePath) as ModelImporter;
                if (animImporter != null)
                {
                    animImporter.animationType = ModelImporterAnimationType.Human;
                    if (charAvatar != null)
                    {
                        animImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                        animImporter.sourceAvatar = charAvatar;
                    }
                    else
                    {
                        animImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    }

                    var clip = new ModelImporterClipAnimation
                    {
                        name = "Idle",
                        takeName = "Root|Idle",
                        firstFrame = 0,
                        lastFrame = 32,
                        loopTime = true,
                        loopPose = true,
                        lockRootRotation = true,
                        lockRootHeightY = true,
                        lockRootPositionXZ = true
                    };
                    animImporter.clipAnimations = new[] { clip };
                    animImporter.SaveAndReimport();
                    Debug.Log("[Stage11VisualPolish] Reimported idle.fbx with take Root|Idle (33 frames, Loop enabled).");
                }
            }

            // Reconfigure run.fbx with take Root|Run (17 frames, loop enabled)
            string runPath = "Assets/_Project/Art/Characters/Animations/run.fbx";
            if (File.Exists(runPath))
            {
                ModelImporter animImporter = AssetImporter.GetAtPath(runPath) as ModelImporter;
                if (animImporter != null)
                {
                    animImporter.animationType = ModelImporterAnimationType.Human;
                    if (charAvatar != null)
                    {
                        animImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                        animImporter.sourceAvatar = charAvatar;
                    }
                    else
                    {
                        animImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    }

                    var clip = new ModelImporterClipAnimation
                    {
                        name = "Run",
                        takeName = "Root|Run",
                        firstFrame = 0,
                        lastFrame = 16,
                        loopTime = true,
                        loopPose = true,
                        lockRootRotation = true,
                        lockRootHeightY = true,
                        lockRootPositionXZ = true
                    };
                    animImporter.clipAnimations = new[] { clip };
                    animImporter.SaveAndReimport();
                    Debug.Log("[Stage11VisualPolish] Reimported run.fbx with take Root|Run (17 frames, Loop enabled).");
                }
            }

            // Reconfigure jump.fbx with take Root|Jump
            string jumpPath = "Assets/_Project/Art/Characters/Animations/jump.fbx";
            if (File.Exists(jumpPath))
            {
                ModelImporter animImporter = AssetImporter.GetAtPath(jumpPath) as ModelImporter;
                if (animImporter != null)
                {
                    animImporter.animationType = ModelImporterAnimationType.Human;
                    if (charAvatar != null)
                    {
                        animImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                        animImporter.sourceAvatar = charAvatar;
                    }
                    else
                    {
                        animImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    }

                    var clip = new ModelImporterClipAnimation
                    {
                        name = "Jump",
                        takeName = "Root|Jump",
                        firstFrame = 0,
                        lastFrame = 12,
                        loopTime = false
                    };
                    animImporter.clipAnimations = new[] { clip };
                    animImporter.SaveAndReimport();
                    Debug.Log("[Stage11VisualPolish] Reimported jump.fbx with take Root|Jump.");
                }
            }
        }

        public static RuntimeAnimatorController EnsureCustomerAnimatorController()
        {
            string dir = "Assets/_Project/Art/Characters";
            string controllerPath = $"{dir}/AC_Customer.controller";

            AnimationClip idleClip = FindClipInAsset("Assets/_Project/Art/Characters/Animations/idle.fbx", "idle");
            AnimationClip runClip = FindClipInAsset("Assets/_Project/Art/Characters/Animations/run.fbx", "run");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Ensure all required state & action parameters
            EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "IsWalking", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Walk", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "WalkFast", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsShopping", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsWaiting", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "QueueIdle", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsCheckingOut", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Pickup", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Pay", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Leave", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            AnimatorState idleState = null;
            AnimatorState walkState = null;
            foreach (var childState in rootStateMachine.states)
            {
                if (childState.state.name == "Idle") idleState = childState.state;
                else if (childState.state.name == "Walk") walkState = childState.state;
            }

            if (idleState == null)
            {
                idleState = rootStateMachine.AddState("Idle");
            }
            if (idleClip != null) idleState.motion = idleClip;
            rootStateMachine.defaultState = idleState;

            if (walkState == null)
            {
                walkState = rootStateMachine.AddState("Walk");
            }
            if (runClip != null) walkState.motion = runClip;

            // Idle -> Walk
            bool hasIdleToWalk = false;
            foreach (var t in idleState.transitions)
            {
                if (t.destinationState == walkState) { hasIdleToWalk = true; break; }
            }
            if (!hasIdleToWalk)
            {
                var toWalk = idleState.AddTransition(walkState);
                toWalk.hasExitTime = false;
                toWalk.duration = 0.15f;
                toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
            }

            // Walk -> Idle
            bool hasWalkToIdle = false;
            foreach (var t in walkState.transitions)
            {
                if (t.destinationState == idleState) { hasWalkToIdle = true; break; }
            }
            if (!hasWalkToIdle)
            {
                var toIdle = walkState.AddTransition(idleState);
                toIdle.hasExitTime = false;
                toIdle.duration = 0.15f;
                toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            // Also copy to Resources folder so runtime script loading works seamlessly
            string resDir = "Assets/_Project/Resources";
            if (!Directory.Exists(resDir)) Directory.CreateDirectory(resDir);
            string resPath = $"{resDir}/AC_Customer.controller";
            AssetDatabase.CopyAsset(controllerPath, resPath);

            Debug.Log($"[Stage11VisualPolish] Created and configured AC_Customer Animator Controller with Idle: {idleClip?.name ?? "null"}, Run: {runClip?.name ?? "null"}");
            return controller;
        }

        private static AnimationClip FindClipInAsset(string path, string preferredName = null)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            AnimationClip fallback = null;
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__preview__") && !clip.name.Contains("Targeting Pose"))
                {
                    if (!string.IsNullOrEmpty(preferredName) && clip.name.IndexOf(preferredName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return clip;
                    }
                    if (fallback == null) fallback = clip;
                }
            }
            return fallback;
        }

        private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (var p in controller.parameters)
            {
                if (p.name == name) return;
            }
            controller.AddParameter(name, type);
        }

        public static Material EnsureAllFaceMaterials()
        {
            string resDir = "Assets/_Project/Resources";
            if (!Directory.Exists(resDir)) Directory.CreateDirectory(resDir);

            // 12 NPC Variations: Diverse faces & styles
            var variations = new (string matName, string texGuid)[]
            {
                ("Mat_Char_HumanMale", "51c5fea8882d0c06f6e59947c486ebe1"),      // humanMaleA.png
                ("Mat_Char_HumanFemale", "75c22d1e5287b9e1b234902d8a4845ac"),    // humanFemaleA.png
                ("Mat_Char_SkaterMale", "d3c67b9558fd334951efeea898c69411"),     // skaterMaleA.png
                ("Mat_Char_SkaterFemale", "256a67d65ef3263f9a7d7c513b660a4f"),   // skaterFemaleA.png
                ("Mat_Char_CriminalMale", "e6fc19d17f8ac5143f733a4531f09e7b"),   // criminalMaleA.png
                ("Mat_Char_CyborgFemale", "1b052b9e163ba81243a62013460e3ba7"),   // cyborgFemaleA.png
                ("Mat_Char_BusinessMale", "92349f03b7eb33061ffa793dc7b27582"),   // businessMale.png
                ("Mat_Char_BusinessFemale", "2739eac21aaf6b67bd66a4007cd7b612"), // businessFemale.png
                ("Mat_Char_Grandpa", "a77dd83179e78b056db6a095bb6d44ba"),        // grandpaMale.png
                ("Mat_Char_Student", "d7dddb4dab3a3bd54b77d8a9dc9127f0"),        // studentCasual.png
                ("Mat_Char_SurvivorFemale", "3f4fbb2a605c5fa76451a42260c33fc0"), // survivorFemaleA.png
                ("Mat_Char_SurvivorMale", "456bbdba3eb652d9d3b0fb15c0a24f08")    // survivorMaleB.png
            };

            Material defaultMat = null;
            foreach (var item in variations)
            {
                Material m = EnsureCharacterMaterial(item.matName, item.texGuid);
                if (defaultMat == null) defaultMat = m;

                // Sync with Resources folder for runtime dynamic skin swapping
                string resMatPath = $"{resDir}/{item.matName}.mat";
                string artMatPath = $"Assets/_Project/Art/Materials/{item.matName}.mat";
                if (!File.Exists(resMatPath) && File.Exists(artMatPath))
                {
                    AssetDatabase.CopyAsset(artMatPath, resMatPath);
                }
            }

            return defaultMat;
        }

        private static Material EnsureKenneyMarketMaterial()
        {
            string path = "Assets/_Project/Art/Materials/Mat_Kenney_Market.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>("Assets/_Project/Art/Environment/Textures/colormap.png");
            if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture>("Assets/_Project/Art/Environment/Textures/variation-a.png");

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(urpLit);
                mat.name = "Mat_Kenney_Market";
                if (tex != null)
                {
                    mat.mainTexture = tex;
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                }
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
                mat.enableInstancing = true;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                if (tex != null && mat.mainTexture == null)
                {
                    mat.mainTexture = tex;
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    EditorUtility.SetDirty(mat);
                }
            }
            return mat;
        }

        private static Material EnsureCharacterMaterial(string matName, string texGuid)
        {
            string path = $"Assets/_Project/Art/Materials/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            string texPath = AssetDatabase.GUIDToAssetPath(texGuid);
            Texture tex = !string.IsNullOrEmpty(texPath) ? AssetDatabase.LoadAssetAtPath<Texture>(texPath) : null;

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(urpLit);
                mat.name = matName;
                if (tex != null)
                {
                    mat.mainTexture = tex;
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                }
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
                mat.enableInstancing = true;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                if (tex != null && mat.mainTexture == null)
                {
                    mat.mainTexture = tex;
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    EditorUtility.SetDirty(mat);
                }
            }
            return mat;
        }

        public static void BuildKenneyCustomerPrefab(Material defaultMat, RuntimeAnimatorController controller)
        {
            string prefabPath = "Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab";
            GameObject charModelAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/characterMedium.fbx");
            if (charModelAsset == null)
            {
                Debug.LogWarning("[Stage11VisualPolish] characterMedium.fbx not found at Assets/_Project/Art/Characters/characterMedium.fbx");
                return;
            }

            GameObject root = new GameObject("PF_Customer_NPC");

            // 1. Navigation & Physics for Natural Human Size (1.75m tall, 0.3m radius)
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.30f;
            agent.height = 1.75f;
            agent.speed = 1.25f;
            agent.angularSpeed = 480f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.20f;

            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.88f, 0f);
            col.radius = 0.30f;
            col.height = 1.75f;

            // 2. Logic Components
            var nav = root.AddComponent<CustomerNavigation>();
            var anim = root.AddComponent<CustomerAnimationController>();
            var lookAt = root.AddComponent<CustomerLookAtController>();
            var visual = root.AddComponent<CustomerVisual>();
            var custController = root.AddComponent<CustomerController>();

            // 3. Instantiate Kenney Model as child
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(charModelAsset);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            // Elevated by 0.82m so feet touch floor perfectly
            model.transform.localPosition = new Vector3(0f, 0.82f, 0f);
            model.transform.localRotation = Quaternion.identity;
            // 3.765m FBX * 0.465f = exactly 1.75m natural human height!
            model.transform.localScale = Vector3.one * 0.465f;

            // 4. Setup Animator on Model
            Animator animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();

            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/_Project/Art/Characters/characterMedium.fbx");
            if (avatar == null)
            {
                foreach (var a in AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Characters/characterMedium.fbx"))
                {
                    if (a is Avatar av) { avatar = av; break; }
                }
            }
            if (avatar != null) animator.avatar = avatar;
            if (controller != null) animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            // Wire Animator to CustomerAnimationController
            SerializedObject soAnim = new SerializedObject(anim);
            soAnim.FindProperty("_animator").objectReferenceValue = animator;
            soAnim.FindProperty("_navigation").objectReferenceValue = nav;
            soAnim.ApplyModifiedProperties();

            // Apply default skin material
            Renderer rend = model.GetComponentInChildren<SkinnedMeshRenderer>() as Renderer ?? model.GetComponentInChildren<MeshRenderer>();
            if (rend != null && defaultMat != null)
            {
                rend.sharedMaterial = defaultMat;
            }

            // Clean any colliders on model parts
            Collider[] modelCols = model.GetComponentsInChildren<Collider>(true);
            foreach (var c in modelCols) Object.DestroyImmediate(c);

            // Connect CustomerVisual and bones
            visual.DiscoverBones();

            // Save Prefab
            string dir = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Stage11VisualPolish] Saved clean Kenney character prefab with Animator at {prefabPath} (Human height: 1.75m)");
        }

        public static void BuildShelfPrefab(Material mat)
        {
            string prefabPath = "Assets/_Project/Prefabs/Environment/PF_Shelf.prefab";
            GameObject shelfAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Models/shelf-boxes.fbx");
            if (shelfAsset == null) shelfAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Models/shelf-bags.fbx");

            GameObject root = new GameObject("PF_Shelf");
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.8f, 0f);
            col.size = new Vector3(1.6f, 1.6f, 1.4f);

            if (shelfAsset != null)
            {
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(shelfAsset);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);

                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sharedMaterial = mat;
                }
                Collider[] subCols = model.GetComponentsInChildren<Collider>(true);
                foreach (var c in subCols) Object.DestroyImmediate(c);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Stage11VisualPolish] Saved Kenney shelf prefab at {prefabPath}");
        }

        public static void BuildCheckoutPrefab(Material mat)
        {
            string prefabPath = "Assets/_Project/Prefabs/Environment/PF_Checkout.prefab";
            GameObject checkoutAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Models/cash-register.fbx");

            GameObject root = new GameObject("PF_Checkout");
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.54f, 0f);
            col.size = new Vector3(1.53f, 1.08f, 1.53f);

            if (checkoutAsset != null)
            {
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(checkoutAsset);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);

                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sharedMaterial = mat;
                }
                Collider[] subCols = model.GetComponentsInChildren<Collider>(true);
                foreach (var c in subCols) Object.DestroyImmediate(c);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Stage11VisualPolish] Saved Kenney checkout prefab at {prefabPath}");
        }

        public static void BuildRefrigeratorPrefab(Material mat)
        {
            string prefabPath = "Assets/_Project/Prefabs/Environment/PF_Refrigerator.prefab";
            GameObject freezerAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Models/freezers-standing.fbx");
            if (freezerAsset == null) freezerAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Models/freezer.fbx");

            GameObject root = new GameObject("PF_Refrigerator");
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.9f, 0f);
            col.size = new Vector3(2.0f, 1.8f, 1.0f);

            if (freezerAsset != null)
            {
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(freezerAsset);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);

                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sharedMaterial = mat;
                }
                Collider[] subCols = model.GetComponentsInChildren<Collider>(true);
                foreach (var c in subCols) Object.DestroyImmediate(c);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Stage11VisualPolish] Saved Kenney refrigerator prefab at {prefabPath}");
        }

        public static void BuildShoppingBasketsPrefab(Material mat)
        {
            string prefabPath = "Assets/_Project/Prefabs/Environment/PF_ShoppingBaskets.prefab";
            GameObject basketAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Models/shopping-basket.fbx");

            GameObject root = new GameObject("PF_ShoppingBaskets");
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.3f, 0f);
            col.size = new Vector3(0.6f, 0.6f, 0.6f);

            if (basketAsset != null)
            {
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(basketAsset);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);

                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sharedMaterial = mat;
                }
                Collider[] subCols = model.GetComponentsInChildren<Collider>(true);
                foreach (var c in subCols) Object.DestroyImmediate(c);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Stage11VisualPolish] Saved Kenney shopping baskets prefab at {prefabPath}");
        }

        public static void BuildStorefrontSignPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Environment/PF_StorefrontSign.prefab";
            Material signMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Signboard.mat");

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "PF_StorefrontSign";
            // Correct rotation: Y: 180 so texture faces outward without being reversed
            root.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            root.transform.localScale = new Vector3(5.5f, 1.35f, 0.15f);

            Collider c = root.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            Renderer r = root.GetComponent<Renderer>();
            if (r != null && signMat != null)
            {
                r.sharedMaterial = signMat;
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Stage11VisualPolish] Saved corrected storefront sign prefab (Y: 180) at {prefabPath}");
        }

        public static void UpdateGameScene()
        {
            string scenePath = "Assets/_Project/Scenes/03_Game.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogWarning($"[Stage11VisualPolish] Could not open scene {scenePath}");
                return;
            }

            // 1. Camera Angle & Position: Sweet Isometric Mobile Tycoon View
            // Position: (0, 8, -6), Rotation: (45, 0, 0)
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 8f, -6f);
                mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

                var camCtrl = mainCam.GetComponent<CameraController>();
                if (camCtrl != null)
                {
                    SerializedObject so = new SerializedObject(camCtrl);
                    so.FindProperty("_pitchAngle").floatValue = 45f;
                    so.FindProperty("_yawAngle").floatValue = 0f;
                    so.FindProperty("_currentZoom").floatValue = 11.31f;
                    so.ApplyModifiedProperties();
                }
            }

            // 2. Rebuild Market Environment with Kenney Models & Corrected Sign
            MarketEnvironmentBuilder builder = Object.FindAnyObjectByType<MarketEnvironmentBuilder>();
            if (builder != null)
            {
                builder.AutoAssignMaterials();
                builder.BuildEnvironment();
                EditorUtility.SetDirty(builder.gameObject);
            }

            // 3. Place 2 Stocked Product Shelves on Market Floor in Direct Camera View
            SetupFrontStockedShelves();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Stage11VisualPolish] Successfully updated and saved 03_Game.unity scene with isometric camera and stocked shelves!");
        }

        private static void SetupFrontStockedShelves()
        {
            GameObject shelvesGroup = GameObject.Find("Front_Display_Shelves");
            if (shelvesGroup != null)
            {
                Object.DestroyImmediate(shelvesGroup);
            }

            shelvesGroup = new GameObject("Front_Display_Shelves");

            // Left Shelf: Soda & Milk at (-2.0, 0, 1.2)
            CreateStockedFloorShelf("Floor_Shelf_Beverages", new Vector3(-2.0f, 0f, 1.2f), "COLD BEVERAGES & DAIRY",
                shelvesGroup.transform,
                "Assets/_Project/Prefabs/Products/PF_Product_Soda.prefab",
                "Assets/_Project/Prefabs/Products/PF_Product_Milk.prefab",
                "prod_soda");

            // Right Shelf: Chips & Cookies at (2.0, 0, 1.2)
            CreateStockedFloorShelf("Floor_Shelf_Snacks", new Vector3(2.0f, 0f, 1.2f), "DELICIOUS SNACKS & BAKERY",
                shelvesGroup.transform,
                "Assets/_Project/Prefabs/Products/PF_Product_Chips.prefab",
                "Assets/_Project/Prefabs/Products/PF_Product_Cookies.prefab",
                "prod_chips");
        }

        private static void CreateStockedFloorShelf(string name, Vector3 pos, string label, Transform parent, string lowerProductPrefabPath, string upperProductPrefabPath, string productId)
        {
            GameObject shelfAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Models/shelf-boxes.fbx");
            Material marketMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Kenney_Market.mat");

            GameObject root = new GameObject(name);
            root.transform.parent = parent;
            root.transform.position = pos;

            if (shelfAsset != null)
            {
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(shelfAsset);
                model.name = "Shelf_Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);

                if (marketMat != null)
                {
                    Renderer[] rends = model.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in rends) r.sharedMaterial = marketMat;
                }
                Collider[] subCols = model.GetComponentsInChildren<Collider>(true);
                foreach (var c in subCols) Object.DestroyImmediate(c);
            }

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.8f, 0f);
            col.size = new Vector3(1.6f, 1.6f, 1.4f);

            NavMeshObstacle obs = root.AddComponent<NavMeshObstacle>();
            obs.center = new Vector3(0f, 0.8f, 0f);
            obs.size = new Vector3(1.6f, 1.6f, 1.4f);
            obs.carving = true;

            // Stock Tier 1 (Lower Shelf at Y: 0.55m)
            GameObject lowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lowerProductPrefabPath);
            List<GameObject> visualItems = new List<GameObject>();
            if (lowerPrefab != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    float xOff = (i - 1.5f) * 0.28f;
                    GameObject itemFront = (GameObject)PrefabUtility.InstantiatePrefab(lowerPrefab);
                    itemFront.name = $"Lower_Item_F_{i}";
                    itemFront.transform.SetParent(root.transform, false);
                    itemFront.transform.localPosition = new Vector3(xOff, 0.55f, 0.22f);
                    visualItems.Add(itemFront);

                    GameObject itemBack = (GameObject)PrefabUtility.InstantiatePrefab(lowerPrefab);
                    itemBack.name = $"Lower_Item_B_{i}";
                    itemBack.transform.SetParent(root.transform, false);
                    itemBack.transform.localPosition = new Vector3(xOff, 0.55f, -0.22f);
                    visualItems.Add(itemBack);
                }
            }

            // Stock Tier 2 (Upper Shelf at Y: 1.05m)
            GameObject upperPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(upperProductPrefabPath);
            if (upperPrefab != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    float xOff = (i - 1.5f) * 0.28f;
                    GameObject itemFront = (GameObject)PrefabUtility.InstantiatePrefab(upperPrefab);
                    itemFront.name = $"Upper_Item_F_{i}";
                    itemFront.transform.SetParent(root.transform, false);
                    itemFront.transform.localPosition = new Vector3(xOff, 1.05f, 0.22f);
                    visualItems.Add(itemFront);

                    GameObject itemBack = (GameObject)PrefabUtility.InstantiatePrefab(upperPrefab);
                    itemBack.name = $"Upper_Item_B_{i}";
                    itemBack.transform.SetParent(root.transform, false);
                    itemBack.transform.localPosition = new Vector3(xOff, 1.05f, -0.22f);
                    visualItems.Add(itemBack);
                }
            }

            // Interaction Points (Front & Back)
            GameObject ptFront = new GameObject("Interaction_Point_Front");
            ptFront.transform.parent = root.transform;
            ptFront.transform.localPosition = new Vector3(0f, 0f, 0.85f);
            var ipFront = ptFront.AddComponent<CustomerInteractionPoint>();
            ipFront.Initialize(label, -Vector3.forward);

            GameObject ptBack = new GameObject("Interaction_Point_Back");
            ptBack.transform.parent = root.transform;
            ptBack.transform.localPosition = new Vector3(0f, 0f, -0.85f);
            var ipBack = ptBack.AddComponent<CustomerInteractionPoint>();
            ipBack.Initialize(label, Vector3.forward);

            var shelfVis = root.AddComponent<ShelfVisualController>();
            ProductData pData = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;
            shelfVis.Initialize(pData, ShelfType.StandardShelf, visualItems, new List<CustomerInteractionPoint> { ipFront, ipBack });
        }
    }
}
#endif
