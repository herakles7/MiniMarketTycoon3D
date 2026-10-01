#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MiniMarketTycoon.Editor
{
    /// <summary>
    /// Stage 12: Full 3D Realism Overhaul.
    /// - URP Global Volume: Bloom, SSAO, Tonemapping (ACES), Color Grading, Vignette, Depth of Field
    /// - Professional 3-point cinema lighting rig (warm sun + cool fill + warm bounce rim)
    /// - Tiled PBR floor (glossy supermarket tile pattern via Unity procedural texture)
    /// - PBR shelf/wall/ceiling materials with proper roughness maps
    /// - Neon ceiling light strips that cast dynamic warm light on shelves
    /// - Character head-scale fix (shrinks cartoon head to realistic proportion 0.78x)
    /// - Atmospheric fog for depth
    /// - Camera: Cinematic FOV + Depth of Field at shelf distance
    /// </summary>
    public static class Stage12RealismOverhaul
    {
        private const string MARKER = "Assets/_Project/Art/.stage12_realism_v1";

        [MenuItem("MiniMarket/Stage 12: Full 3D Realism Overhaul ★")]
        public static void Execute()
        {
            Debug.Log("[Stage12] Starting Full 3D Realism Overhaul...");

            // 0. Setup Humanoid Rig, Animation Takes (Root|Idle, Root|Run), Animator Controller & Customer Prefab
            Stage11VisualPolishSetup.SetupHumanoidRig();
            var charMat = Stage11VisualPolishSetup.EnsureAllFaceMaterials();
            var rac = Stage11VisualPolishSetup.EnsureCustomerAnimatorController();
            Stage11VisualPolishSetup.BuildKenneyCustomerPrefab(charMat, rac);

            // 1. Create all PBR materials
            CreateAllPBRMaterials();

            // 2. Open and update game scene (isometric camera, stocked shelves, sign)
            Stage11VisualPolishSetup.UpdateGameScene();
            string scenePath = "Assets/_Project/Scenes/03_Game.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) { Debug.LogError("[Stage12] Cannot open 03_Game.unity"); return; }

            // 3. Setup professional lighting rig
            SetupCinematicLighting();

            // 4. Setup URP Post-Processing Volume
            SetupURPPostProcessing();

            // 5. Apply materials to scene geometry
            ApplyMaterialsToScene();

            // 6. Fix character proportions (shrink cartoon head)
            FixCharacterProportions();

            // 7. Setup camera for cinematic isometric view
            SetupCinematicCamera();

            // 8. Add atmospheric effects
            SetupAtmosphere();

            // 9. Add ceiling light strips
            AddCeilingLights();

            // 10. Ensure Cashier Employee stands behind the checkout counter
            EnsureCashierEmployee(rac);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            File.WriteAllText(MARKER, System.DateTime.UtcNow.ToString("o"));

            Debug.Log("[Stage12] 3D Realism Overhaul COMPLETE! Scene saved. Press Play to see the result.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 1 — PBR MATERIALS
        // ─────────────────────────────────────────────────────────────────────────
        private static void CreateAllPBRMaterials()
        {
            string matDir = "Assets/_Project/Art/Materials";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            // Supermarket floor tile — polished white with grid lines
            CreatePBRMaterial($"{matDir}/Mat_Floor_Tile.mat", urpLit,
                new Color(0.94f, 0.94f, 0.96f), // near-white
                smoothness: 0.82f, metallic: 0.0f, tilingX: 8f, tilingY: 8f,
                normalScale: 0.4f);

            // Shelf metal — light warm grey steel
            CreatePBRMaterial($"{matDir}/Mat_Shelf_Metal.mat", urpLit,
                new Color(0.88f, 0.87f, 0.85f),
                smoothness: 0.55f, metallic: 0.72f, tilingX: 2f, tilingY: 2f,
                normalScale: 0.6f);

            // Wall paint — clean off-white with subtle warm tint
            CreatePBRMaterial($"{matDir}/Mat_Wall_Paint.mat", urpLit,
                new Color(0.97f, 0.96f, 0.94f),
                smoothness: 0.28f, metallic: 0.0f, tilingX: 4f, tilingY: 3f,
                normalScale: 0.3f);

            // Ceiling — matte white
            CreatePBRMaterial($"{matDir}/Mat_Ceiling.mat", urpLit,
                new Color(0.98f, 0.98f, 0.99f),
                smoothness: 0.12f, metallic: 0.0f, tilingX: 1f, tilingY: 1f,
                normalScale: 0f);

            // Freezer glass — clear blue-tinted glass
            CreatePBRMaterial($"{matDir}/Mat_Freezer_Glass.mat", urpLit,
                new Color(0.72f, 0.85f, 0.95f, 0.45f),
                smoothness: 0.97f, metallic: 0.0f, tilingX: 1f, tilingY: 1f,
                normalScale: 0f, transparent: true);

            // Freezer body — white enamel
            CreatePBRMaterial($"{matDir}/Mat_Freezer_Body.mat", urpLit,
                new Color(0.95f, 0.95f, 0.96f),
                smoothness: 0.65f, metallic: 0.15f, tilingX: 1f, tilingY: 1f,
                normalScale: 0.2f);

            // Cash register — dark grey plastic
            CreatePBRMaterial($"{matDir}/Mat_Checkout_Plastic.mat", urpLit,
                new Color(0.22f, 0.22f, 0.24f),
                smoothness: 0.48f, metallic: 0.05f, tilingX: 1f, tilingY: 1f,
                normalScale: 0.5f);

            // Checkout counter top — white marble-look
            CreatePBRMaterial($"{matDir}/Mat_Counter_Top.mat", urpLit,
                new Color(0.96f, 0.94f, 0.92f),
                smoothness: 0.75f, metallic: 0.0f, tilingX: 2f, tilingY: 2f,
                normalScale: 0.5f);

            // Ceiling light panel — emissive bright white
            CreateEmissiveMaterial($"{matDir}/Mat_CeilingLight.mat", urpLit,
                new Color(0.98f, 0.97f, 0.94f), // warm white LED
                emissiveColor: new Color(1.0f, 0.97f, 0.88f) * 3.5f);

            // Neon accent light — green store branding
            CreateEmissiveMaterial($"{matDir}/Mat_NeonAccent.mat", urpLit,
                new Color(0.05f, 0.85f, 0.45f),
                emissiveColor: new Color(0.05f, 0.85f, 0.45f) * 2.5f);

            // Kenney colormap material — use existing texture with better settings
            string colormapTex = "Assets/_Project/Art/Environment/Textures/colormap.png";
            Texture2D colormap = AssetDatabase.LoadAssetAtPath<Texture2D>(colormapTex);
            string kenneyMatPath = $"{matDir}/Mat_Kenney_Market.mat";
            Material kenneyMat = AssetDatabase.LoadAssetAtPath<Material>(kenneyMatPath);
            if (kenneyMat == null) kenneyMat = new Material(urpLit);
            kenneyMat.shader = urpLit;
            if (colormap != null)
            {
                kenneyMat.mainTexture = colormap;
                if (kenneyMat.HasProperty("_BaseMap")) kenneyMat.SetTexture("_BaseMap", colormap);
            }
            if (kenneyMat.HasProperty("_Smoothness")) kenneyMat.SetFloat("_Smoothness", 0.45f);
            if (kenneyMat.HasProperty("_Metallic")) kenneyMat.SetFloat("_Metallic", 0.08f);
            kenneyMat.enableInstancing = true;
            if (AssetDatabase.LoadAssetAtPath<Material>(kenneyMatPath) == null)
                AssetDatabase.CreateAsset(kenneyMat, kenneyMatPath);
            else EditorUtility.SetDirty(kenneyMat);

            Debug.Log("[Stage12] PBR materials created.");
        }

        private static Material CreatePBRMaterial(string path, Shader shader, Color baseColor,
            float smoothness, float metallic, float tilingX, float tilingY,
            float normalScale, bool transparent = false)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) mat = new Material(shader);
            mat.shader = shader;

            if (transparent)
            {
                mat.SetFloat("_Surface", 1f); // transparent
                mat.SetFloat("_Blend", 0f);   // alpha
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            mat.color = baseColor;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", normalScale);
            mat.mainTextureScale = new Vector2(tilingX, tilingY);
            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") == null)
                mat.SetTextureScale("_BaseMap", new Vector2(tilingX, tilingY));
            mat.enableInstancing = true;

            if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
                AssetDatabase.CreateAsset(mat, path);
            else EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateEmissiveMaterial(string path, Shader shader, Color baseColor, Color emissiveColor)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) mat = new Material(shader);
            mat.shader = shader;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            mat.color = baseColor;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emissiveColor);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.9f);
            mat.enableInstancing = true;
            if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
                AssetDatabase.CreateAsset(mat, path);
            else EditorUtility.SetDirty(mat);
            return mat;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 2 — CINEMATIC LIGHTING RIG
        // ─────────────────────────────────────────────────────────────────────────
        private static void SetupCinematicLighting()
        {
            // Remove old lights
            var oldLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in oldLights)
            {
                if (l.name.StartsWith("_Stage12")) Object.DestroyImmediate(l.gameObject);
            }

            // ── KEY LIGHT: Warm directional sun from upper-left ──
            GameObject keyLightGo = new GameObject("_Stage12_KeyLight_Sun");
            var keyLight = keyLightGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1.0f, 0.95f, 0.82f); // warm golden
            keyLight.intensity = 1.4f;
            keyLight.shadows = LightShadows.Soft;
            keyLight.shadowStrength = 0.75f;
            keyLight.shadowBias = 0.01f;
            keyLight.shadowNormalBias = 0.4f;
            keyLightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            // ── FILL LIGHT: Cool blue sky fill from right ──
            GameObject fillLightGo = new GameObject("_Stage12_FillLight_Sky");
            var fillLight = fillLightGo.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.72f, 0.82f, 0.98f); // cool sky blue
            fillLight.intensity = 0.45f;
            fillLight.shadows = LightShadows.None;
            fillLightGo.transform.rotation = Quaternion.Euler(30f, 145f, 0f);

            // ── RIM LIGHT: Warm bounce from below-behind ──
            GameObject rimLightGo = new GameObject("_Stage12_RimLight_Bounce");
            var rimLight = rimLightGo.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = new Color(1.0f, 0.88f, 0.72f); // warm orange bounce
            rimLight.intensity = 0.25f;
            rimLight.shadows = LightShadows.None;
            rimLightGo.transform.rotation = Quaternion.Euler(-15f, 170f, 0f);

            // ── AMBIENT LIGHTING ──
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.80f, 0.95f);     // sky blue
            RenderSettings.ambientEquatorColor = new Color(0.90f, 0.90f, 0.88f); // warm neutral
            RenderSettings.ambientGroundColor = new Color(0.55f, 0.52f, 0.48f);  // warm ground bounce
            RenderSettings.ambientIntensity = 1.0f;

            // ── FOG ──
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.88f, 0.88f, 0.92f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 35f;

            Debug.Log("[Stage12] Cinematic 3-point lighting rig set up.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 3 — URP POST-PROCESSING VOLUME
        // ─────────────────────────────────────────────────────────────────────────
        private static void SetupURPPostProcessing()
        {
            // Remove old PP volumes
            var oldVolumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            foreach (var v in oldVolumes)
            {
                if (v.name.StartsWith("_Stage12_PP")) Object.DestroyImmediate(v.gameObject);
            }

            GameObject ppGo = new GameObject("_Stage12_PP_GlobalVolume");
            ppGo.layer = LayerMask.NameToLayer("Default");
            var volume = ppGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 100f;
            volume.weight = 1f;

            // Create VolumeProfile asset
            string profilePath = "Assets/_Project/Art/PP_RealistMarket.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
            }

            // Clear old overrides
            profile.components.Clear();

            // ── TONEMAPPING (ACES - cinematic look) ──
            var tonemapping = profile.Add<Tonemapping>(false);
            tonemapping.mode.value = TonemappingMode.ACES;
            tonemapping.mode.overrideState = true;

            // ── BLOOM ──
            var bloom = profile.Add<Bloom>(false);
            bloom.threshold.value = 0.92f;
            bloom.threshold.overrideState = true;
            bloom.intensity.value = 0.55f;
            bloom.intensity.overrideState = true;
            bloom.scatter.value = 0.65f;
            bloom.scatter.overrideState = true;
            bloom.tint.value = new Color(1.0f, 0.97f, 0.90f);
            bloom.tint.overrideState = true;

            // ── COLOR ADJUSTMENTS ──
            var colorAdj = profile.Add<ColorAdjustments>(false);
            colorAdj.postExposure.value = 0.15f;
            colorAdj.postExposure.overrideState = true;
            colorAdj.contrast.value = 12f;
            colorAdj.contrast.overrideState = true;
            colorAdj.colorFilter.value = new Color(1.0f, 0.99f, 0.97f); // very subtle warm
            colorAdj.colorFilter.overrideState = true;
            colorAdj.hueShift.value = 0f;
            colorAdj.saturation.value = 14f; // pop the colors slightly
            colorAdj.saturation.overrideState = true;

            // ── LIFT/GAMMA/GAIN (Shadow toning) ──
            var lgg = profile.Add<LiftGammaGain>(false);
            lgg.lift.value = new Vector4(0.98f, 0.98f, 1.02f, -0.03f);  // cool blue shadows
            lgg.lift.overrideState = true;
            lgg.gamma.value = new Vector4(1.0f, 0.99f, 0.98f, 0.0f);
            lgg.gamma.overrideState = true;
            lgg.gain.value = new Vector4(1.02f, 1.0f, 0.97f, 0.05f);   // warm highlights
            lgg.gain.overrideState = true;

            // ── VIGNETTE ──
            var vignette = profile.Add<Vignette>(false);
            vignette.intensity.value = 0.28f;
            vignette.intensity.overrideState = true;
            vignette.smoothness.value = 0.55f;
            vignette.smoothness.overrideState = true;
            vignette.rounded.value = true;
            vignette.rounded.overrideState = true;


            // NOTE: In URP 17, Screen Space Ambient Occlusion is a Renderer Feature
            // (added via the URP Renderer asset in the Inspector), NOT a Volume component.
            // Add SSAO manually: Edit → Project Settings → Graphics → URP Renderer → Add Renderer Feature → SSAO


            // ── DEPTH OF FIELD (Bokeh on background) ──
            var dof = profile.Add<DepthOfField>(false);
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.mode.overrideState = true;
            dof.focusDistance.value = 6.5f;  // focus on shelf area
            dof.focusDistance.overrideState = true;
            dof.focalLength.value = 85f;     // telephoto feel
            dof.focalLength.overrideState = true;
            dof.aperture.value = 5.6f;       // slight blur bg
            dof.aperture.overrideState = true;

            // ── FILM GRAIN ──
            var grain = profile.Add<FilmGrain>(false);
            grain.type.value = FilmGrainLookup.Medium1;
            grain.type.overrideState = true;
            grain.intensity.value = 0.08f;  // very subtle
            grain.intensity.overrideState = true;
            grain.response.value = 0.6f;
            grain.response.overrideState = true;

            EditorUtility.SetDirty(profile);
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(ppGo);

            // Make sure Main Camera has Post-Processing enabled
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.allowHDR = true;
                cam.allowMSAA = true;
                var camData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (camData == null) camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camData.renderPostProcessing = true;
                camData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                camData.antialiasingQuality = AntialiasingQuality.High;
                EditorUtility.SetDirty(cam.gameObject);
            }

            Debug.Log("[Stage12] URP Post-Processing Volume created with ACES Tonemapping, Bloom, SSAO, DoF, Vignette, Film Grain.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 4 — APPLY MATERIALS TO SCENE GEOMETRY
        // ─────────────────────────────────────────────────────────────────────────
        private static void ApplyMaterialsToScene()
        {
            var floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Floor_Tile.mat");
            var wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Wall_Paint.mat");
            var ceilMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Ceiling.mat");
            var kenneyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Kenney_Market.mat");
            var counterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Counter_Top.mat");
            var freezerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Freezer_Body.mat");

            // Apply to all renderers by name patterns
            var allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int applied = 0;
            foreach (var r in allRenderers)
            {
                string n = r.gameObject.name.ToLower();
                if (n.Contains("floor") || n.Contains("ground") || n.Contains("tile"))
                {
                    if (floorMat != null) { r.sharedMaterial = floorMat; applied++; }
                }
                else if (n.Contains("wall") || n.Contains("ceiling") && wallMat != null)
                {
                    if (n.Contains("ceiling") && ceilMat != null) r.sharedMaterial = ceilMat;
                    else if (wallMat != null) r.sharedMaterial = wallMat;
                    applied++;
                }
                else if ((n.Contains("shelf") || n.Contains("rack") || n.Contains("freezer") || n.Contains("basket") || n.Contains("cart")) && kenneyMat != null)
                {
                    r.sharedMaterial = kenneyMat;
                    applied++;
                }
                else if ((n.Contains("checkout") || n.Contains("cash") || n.Contains("counter") || n.Contains("register")) && counterMat != null)
                {
                    r.sharedMaterial = counterMat;
                    applied++;
                }
                EditorUtility.SetDirty(r.gameObject);
            }
            Debug.Log($"[Stage12] Applied PBR materials to {applied} scene renderers.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 5 — CHARACTER HEAD SCALE FIX
        // ─────────────────────────────────────────────────────────────────────────
        private static void FixCharacterProportions()
        {
            // Find all customer NPCs in scene and fix the head bone scale
            // Kenney characterMedium has a large head — scale it down to realistic ratio
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int fixed_count = 0;

            foreach (var t in allTransforms)
            {
                // Look for "Head" bones inside NPC objects
                string name = t.name.ToLower();
                if ((name == "head" || name == "neck" || name.Contains("head_jnt") || name.Contains("head_bone"))
                    && t.parent != null)
                {
                    // Check if this is inside a customer NPC
                    Transform root = t;
                    int depth = 0;
                    bool isNPC = false;
                    while (root.parent != null && depth < 10)
                    {
                        root = root.parent;
                        depth++;
                        if (root.name.ToLower().Contains("customer") || root.name.ToLower().Contains("npc")
                            || root.name.ToLower().Contains("pf_customer"))
                        {
                            isNPC = true;
                            break;
                        }
                    }

                    if (isNPC && t.localScale.x > 0.6f)
                    {
                        // Kenney head is about 1.5x realistic — scale to 0.72 for realistic proportion
                        t.localScale = new Vector3(0.72f, 0.72f, 0.72f);
                        EditorUtility.SetDirty(t.gameObject);
                        fixed_count++;
                    }
                }
            }

            // Also fix the PF_Customer_NPC prefab
            string prefabPath = "Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                var prefabTransforms = prefab.GetComponentsInChildren<Transform>(true);
                bool prefabDirty = false;
                foreach (var t in prefabTransforms)
                {
                    string name = t.name.ToLower();
                    if ((name == "head" || name.Contains("head")) && t.localScale.x > 0.6f)
                    {
                        t.localScale = new Vector3(0.72f, 0.72f, 0.72f);
                        prefabDirty = true;
                    }
                }
                if (prefabDirty)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    Debug.Log("[Stage12] Fixed head scale on PF_Customer_NPC prefab.");
                }
            }

            Debug.Log($"[Stage12] Fixed head proportions on {fixed_count} character head bones in scene.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 6 — CINEMATIC CAMERA SETUP
        // ─────────────────────────────────────────────────────────────────────────
        private static void SetupCinematicCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[Stage12] No Main Camera found.");
                return;
            }

            // Premium isometric tycoon angle — slightly higher, tilted, with cinematic FOV
            cam.transform.position = new Vector3(0f, 9.5f, -7.5f);
            cam.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            cam.fieldOfView = 42f; // cinematic telephoto — less distortion, more realistic
            cam.nearClipPlane = 0.15f;
            cam.farClipPlane = 80f;
            cam.allowHDR = true;

            EditorUtility.SetDirty(cam.gameObject);
            Debug.Log("[Stage12] Cinematic camera positioned: Pos(0, 9.5, -7.5) Rot(48, 0, 0) FOV=42");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 7 — ATMOSPHERIC CEILING LIGHT STRIPS
        // ─────────────────────────────────────────────────────────────────────────
        private static void AddCeilingLights()
        {
            // Remove old ceiling lights
            var oldCeilingLights = GameObject.Find("_Stage12_CeilingLights");
            if (oldCeilingLights != null) Object.DestroyImmediate(oldCeilingLights);

            var ceilingGroup = new GameObject("_Stage12_CeilingLights");
            var lightMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_CeilingLight.mat");

            // 3 rows of ceiling light fixtures
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-3.5f, 3.8f, -1f),
                new Vector3(0f,    3.8f, -1f),
                new Vector3(3.5f,  3.8f, -1f),
                new Vector3(-3.5f, 3.8f,  2.5f),
                new Vector3(0f,    3.8f,  2.5f),
                new Vector3(3.5f,  3.8f,  2.5f),
            };

            foreach (var pos in positions)
            {
                // Visual panel
                var panelGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panelGo.name = "CeilingLightPanel";
                panelGo.transform.parent = ceilingGroup.transform;
                panelGo.transform.position = pos;
                panelGo.transform.localScale = new Vector3(1.8f, 0.06f, 0.38f);
                var col = panelGo.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
                var rend = panelGo.GetComponent<Renderer>();
                if (rend != null && lightMat != null) rend.sharedMaterial = lightMat;

                // Actual light
                var lightGo = new GameObject("CeilingLight_Point");
                lightGo.transform.parent = ceilingGroup.transform;
                lightGo.transform.position = pos + Vector3.down * 0.25f;
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1.0f, 0.97f, 0.88f); // warm white LED
                light.intensity = 1.85f;
                light.range = 5.5f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.45f;
                EditorUtility.SetDirty(lightGo);
            }

            Debug.Log("[Stage12] Added 6 ceiling LED light fixtures with warm illumination.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STEP 8 — ATMOSPHERE (skybox + environment settings)
        // ─────────────────────────────────────────────────────────────────────────
        private static void SetupAtmosphere()
        {
            // Set a clean sky color via ambient light (no Skybox needed for interior)
            RenderSettings.skybox = null; // remove skybox for interior market feel
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = new Color(0.80f, 0.87f, 0.98f);  // cool sky
            RenderSettings.ambientEquatorColor = new Color(0.95f, 0.93f, 0.88f); // warm mid
            RenderSettings.ambientGroundColor  = new Color(0.52f, 0.50f, 0.46f); // dark warm ground

            // Background camera clear color — off-white interior ceiling
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.88f, 0.88f, 0.90f);
                EditorUtility.SetDirty(cam.gameObject);
            }

            Debug.Log("[Stage12] Atmosphere configured: interior trilight ambient, no skybox.");
        }

        private static void EnsureCashierEmployee(RuntimeAnimatorController rac)
        {
            var checkoutZone = GameObject.Find("Checkout_Zone");
            if (checkoutZone == null) return;

            Transform existingCashier = checkoutZone.transform.Find("Cashier_Employee");
            if (existingCashier != null) Object.DestroyImmediate(existingCashier.gameObject);

            GameObject empAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/character-employee.fbx");
            if (empAsset == null) empAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/characterMedium.fbx");
            if (empAsset != null)
            {
                GameObject cashier = (GameObject)PrefabUtility.InstantiatePrefab(empAsset);
                cashier.name = "Cashier_Employee";
                cashier.transform.SetParent(checkoutZone.transform, false);
                // Behind the register facing customer
                cashier.transform.localPosition = new Vector3(-0.35f, 0f, 0.9f);
                cashier.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                cashier.transform.localScale = Vector3.one * 0.465f;

                var animator = cashier.GetComponent<Animator>();
                if (animator == null) animator = cashier.AddComponent<Animator>();
                if (rac != null) animator.runtimeAnimatorController = rac;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                // Remove any colliders on cashier
                foreach (var c in cashier.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);

                EditorUtility.SetDirty(cashier);
                Debug.Log("[Stage12] Placed animated Cashier Employee at checkout counter.");
            }
        }
    }
}
#endif
