using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Economy;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Constructs a realistic, modern, and mobile-optimized 3D mini-market environment.
    /// Features:
    /// - Architectural exterior facade, glass entrance portal, and 3D brand signage
    /// - Clean commercial porcelain tile floor, baseboards, and structural wall columns
    /// - Drop-ceiling grid with recessed warm-white LED troffer panels
    /// - 6 double-sided center gondola shelves and perimeter wall shelving with price rails
    /// - 4 Commercial upright glass-door beverage and dairy chillers with illuminated headers
    /// - Modern checkout counter with conveyor belt, POS terminal, touch register, and impulse rack
    /// - Shopping basket station with stacked chrome/plastic retail baskets
    /// - 12 Neatly merchandised generic products with distinct 3D silhouettes, colors, and categories
    /// - Proper BoxColliders and mobile-first PBR materials
    /// </summary>
    [ExecuteAlways]
    public class MarketEnvironmentBuilder : MonoBehaviour
    {
        [Header("PBR Materials")]
        [SerializeField] private Material _floorMaterial;
        [SerializeField] private Material _wallMaterial;
        [SerializeField] private Material _sidewalkMaterial;
        [SerializeField] private Material _roadMaterial;
        [SerializeField] private Material _ceilingMaterial;
        [SerializeField] private Material _lightFixtureMaterial;
        [SerializeField] private Material _glassMaterial;
        [SerializeField] private Material _doorMetalMaterial;
        [SerializeField] private Material _signboardMaterial;
        [SerializeField] private Material _welcomeMatMaterial;
        [SerializeField] private Material _shelfMetalMaterial;
        [SerializeField] private Material _shelfTraysMaterial;
        [SerializeField] private Material _shelfAccentMaterial;
        [SerializeField] private Material _refrigeratorMaterial;
        [SerializeField] private Material _refrigeratorInteriorMaterial;
        [SerializeField] private Material _refrigeratorHeaderMaterial;
        [SerializeField] private Material _counterBodyMaterial;
        [SerializeField] private Material _counterTopMaterial;
        [SerializeField] private Material _conveyorBeltMaterial;
        [SerializeField] private Material _posScreenMaterial;
        [SerializeField] private Material _electronicsMaterial;
        [SerializeField] private Material _exitSignMaterial;
        [SerializeField] private Material _staffDoorMaterial;
        [SerializeField] private Material _basketPlasticMaterial;
        [SerializeField] private Material _chromeMaterial;
        [SerializeField] private Material _cardboardMaterial;
        [SerializeField] private Material _productsAtlasMaterial;

        [Header("Product PBR Materials")]
        [SerializeField] private Material _matProdWater;
        [SerializeField] private Material _matProdMilk;
        [SerializeField] private Material _matProdChips;
        [SerializeField] private Material _matProdChocolate;
        [SerializeField] private Material _matProdSoda;
        [SerializeField] private Material _matProdCanned;
        [SerializeField] private Material _matProdJuice;
        [SerializeField] private Material _matProdYogurt;
        [SerializeField] private Material _matProdCookies;
        [SerializeField] private Material _matProdCandy;
        [SerializeField] private Material _matProdSauce;
        [SerializeField] private Material _matProdRice;

        [Header("Generation Settings")]
        [SerializeField] private bool _rebuildOnStart = true;

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoAssignMaterials();
        }

        public void AutoAssignMaterials()
        {
            if (_floorMaterial == null) _floorMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Floor.mat");
            if (_wallMaterial == null) _wallMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Walls.mat");
            if (_sidewalkMaterial == null) _sidewalkMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Sidewalk.mat");
            if (_roadMaterial == null) _roadMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Road.mat");
            if (_ceilingMaterial == null) _ceilingMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Ceiling.mat");
            if (_lightFixtureMaterial == null) _lightFixtureMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_LightFixture.mat");
            if (_glassMaterial == null) _glassMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Glass.mat");
            if (_doorMetalMaterial == null) _doorMetalMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Door_Metal.mat");
            if (_signboardMaterial == null) _signboardMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Signboard.mat");
            if (_welcomeMatMaterial == null) _welcomeMatMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Welcome_Mat.mat");
            if (_shelfMetalMaterial == null) _shelfMetalMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Shelf_Metal.mat");
            if (_shelfTraysMaterial == null) _shelfTraysMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Shelf_Trays.mat");
            if (_shelfAccentMaterial == null) _shelfAccentMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Shelf_Accent.mat");
            if (_refrigeratorMaterial == null) _refrigeratorMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Refrigerator.mat");
            if (_refrigeratorInteriorMaterial == null) _refrigeratorInteriorMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Refrigerator_Interior.mat");
            if (_refrigeratorHeaderMaterial == null) _refrigeratorHeaderMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Refrigerator_Header.mat");
            if (_counterBodyMaterial == null) _counterBodyMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Counter_Body.mat");
            if (_counterTopMaterial == null) _counterTopMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Counter_Top.mat");
            if (_conveyorBeltMaterial == null) _conveyorBeltMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Belt_Rubber.mat");
            if (_posScreenMaterial == null) _posScreenMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_POS_Screen.mat");
            if (_electronicsMaterial == null) _electronicsMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Counter.mat");
            if (_exitSignMaterial == null) _exitSignMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Exit_Sign.mat");
            if (_staffDoorMaterial == null) _staffDoorMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Staff_Door.mat");
            if (_basketPlasticMaterial == null) _basketPlasticMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Basket_Red.mat");
            if (_chromeMaterial == null) _chromeMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Chrome.mat");
            if (_cardboardMaterial == null) _cardboardMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Cardboard.mat");
            if (_productsAtlasMaterial == null) _productsAtlasMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Products_Atlas.mat");

            // Products materials
            if (_matProdWater == null) _matProdWater = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Water.mat");
            if (_matProdMilk == null) _matProdMilk = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Milk.mat");
            if (_matProdChips == null) _matProdChips = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Chips.mat");
            if (_matProdChocolate == null) _matProdChocolate = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Chocolate.mat");
            if (_matProdSoda == null) _matProdSoda = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Soda.mat");
            if (_matProdCanned == null) _matProdCanned = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Canned.mat");
            if (_matProdJuice == null) _matProdJuice = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Juice.mat");
            if (_matProdYogurt == null) _matProdYogurt = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Yogurt.mat");
            if (_matProdCookies == null) _matProdCookies = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Cookies.mat");
            if (_matProdCandy == null) _matProdCandy = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Candy.mat");
            if (_matProdSauce == null) _matProdSauce = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Sauce.mat");
            if (_matProdRice == null) _matProdRice = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Prod_Rice.mat");
        }
#endif

        private void Start()
        {
#if UNITY_EDITOR
            AutoAssignMaterials();
#endif
            if (_rebuildOnStart || transform.childCount == 0)
            {
                BuildEnvironment();
            }
        }

        [ContextMenu("Rebuild Realistic Market Environment")]
        public void BuildEnvironment()
        {
            // Clear existing hierarchy
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            // 1. Root Category Containers
            Transform envRoot = CreateSubGroup("Environment");
            Transform storeRoot = CreateSubGroup("Store");

            // Ensure ShelfManager exists in scene
            if (storeRoot.GetComponent<ShelfManager>() == null)
            {
                storeRoot.gameObject.AddComponent<ShelfManager>();
            }

            // Ensure MarketExpansionManager exists in scene
            if (storeRoot.GetComponent<MarketExpansionManager>() == null)
            {
                storeRoot.gameObject.AddComponent<MarketExpansionManager>();
            }

            Transform exteriorGroup = CreateChildGroup("Exterior", envRoot);
            Transform buildingGroup = CreateChildGroup("Building", envRoot);
            Transform ceilingGroup = CreateChildGroup("Ceiling", envRoot);
            Transform decoGroup = CreateChildGroup("Decorations", envRoot);

            Transform shelvesGroup = CreateChildGroup("Shelves", storeRoot);
            Transform fridgeGroup = CreateChildGroup("Refrigerators", storeRoot);
            Transform checkoutGroup = CreateChildGroup("Checkout", storeRoot);
            Transform basketsGroup = CreateChildGroup("ShoppingBaskets", storeRoot);
            Transform expansionGroup = CreateChildGroup("Expansion", storeRoot);
            Transform productsGroup = CreateChildGroup("Products", storeRoot);

            // ==========================================
            // 2. EXTERIOR & FAÇADE
            // ==========================================
            BuildExterior(exteriorGroup);

            // ==========================================
            // 3. BUILDING ARCHITECTURE
            // ==========================================
            BuildBuildingArchitecture(buildingGroup);

            // ==========================================
            // 4. CEILING & LIGHTING FIXTURES
            // ==========================================
            BuildCeilingAndLights(ceilingGroup);

            // ==========================================
            // 5. DECORATIONS & SAFETY DETAILS
            // ==========================================
            BuildDecorations(decoGroup);

            // ==========================================
            // 6. RETAIL SHELVING (Gondolas & Wall Units)
            // ==========================================
            BuildShelving(shelvesGroup, productsGroup);

            // ==========================================
            // 7. COMMERCIAL REFRIGERATORS
            // ==========================================
            BuildRefrigerators(fridgeGroup, productsGroup);

            // ==========================================
            // 8. CHECKOUT / CASHIER AREA
            // ==========================================
            BuildCheckoutArea(checkoutGroup);

            // ==========================================
            // 9. SHOPPING BASKETS STATION
            // ==========================================
            BuildShoppingBaskets(basketsGroup);

            // ==========================================
            // 10. PHYSICAL MARKET EXPANSIONS (Levels 2 - 5)
            // ==========================================
            BuildExpansionAreas(expansionGroup, productsGroup);

            // Discover shelves and customer interaction points
            if (ShelfManager.HasInstance)
            {
                ShelfManager.Instance.DiscoverShelves();
            }

            var targetSelector = FindFirstObjectByType<CustomerTargetSelector>();
            if (targetSelector != null)
            {
                targetSelector.DiscoverInteractionPoints();
            }

            // Rebuild NavMesh
            var navMgr = FindFirstObjectByType<MarketNavMeshManager>();
            if (navMgr != null)
            {
                navMgr.BuildRuntimeNavMesh();
            }

            // Synchronize MarketExpansionManager with active store level
            if (MarketExpansionManager.HasInstance)
            {
                MarketExpansionManager.Instance.DiscoverExpansionAreas();
                int currentLvl = MarketUpgradeManager.HasInstance ? MarketUpgradeManager.Instance.CurrentLevel : 1;
                MarketExpansionManager.Instance.ApplyExpansionLevel(currentLvl, animate: false);
            }

            Debug.Log("[MarketEnvironmentBuilder] Realistic Modern 3D Mini Market successfully constructed with 12 organized shelves, physical expansions, and products!");
        }

        private void BuildExterior(Transform parent)
        {
            // Exterior Sidewalk (18m x 6m x 0.2m)
            CreateBox("Exterior_Sidewalk", new Vector3(0f, -0.1f, -13f), new Vector3(18f, 0.2f, 6f), _sidewalkMaterial, parent, addCollider: true);

            // Sidewalk Curbs
            CreateBox("Sidewalk_Curb", new Vector3(0f, -0.05f, -16f), new Vector3(18f, 0.25f, 0.25f), _doorMetalMaterial, parent);

            // Road Asphalt (22m x 6m x 0.2m)
            CreateBox("Exterior_Road", new Vector3(0f, -0.15f, -19.1f), new Vector3(22f, 0.2f, 6f), _roadMaterial, parent, addCollider: true);

            // Storefront Architectural Portal
            CreateBox("Storefront_Top_Fascia", new Vector3(0f, 3.85f, -10f), new Vector3(14.6f, 0.7f, 0.6f), _doorMetalMaterial, parent);
            CreateBox("Storefront_Left_Pillar", new Vector3(-7.15f, 2f, -10f), new Vector3(0.5f, 4.4f, 0.6f), _doorMetalMaterial, parent);
            CreateBox("Storefront_Right_Pillar", new Vector3(7.15f, 2f, -10f), new Vector3(0.5f, 4.4f, 0.6f), _doorMetalMaterial, parent);

            // 3D Brand Signboard ("MINI MARKET")
            CreateBox("Storefront_Signboard", new Vector3(0f, 3.9f, -10.32f), new Vector3(5.5f, 1.35f, 0.15f), _signboardMaterial, parent);
            CreateBox("Signboard_Canopy", new Vector3(0f, 4.6f, -10.45f), new Vector3(6.0f, 0.15f, 0.45f), _doorMetalMaterial, parent);

            // Storefront Plate Glass Windows
            CreateBox("Window_Glass_Left", new Vector3(-4.5f, 1.8f, -10f), new Vector3(4.6f, 3.2f, 0.08f), _glassMaterial, parent);
            CreateBox("Window_Frame_Left_Bottom", new Vector3(-4.5f, 0.15f, -10f), new Vector3(4.8f, 0.3f, 0.25f), _doorMetalMaterial, parent);
            CreateBox("Window_Mullion_Left_1", new Vector3(-4.5f, 1.8f, -10f), new Vector3(0.12f, 3.2f, 0.15f), _doorMetalMaterial, parent);

            CreateBox("Window_Glass_Right", new Vector3(4.5f, 1.8f, -10f), new Vector3(4.6f, 3.2f, 0.08f), _glassMaterial, parent);
            CreateBox("Window_Frame_Right_Bottom", new Vector3(4.5f, 0.15f, -10f), new Vector3(4.8f, 0.3f, 0.25f), _doorMetalMaterial, parent);
            CreateBox("Window_Mullion_Right_1", new Vector3(4.5f, 1.8f, -10f), new Vector3(0.12f, 3.2f, 0.15f), _doorMetalMaterial, parent);

            // Entrance Glass Transom Above Sliding Doors
            CreateBox("Entrance_Transom_Glass", new Vector3(0f, 2.95f, -10f), new Vector3(3.8f, 0.9f, 0.08f), _glassMaterial, parent);
            CreateBox("Entrance_Header_Bar", new Vector3(0f, 2.45f, -10f), new Vector3(4.0f, 0.12f, 0.25f), _doorMetalMaterial, parent);

            // Glass Sliding Doors
            CreateBox("Door_Glass_Left", new Vector3(-0.95f, 1.2f, -10f), new Vector3(1.8f, 2.35f, 0.08f), _glassMaterial, parent);
            CreateBox("Door_Handle_Left", new Vector3(-0.15f, 1.15f, -10.08f), new Vector3(0.04f, 0.9f, 0.06f), _chromeMaterial, parent);

            CreateBox("Door_Glass_Right", new Vector3(0.95f, 1.2f, -10f), new Vector3(1.8f, 2.35f, 0.08f), _glassMaterial, parent);
            CreateBox("Door_Handle_Right", new Vector3(0.15f, 1.15f, -10.08f), new Vector3(0.04f, 0.9f, 0.06f), _chromeMaterial, parent);

            // Welcome Walk-off Mat
            CreateBox("Welcome_Mat", new Vector3(0f, 0.01f, -10.8f), new Vector3(2.6f, 0.02f, 1.3f), _welcomeMatMaterial, parent);

            // Exterior Details: Modern Commercial Waste Bin & Planter
            CreateBox("Exterior_TrashBin", new Vector3(-6.2f, 0.5f, -11.2f), new Vector3(0.6f, 1.0f, 0.6f), _doorMetalMaterial, parent, addCollider: true);
            CreateBox("Exterior_TrashBin_Top", new Vector3(-6.2f, 1.02f, -11.2f), new Vector3(0.65f, 0.08f, 0.65f), _chromeMaterial, parent);

            CreateBox("Exterior_Planter", new Vector3(6.2f, 0.4f, -11.2f), new Vector3(1.0f, 0.8f, 0.8f), _sidewalkMaterial, parent, addCollider: true);
            CreateBox("Exterior_Plant_Shrub", new Vector3(6.2f, 0.95f, -11.2f), new Vector3(0.85f, 0.5f, 0.7f), _shelfAccentMaterial, parent);
        }

        private void BuildBuildingArchitecture(Transform parent)
        {
            // Store Floor (14m x 20m x 0.2m)
            CreateBox("Store_Floor", new Vector3(0f, -0.1f, 0f), new Vector3(14f, 0.2f, 20f), _floorMaterial, parent, addCollider: true);

            // Back Wall (Z: 10m) - Segmented to provide Grand Pavilion Portal (X: -3.0 to +2.0, width 5.0m)
            CreateBox("Wall_Back_Left", new Vector3(-5.0f, 1.75f, 10f), new Vector3(4.0f, 3.5f, 0.3f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Back_Right", new Vector3(4.5f, 1.75f, 10f), new Vector3(5.0f, 3.5f, 0.3f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Back_Lintel_Level5", new Vector3(-0.5f, 3.05f, 10f), new Vector3(5.0f, 0.9f, 0.3f), _wallMaterial, parent, addCollider: true);

            // Left Wall (X: -7m) - Segmented to provide Chiller Bay Portal (Z: 6.0 to 9.0, width 3.0m)
            CreateBox("Wall_Left_Main", new Vector3(-7f, 1.75f, -2.0f), new Vector3(0.3f, 3.5f, 16.0f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Left_Corner", new Vector3(-7f, 1.75f, 9.5f), new Vector3(0.3f, 3.5f, 1.0f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Left_Lintel_Level4", new Vector3(-7f, 3.05f, 7.5f), new Vector3(0.3f, 0.9f, 3.0f), _wallMaterial, parent, addCollider: true);

            // Right Wall (X: 7m) - Segmented to provide Portal 1 (Z: -6.8 to -4.2) & Portal 2 (Z: 4.2 to 6.8)
            CreateBox("Wall_Right_Front", new Vector3(7f, 1.75f, -8.4f), new Vector3(0.3f, 3.5f, 3.2f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Right_Mid", new Vector3(7f, 1.75f, 0f), new Vector3(0.3f, 3.5f, 8.4f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Right_Back", new Vector3(7f, 1.75f, 8.4f), new Vector3(0.3f, 3.5f, 3.2f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Right_Lintel_Level2", new Vector3(7f, 3.05f, -5.5f), new Vector3(0.3f, 0.9f, 2.6f), _wallMaterial, parent, addCollider: true);
            CreateBox("Wall_Right_Lintel_Level3", new Vector3(7f, 3.05f, 5.5f), new Vector3(0.3f, 0.9f, 2.6f), _wallMaterial, parent, addCollider: true);

            // Baseboards
            CreateBox("Baseboard_Back_Left", new Vector3(-5.0f, 0.08f, 9.84f), new Vector3(4.0f, 0.16f, 0.04f), _doorMetalMaterial, parent);
            CreateBox("Baseboard_Back_Right", new Vector3(4.5f, 0.08f, 9.84f), new Vector3(5.0f, 0.16f, 0.04f), _doorMetalMaterial, parent);
            CreateBox("Baseboard_Left_Main", new Vector3(-6.84f, 0.08f, -2.0f), new Vector3(0.04f, 0.16f, 16.0f), _doorMetalMaterial, parent);
            CreateBox("Baseboard_Left_Corner", new Vector3(-6.84f, 0.08f, 9.5f), new Vector3(0.04f, 0.16f, 1.0f), _doorMetalMaterial, parent);
            CreateBox("Baseboard_Right_Front", new Vector3(6.84f, 0.08f, -8.4f), new Vector3(0.04f, 0.16f, 3.2f), _doorMetalMaterial, parent);
            CreateBox("Baseboard_Right_Mid", new Vector3(6.84f, 0.08f, 0f), new Vector3(0.04f, 0.16f, 8.4f), _doorMetalMaterial, parent);
            CreateBox("Baseboard_Right_Back", new Vector3(6.84f, 0.08f, 8.4f), new Vector3(0.04f, 0.16f, 3.2f), _doorMetalMaterial, parent);

            // Structural Columns Framing Portals
            CreateBox("Column_Right_1", new Vector3(6.75f, 1.75f, -6.95f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);
            CreateBox("Column_Right_2", new Vector3(6.75f, 1.75f, -4.05f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);
            CreateBox("Column_Right_3", new Vector3(6.75f, 1.75f, 4.05f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);
            CreateBox("Column_Right_4", new Vector3(6.75f, 1.75f, 6.95f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);

            CreateBox("Column_Left_1", new Vector3(-6.75f, 1.75f, -6.0f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);
            CreateBox("Column_Left_2", new Vector3(-6.75f, 1.75f, 0.0f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);
            CreateBox("Column_Left_3", new Vector3(-6.75f, 1.75f, 5.85f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);
            CreateBox("Column_Left_4", new Vector3(-6.75f, 1.75f, 9.15f), new Vector3(0.35f, 3.5f, 0.35f), _wallMaterial, parent);

            // Staff & Storage Area Door
            CreateBox("Staff_Door_Leaf", new Vector3(5.2f, 1.15f, 9.83f), new Vector3(1.2f, 2.3f, 0.04f), _staffDoorMaterial, parent);
            CreateBox("Staff_Door_Frame", new Vector3(5.2f, 1.15f, 9.82f), new Vector3(1.3f, 2.38f, 0.06f), _doorMetalMaterial, parent);
            CreateBox("Staff_Door_Handle", new Vector3(4.75f, 1.05f, 9.78f), new Vector3(0.06f, 0.18f, 0.06f), _chromeMaterial, parent);

            GameObject staffBarrier = new GameObject("Staff_Area_NavMesh_Barrier");
            staffBarrier.transform.parent = parent;
            staffBarrier.transform.position = new Vector3(5.2f, 1.0f, 8.8f);
            NavMeshObstacle sObs = staffBarrier.AddComponent<NavMeshObstacle>();
            sObs.size = new Vector3(2.2f, 2.0f, 2.0f);
            sObs.carving = true;
        }

        private void BuildCeilingAndLights(Transform parent)
        {
            CreateBox("Ceiling_Plane", new Vector3(0f, 3.55f, 0f), new Vector3(14f, 0.15f, 20f), _ceilingMaterial, parent);

            float[] xCols = new float[] { -3.2f, 3.2f };
            float[] zRows = new float[] { -6f, -1.5f, 3f, 7.5f };

            int lightIndex = 1;
            foreach (float x in xCols)
            {
                foreach (float z in zRows)
                {
                    CreateBox($"Ceiling_LED_Troffer_{lightIndex}", new Vector3(x, 3.48f, z), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, parent);
                    CreateBox($"Light_Housing_{lightIndex}", new Vector3(x, 3.51f, z), new Vector3(1.5f, 0.06f, 2.7f), _doorMetalMaterial, parent);
                    lightIndex++;
                }
            }

            CreateBox("HVAC_Duct_Main", new Vector3(0f, 3.42f, 0f), new Vector3(0.8f, 0.35f, 18.5f), _chromeMaterial, parent);
            for (float z = -6f; z <= 6f; z += 4f)
            {
                CreateBox($"HVAC_Vent_Diffuser_{z:F0}", new Vector3(0f, 3.23f, z), new Vector3(0.7f, 0.04f, 0.7f), _doorMetalMaterial, parent);
            }
        }

        private void BuildDecorations(Transform parent)
        {
            CreateBox("Emergency_Exit_Sign", new Vector3(0f, 2.6f, -9.85f), new Vector3(0.9f, 0.35f, 0.08f), _exitSignMaterial, parent);

            GameObject extinguisher = new GameObject("Fire_Extinguisher_Station");
            extinguisher.transform.parent = parent;
            extinguisher.transform.position = new Vector3(-6.75f, 1.1f, -8.5f);

            CreateBox("Extinguisher_Tank", new Vector3(-6.75f, 1.1f, -8.5f), new Vector3(0.2f, 0.55f, 0.2f), _basketPlasticMaterial, extinguisher.transform);
            CreateBox("Extinguisher_Valve", new Vector3(-6.75f, 1.42f, -8.5f), new Vector3(0.12f, 0.15f, 0.12f), _doorMetalMaterial, extinguisher.transform);
            CreateBox("Extinguisher_Bracket", new Vector3(-6.82f, 1.1f, -8.5f), new Vector3(0.04f, 0.4f, 0.22f), _chromeMaterial, extinguisher.transform);

            Vector3[] boxPositions = new Vector3[]
            {
                new Vector3(-5.2f, 0.3f, 8.2f),
                new Vector3(-4.4f, 0.3f, 8.2f),
                new Vector3(-4.8f, 0.85f, 8.2f),
                new Vector3(-5.2f, 0.3f, 9.0f),
                new Vector3(-4.4f, 0.3f, 9.0f)
            };
            int bIdx = 1;
            foreach (var pos in boxPositions)
            {
                CreateBox($"Supply_Box_{bIdx++}", pos, new Vector3(0.75f, 0.55f, 0.75f), _cardboardMaterial, parent, addCollider: true);
            }
        }

        private void BuildShelving(Transform parent, Transform productsGroup)
        {
            // 6 Double-Sided Center Gondola Shelves (Aisle 1 Left, Aisle 2 Right)
            // Left Aisle (X: -3.2m): Snacks & Sweets
            CreateDoubleGondola("Shelf_Gondola_Chips", new Vector3(-3.2f, 0f, -2.5f), "CRUNCH CHIPS", parent, productsGroup, ProductType.Chips, "prod_chips");
            CreateDoubleGondola("Shelf_Gondola_Cookies", new Vector3(-3.2f, 0f, 1.5f), "BAKED COOKIES", parent, productsGroup, ProductType.Cookies, "prod_cookies");
            CreateDoubleGondola("Shelf_Gondola_Chocolate", new Vector3(-3.2f, 0f, 5.5f), "SWEET CHOCOLATE", parent, productsGroup, ProductType.Chocolate, "prod_chocolate");

            // Right Aisle (X: 3.2m): Candy, Water, Canned Meals
            CreateDoubleGondola("Shelf_Gondola_Candy", new Vector3(3.2f, 0f, -2.5f), "CANDY & SWEETS", parent, productsGroup, ProductType.Candy, "prod_candy");
            CreateDoubleGondola("Shelf_Gondola_Water", new Vector3(3.2f, 0f, 1.5f), "PURE WATER", parent, productsGroup, ProductType.Water, "prod_water");
            CreateDoubleGondola("Shelf_Gondola_Canned", new Vector3(3.2f, 0f, 5.5f), "CANNED MEALS", parent, productsGroup, ProductType.Canned, "prod_canned");

            // 2 Perimeter Wall Shelving Units along the Right Wall (X: 6.55m)
            CreateWallShelf("Wall_Shelf_Sauce", new Vector3(6.55f, 0f, -2.5f), "TOMATO SAUCE", parent, productsGroup, ProductType.Sauce, "prod_sauce");
            CreateWallShelf("Wall_Shelf_Rice", new Vector3(6.55f, 0f, 2.5f), "PREMIUM RICE", parent, productsGroup, ProductType.Rice, "prod_rice");
        }

        private ShelfVisualController CreateDoubleGondola(string name, Vector3 pos, string categoryLabel, Transform parent, Transform productsGroup, ProductType prodType, string productId)
        {
            GameObject gondola = new GameObject(name);
            gondola.transform.parent = parent;
            gondola.transform.position = pos;

            CreateBox("Base_Plinth", pos + new Vector3(0f, 0.1f, 0f), new Vector3(1.5f, 0.2f, 0.85f), _shelfMetalMaterial, gondola.transform, addCollider: true);
            CreateBox("Backboard_Center", pos + new Vector3(0f, 0.95f, 0f), new Vector3(1.5f, 1.7f, 0.06f), _shelfMetalMaterial, gondola.transform);
            CreateBox("Post_Left", pos + new Vector3(-0.73f, 0.95f, 0f), new Vector3(0.06f, 1.7f, 0.85f), _shelfMetalMaterial, gondola.transform);
            CreateBox("Post_Right", pos + new Vector3(0.73f, 0.95f, 0f), new Vector3(0.06f, 1.7f, 0.85f), _shelfMetalMaterial, gondola.transform);
            CreateBox("Category_Header", pos + new Vector3(0f, 1.95f, 0f), new Vector3(1.35f, 0.25f, 0.15f), _shelfAccentMaterial, gondola.transform);

            List<GameObject> visualItems = new List<GameObject>();

            // 4 Shelf Tiers on both front (+Z) and back (-Z) sides
            float[] tierHeights = new float[] { 0.25f, 0.65f, 1.1f, 1.55f };
            for (int t = 0; t < tierHeights.Length; t++)
            {
                float h = tierHeights[t];
                CreateBox($"Tray_Front_{t}", pos + new Vector3(0f, h, 0.22f), new Vector3(1.4f, 0.04f, 0.38f), _shelfTraysMaterial, gondola.transform);
                CreateBox($"PriceRail_Front_{t}", pos + new Vector3(0f, h + 0.02f, 0.41f), new Vector3(1.4f, 0.04f, 0.02f), _shelfAccentMaterial, gondola.transform);

                CreateBox($"Tray_Back_{t}", pos + new Vector3(0f, h, -0.22f), new Vector3(1.4f, 0.04f, 0.38f), _shelfTraysMaterial, gondola.transform);
                CreateBox($"PriceRail_Back_{t}", pos + new Vector3(0f, h + 0.02f, -0.41f), new Vector3(1.4f, 0.04f, 0.02f), _shelfAccentMaterial, gondola.transform);

                // Stock exactly 8 visual items across middle tiers
                if (t == 1)
                {
                    StockShelfRow(pos + new Vector3(0f, h + 0.02f, 0.22f), prodType, productId, productsGroup, visualItems, count: 4);
                }
                else if (t == 2)
                {
                    StockShelfRow(pos + new Vector3(0f, h + 0.02f, -0.22f), prodType, productId, productsGroup, visualItems, count: 4);
                }
            }

            // Customer interaction points (Front +Z, Back -Z)
            GameObject ptFront = new GameObject("Interaction_Point_Front");
            ptFront.transform.parent = gondola.transform;
            ptFront.transform.position = pos + new Vector3(0f, 0f, 0.85f);
            var ipFront = ptFront.AddComponent<CustomerInteractionPoint>();
            ipFront.Initialize(categoryLabel, -Vector3.forward);

            GameObject ptBack = new GameObject("Interaction_Point_Back");
            ptBack.transform.parent = gondola.transform;
            ptBack.transform.position = pos + new Vector3(0f, 0f, -0.85f);
            var ipBack = ptBack.AddComponent<CustomerInteractionPoint>();
            ipBack.Initialize(categoryLabel, Vector3.forward);

            var shelfVis = gondola.AddComponent<ShelfVisualController>();
            ProductData pData = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;
            shelfVis.Initialize(pData, ShelfType.StandardShelf, visualItems, new List<CustomerInteractionPoint> { ipFront, ipBack });

            NavMeshObstacle obs = gondola.AddComponent<NavMeshObstacle>();
            obs.center = new Vector3(0f, 1.0f, 0f);
            obs.size = new Vector3(1.6f, 2.0f, 0.95f);
            obs.carving = true;

            return shelfVis;
        }

        private void CreateWallShelf(string name, Vector3 pos, string categoryLabel, Transform parent, Transform productsGroup, ProductType prodType, string productId)
        {
            GameObject wallShelf = new GameObject(name);
            wallShelf.transform.parent = parent;
            wallShelf.transform.position = pos;

            CreateBox("Base_Plinth", pos + new Vector3(0f, 0.1f, 0f), new Vector3(0.55f, 0.2f, 2.6f), _shelfMetalMaterial, wallShelf.transform, addCollider: true);
            CreateBox("Backboard", pos + new Vector3(0.24f, 1.15f, 0f), new Vector3(0.06f, 2.1f, 2.6f), _shelfMetalMaterial, wallShelf.transform);

            List<GameObject> visualItems = new List<GameObject>();

            float[] tierHeights = new float[] { 0.25f, 0.7f, 1.15f, 1.6f, 2.05f };
            for (int t = 0; t < tierHeights.Length; t++)
            {
                float h = tierHeights[t];
                CreateBox($"Tray_{t}", pos + new Vector3(0f, h, 0f), new Vector3(0.48f, 0.04f, 2.5f), _shelfTraysMaterial, wallShelf.transform);
                CreateBox($"PriceRail_{t}", pos + new Vector3(-0.24f, h + 0.02f, 0f), new Vector3(0.02f, 0.04f, 2.5f), _shelfAccentMaterial, wallShelf.transform);

                if (t == 1)
                {
                    StockWallRow(pos + new Vector3(-0.05f, h + 0.02f, 0f), prodType, productId, productsGroup, visualItems, count: 4);
                }
                else if (t == 2)
                {
                    StockWallRow(pos + new Vector3(-0.05f, h + 0.02f, 0f), prodType, productId, productsGroup, visualItems, count: 4);
                }
            }

            GameObject pt = new GameObject("Interaction_Point");
            pt.transform.parent = wallShelf.transform;
            pt.transform.position = pos + new Vector3(-0.85f, 0f, 0f);
            var ip = pt.AddComponent<CustomerInteractionPoint>();
            ip.Initialize(categoryLabel, Vector3.right);

            var shelfVis = wallShelf.AddComponent<ShelfVisualController>();
            ProductData pData = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;
            shelfVis.Initialize(pData, ShelfType.StandardShelf, visualItems, new List<CustomerInteractionPoint> { ip });

            NavMeshObstacle obs = wallShelf.AddComponent<NavMeshObstacle>();
            obs.center = new Vector3(0f, 1.1f, 0f);
            obs.size = new Vector3(0.65f, 2.2f, 2.7f);
            obs.carving = true;
        }

        private void BuildRefrigerators(Transform parent, Transform productsGroup)
        {
            // 4 Upright Commercial Beverage & Dairy Chillers along Left Wall (X: -6.45m)
            CreateChillerUnit("Cooler_Soda", new Vector3(-6.45f, 0f, -4.5f), "CHILLED SODAS", parent, productsGroup, ProductType.Soda, "prod_soda");
            CreateChillerUnit("Cooler_Juice", new Vector3(-6.45f, 0f, -1.5f), "FRESH JUICES", parent, productsGroup, ProductType.Juice, "prod_juice");
            CreateChillerUnit("Cooler_Milk", new Vector3(-6.45f, 0f, 1.5f), "DAILY MILK", parent, productsGroup, ProductType.Milk, "prod_milk");
            CreateChillerUnit("Cooler_Yogurt", new Vector3(-6.45f, 0f, 4.5f), "FARM YOGURT", parent, productsGroup, ProductType.Yogurt, "prod_yogurt");
        }

        private ShelfVisualController CreateChillerUnit(string name, Vector3 pos, string headerTitle, Transform parent, Transform productsGroup, ProductType prodType, string productId)
        {
            GameObject cooler = new GameObject(name);
            cooler.transform.parent = parent;
            cooler.transform.position = pos;

            // Outer cabinet
            CreateBox("Cooler_Cabinet", pos + new Vector3(0f, 1.125f, 0f), new Vector3(0.75f, 2.25f, 2.2f), _refrigeratorMaterial, cooler.transform, addCollider: true);
            CreateBox("Interior_Chamber", pos + new Vector3(0.05f, 1.05f, 0f), new Vector3(0.65f, 1.65f, 2.05f), _refrigeratorInteriorMaterial, cooler.transform);
            CreateBox("Header_Canopy", pos + new Vector3(0.38f, 2.05f, 0f), new Vector3(0.04f, 0.35f, 2.15f), _refrigeratorHeaderMaterial, cooler.transform);

            // Double Glass Front Doors with Handles
            CreateBox("Glass_Door_A", pos + new Vector3(0.38f, 1.05f, -0.52f), new Vector3(0.04f, 1.65f, 0.98f), _glassMaterial, cooler.transform);
            CreateBox("Handle_A", pos + new Vector3(0.42f, 1.05f, -0.1f), new Vector3(0.04f, 0.75f, 0.04f), _chromeMaterial, cooler.transform);

            CreateBox("Glass_Door_B", pos + new Vector3(0.38f, 1.05f, 0.52f), new Vector3(0.04f, 1.65f, 0.98f), _glassMaterial, cooler.transform);
            CreateBox("Handle_B", pos + new Vector3(0.42f, 1.05f, 0.1f), new Vector3(0.04f, 0.75f, 0.04f), _chromeMaterial, cooler.transform);

            List<GameObject> visualItems = new List<GameObject>();

            // 4 Internal wire shelf racks
            float[] rackHeights = new float[] { 0.45f, 0.85f, 1.25f, 1.65f };
            for (int r = 0; r < rackHeights.Length; r++)
            {
                float h = rackHeights[r];
                CreateBox($"Wire_Rack_{r}", pos + new Vector3(0.05f, h, 0f), new Vector3(0.6f, 0.02f, 2.0f), _chromeMaterial, cooler.transform);

                // Stock exactly 8 visual items across middle racks
                if (r == 1)
                {
                    StockCoolerRow(pos + new Vector3(0.08f, h + 0.01f, 0f), prodType, productId, productsGroup, visualItems, count: 4);
                }
                else if (r == 2)
                {
                    StockCoolerRow(pos + new Vector3(0.08f, h + 0.01f, 0f), prodType, productId, productsGroup, visualItems, count: 4);
                }
            }

            GameObject pt = new GameObject("Interaction_Point");
            pt.transform.parent = cooler.transform;
            pt.transform.position = pos + new Vector3(0.85f, 0f, 0f);
            var ip = pt.AddComponent<CustomerInteractionPoint>();
            ip.Initialize(headerTitle, -Vector3.right);

            var shelfVis = cooler.AddComponent<ShelfVisualController>();
            ProductData pData = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;
            shelfVis.Initialize(pData, ShelfType.RefrigeratedShelf, visualItems, new List<CustomerInteractionPoint> { ip });

            NavMeshObstacle obs = cooler.AddComponent<NavMeshObstacle>();
            obs.center = new Vector3(0f, 1.125f, 0f);
            obs.size = new Vector3(0.85f, 2.25f, 2.3f);
            obs.carving = true;

            return shelfVis;
        }

        private void BuildCheckoutArea(Transform parent)
        {
            GameObject checkoutRoot = new GameObject("Checkout_Zone");
            checkoutRoot.transform.parent = parent;
            Vector3 basePos = new Vector3(-2.8f, 0f, -6.5f);
            checkoutRoot.transform.position = basePos;

            CreateBox("Counter_Main_Body", basePos + new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 0.85f), _counterBodyMaterial, checkoutRoot.transform, addCollider: true);
            CreateBox("Countertop_Surface", basePos + new Vector3(0f, 0.91f, 0f), new Vector3(2.65f, 0.04f, 0.9f), _counterTopMaterial, checkoutRoot.transform);

            CreateBox("Conveyor_Belt_Bed", basePos + new Vector3(-0.35f, 0.93f, 0f), new Vector3(1.6f, 0.02f, 0.65f), _conveyorBeltMaterial, checkoutRoot.transform);
            CreateBox("Conveyor_Rail_Front", basePos + new Vector3(-0.35f, 0.96f, -0.34f), new Vector3(1.6f, 0.05f, 0.03f), _chromeMaterial, checkoutRoot.transform);
            CreateBox("Conveyor_Rail_Back", basePos + new Vector3(-0.35f, 0.96f, 0.34f), new Vector3(1.6f, 0.05f, 0.03f), _chromeMaterial, checkoutRoot.transform);

            // Cashier Register Monitor
            GameObject reg = new GameObject("Cashier_Register_Monitor");
            reg.transform.parent = checkoutRoot.transform;
            CreateBox("Register_Base", basePos + new Vector3(0.85f, 1.0f, 0.15f), new Vector3(0.2f, 0.15f, 0.2f), _electronicsMaterial, reg.transform);
            CreateBox("Register_Screen", basePos + new Vector3(0.85f, 1.2f, 0.15f), new Vector3(0.35f, 0.28f, 0.04f), _posScreenMaterial, reg.transform);

            // Customer POS PIN Pad Terminal
            GameObject posTerm = new GameObject("POS_Terminal");
            posTerm.transform.parent = checkoutRoot.transform;
            CreateBox("POS_Stand", basePos + new Vector3(0.45f, 1.0f, -0.25f), new Vector3(0.12f, 0.15f, 0.12f), _doorMetalMaterial, posTerm.transform);
            CreateBox("POS_Keypad", basePos + new Vector3(0.45f, 1.12f, -0.25f), new Vector3(0.14f, 0.05f, 0.22f), _electronicsMaterial, posTerm.transform);

            // Barcode Scanner Bed
            CreateBox("Barcode_Scanner_Glass", basePos + new Vector3(0.55f, 0.94f, 0.08f), new Vector3(0.22f, 0.01f, 0.25f), _glassMaterial, checkoutRoot.transform);

            // Bag Stand
            CreateBox("Bag_Stand_Post", basePos + new Vector3(1.2f, 1.15f, 0f), new Vector3(0.04f, 0.5f, 0.04f), _chromeMaterial, checkoutRoot.transform);
            CreateBox("Bag_Stand_Arm", basePos + new Vector3(1.2f, 1.38f, 0f), new Vector3(0.25f, 0.03f, 0.25f), _chromeMaterial, checkoutRoot.transform);
            CreateBox("Plastic_Bags_Pack", basePos + new Vector3(1.2f, 1.15f, 0.08f), new Vector3(0.22f, 0.35f, 0.08f), _floorMaterial, checkoutRoot.transform);

            // Front Impulse Rack
            CreateBox("Impulse_Rack_Frame", basePos + new Vector3(-0.35f, 0.6f, -0.46f), new Vector3(1.4f, 0.5f, 0.12f), _shelfTraysMaterial, checkoutRoot.transform);
            CreateBox("Impulse_Gums_Row1", basePos + new Vector3(-0.35f, 0.75f, -0.46f), new Vector3(1.3f, 0.08f, 0.08f), _shelfAccentMaterial, checkoutRoot.transform);
            CreateBox("Impulse_Gums_Row2", basePos + new Vector3(-0.35f, 0.5f, -0.46f), new Vector3(1.3f, 0.08f, 0.08f), _basketPlasticMaterial, checkoutRoot.transform);

            NavMeshObstacle obs = checkoutRoot.AddComponent<NavMeshObstacle>();
            obs.center = new Vector3(0f, 0.6f, 0f);
            obs.size = new Vector3(2.7f, 1.2f, 0.95f);
            obs.carving = true;

            GameObject cashierBarrier = new GameObject("Cashier_Barrier");
            cashierBarrier.transform.parent = checkoutRoot.transform;
            cashierBarrier.transform.localPosition = new Vector3(-0.35f, 0.5f, 0.85f);
            NavMeshObstacle bObs = cashierBarrier.AddComponent<NavMeshObstacle>();
            bObs.size = new Vector3(2.2f, 1.0f, 0.8f);
            bObs.carving = true;

            GameObject checkoutPoint = new GameObject("Checkout_Interaction_Point");
            checkoutPoint.transform.parent = checkoutRoot.transform;
            checkoutPoint.transform.position = new Vector3(-2.0f, 0f, -5.6f);
            var cp = checkoutPoint.AddComponent<CustomerInteractionPoint>();
            cp.Initialize("Checkout", new Vector3(0f, 0f, -1f));
        }

        private void BuildShoppingBaskets(Transform parent)
        {
            GameObject station = new GameObject("Basket_Station");
            station.transform.parent = parent;
            Vector3 bPos = new Vector3(-2.2f, 0f, -8.8f);
            station.transform.position = bPos;

            CreateBox("Stand_Base", bPos + new Vector3(0f, 0.05f, 0f), new Vector3(0.55f, 0.08f, 0.45f), _chromeMaterial, station.transform, addCollider: true);
            CreateBox("Stand_Back_Bar", bPos + new Vector3(0f, 0.5f, -0.2f), new Vector3(0.5f, 0.95f, 0.04f), _chromeMaterial, station.transform);

            for (int i = 0; i < 4; i++)
            {
                float y = 0.12f + i * 0.1f;
                CreateBox($"Basket_{i}", bPos + new Vector3(0f, y + 0.12f, 0f), new Vector3(0.48f, 0.22f, 0.36f), _basketPlasticMaterial, station.transform);
                CreateBox($"Handle_{i}", bPos + new Vector3(0f, y + 0.24f, 0f), new Vector3(0.42f, 0.03f, 0.03f), _doorMetalMaterial, station.transform);
            }
        }

        // ==========================================
        // PHYSICAL STORE EXPANSIONS (AŞAMA 7)
        // ==========================================
        private void BuildExpansionAreas(Transform parent, Transform productsGroup)
        {
            BuildExpansionLevel2(parent, productsGroup);
            BuildExpansionLevel3(parent, productsGroup);
            BuildExpansionLevel4(parent, productsGroup);
            BuildExpansionLevel5(parent, productsGroup);
        }

        private void BuildExpansionLevel2(Transform parent, Transform productsGroup)
        {
            GameObject areaRoot = new GameObject("Expansion_Level2");
            areaRoot.transform.parent = parent;
            areaRoot.transform.localPosition = Vector3.zero;

            Transform contentGroup = CreateChildGroup("Content", areaRoot.transform);

            // Floor (10.5m, -0.1m, -5.0m) - Size: 7m x 10m x 0.2m (Area = 70 m²)
            CreateBox("Floor_Lvl2", new Vector3(10.5f, -0.1f, -5.0f), new Vector3(7.0f, 0.2f, 10.0f), _floorMaterial, contentGroup, addCollider: true);

            // Perimeter Walls
            CreateBox("Wall_Lvl2_North", new Vector3(10.5f, 1.75f, 0.0f), new Vector3(7.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl2_South", new Vector3(10.5f, 1.75f, -10.0f), new Vector3(7.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl2_East", new Vector3(14.0f, 1.75f, -5.0f), new Vector3(0.3f, 3.5f, 10.0f), _wallMaterial, contentGroup, addCollider: true);

            // Ceiling & Lighting
            CreateBox("Ceiling_Lvl2", new Vector3(10.5f, 3.55f, -5.0f), new Vector3(7.0f, 0.15f, 10.0f), _ceilingMaterial, contentGroup);

            List<GameObject> lights = new List<GameObject>();
            GameObject t1 = CreateBox("LED_Troffer_Lvl2_A", new Vector3(10.5f, 3.48f, -7.0f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            GameObject t2 = CreateBox("LED_Troffer_Lvl2_B", new Vector3(10.5f, 3.48f, -3.0f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            lights.Add(t1);
            lights.Add(t2);

            // Gate at Portal 1 (X: 7.0f, Z: -5.5f)
            Collider gateCol;
            NavMeshObstacle gateObs;
            GameObject gateObj = CreateExpansionGate("Gate_Level2", new Vector3(7.0f, 1.3f, -5.5f), new Vector3(0.12f, 2.6f, 2.5f), areaRoot.transform, out gateCol, out gateObs);

            // 3D Lock Indicator facing into base store (West)
            GameObject indicator = CreateExpansionIndicator("Indicator_Level2", new Vector3(6.82f, 1.6f, -5.5f), Quaternion.Euler(0f, -90f, 0f), 2, areaRoot.transform);

            // Shelves in Level 2 Expansion (Chips & Water)
            List<ShelfVisualController> shelves = new List<ShelfVisualController>();
            var s1 = CreateDoubleGondola("Expansion_Shelf_Chips", new Vector3(10.5f, 0f, -7.0f), "DELI CHIPS", contentGroup, productsGroup, ProductType.Chips, "prod_chips");
            var s2 = CreateDoubleGondola("Expansion_Shelf_Water", new Vector3(10.5f, 0f, -3.0f), "MINERAL WATER", contentGroup, productsGroup, ProductType.Water, "prod_water");
            if (s1 != null) shelves.Add(s1);
            if (s2 != null) shelves.Add(s2);

            var areaComp = areaRoot.AddComponent<MarketExpansionArea>();
            areaComp.Configure(2, "Expanded Mini Mart", "East Wing - Fresh Deli", gateObj, gateCol, gateObs, indicator, contentGroup.gameObject, lights, shelves);
        }

        private void BuildExpansionLevel3(Transform parent, Transform productsGroup)
        {
            GameObject areaRoot = new GameObject("Expansion_Level3");
            areaRoot.transform.parent = parent;
            areaRoot.transform.localPosition = Vector3.zero;

            Transform contentGroup = CreateChildGroup("Content", areaRoot.transform);

            // Floor (10.5m, -0.1m, 5.0m) - Size: 7m x 8m x 0.2m (Area = 56 m²)
            CreateBox("Floor_Lvl3", new Vector3(10.5f, -0.1f, 5.0f), new Vector3(7.0f, 0.2f, 8.0f), _floorMaterial, contentGroup, addCollider: true);

            // Perimeter Walls
            CreateBox("Wall_Lvl3_North", new Vector3(10.5f, 1.75f, 9.0f), new Vector3(7.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl3_South", new Vector3(10.5f, 1.75f, 1.0f), new Vector3(7.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl3_East", new Vector3(14.0f, 1.75f, 5.0f), new Vector3(0.3f, 3.5f, 8.0f), _wallMaterial, contentGroup, addCollider: true);

            // Ceiling & Lighting
            CreateBox("Ceiling_Lvl3", new Vector3(10.5f, 3.55f, 5.0f), new Vector3(7.0f, 0.15f, 8.0f), _ceilingMaterial, contentGroup);

            List<GameObject> lights = new List<GameObject>();
            GameObject t1 = CreateBox("LED_Troffer_Lvl3_A", new Vector3(10.5f, 3.48f, 3.5f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            GameObject t2 = CreateBox("LED_Troffer_Lvl3_B", new Vector3(10.5f, 3.48f, 6.5f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            lights.Add(t1);
            lights.Add(t2);

            // Gate at Portal 2 (X: 7.0f, Z: 5.5f)
            Collider gateCol;
            NavMeshObstacle gateObs;
            GameObject gateObj = CreateExpansionGate("Gate_Level3", new Vector3(7.0f, 1.3f, 5.5f), new Vector3(0.12f, 2.6f, 2.5f), areaRoot.transform, out gateCol, out gateObs);

            // 3D Lock Indicator facing into base store (West)
            GameObject indicator = CreateExpansionIndicator("Indicator_Level3", new Vector3(6.82f, 1.6f, 5.5f), Quaternion.Euler(0f, -90f, 0f), 3, areaRoot.transform);

            // Shelves in Level 3 Expansion (Cookies & Candy)
            List<ShelfVisualController> shelves = new List<ShelfVisualController>();
            var s1 = CreateDoubleGondola("Expansion_Shelf_Cookies", new Vector3(10.5f, 0f, 3.5f), "CORRIDOR COOKIES", contentGroup, productsGroup, ProductType.Cookies, "prod_cookies");
            var s2 = CreateDoubleGondola("Expansion_Shelf_Candy", new Vector3(10.5f, 0f, 6.5f), "SWEET CANDY", contentGroup, productsGroup, ProductType.Candy, "prod_candy");
            if (s1 != null) shelves.Add(s1);
            if (s2 != null) shelves.Add(s2);

            var areaComp = areaRoot.AddComponent<MarketExpansionArea>();
            areaComp.Configure(3, "Busy Local Store", "East Pantry & Sweet Corridor", gateObj, gateCol, gateObs, indicator, contentGroup.gameObject, lights, shelves);
        }

        private void BuildExpansionLevel4(Transform parent, Transform productsGroup)
        {
            GameObject areaRoot = new GameObject("Expansion_Level4");
            areaRoot.transform.parent = parent;
            areaRoot.transform.localPosition = Vector3.zero;

            Transform contentGroup = CreateChildGroup("Content", areaRoot.transform);

            // Floor (-10.5m, -0.1m, 5.0m) - Size: 7m x 10m x 0.2m (Area = 70 m²)
            CreateBox("Floor_Lvl4", new Vector3(-10.5f, -0.1f, 5.0f), new Vector3(7.0f, 0.2f, 10.0f), _floorMaterial, contentGroup, addCollider: true);

            // Perimeter Walls
            CreateBox("Wall_Lvl4_North", new Vector3(-10.5f, 1.75f, 10.0f), new Vector3(7.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl4_South", new Vector3(-10.5f, 1.75f, 0.0f), new Vector3(7.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl4_West", new Vector3(-14.0f, 1.75f, 5.0f), new Vector3(0.3f, 3.5f, 10.0f), _wallMaterial, contentGroup, addCollider: true);

            // Ceiling & Lighting
            CreateBox("Ceiling_Lvl4", new Vector3(-10.5f, 3.55f, 5.0f), new Vector3(7.0f, 0.15f, 10.0f), _ceilingMaterial, contentGroup);

            List<GameObject> lights = new List<GameObject>();
            GameObject t1 = CreateBox("LED_Troffer_Lvl4_A", new Vector3(-10.5f, 3.48f, 3.0f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            GameObject t2 = CreateBox("LED_Troffer_Lvl4_B", new Vector3(-10.5f, 3.48f, 7.0f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            lights.Add(t1);
            lights.Add(t2);

            // Gate at Portal 3 (X: -7.0f, Z: 7.5f)
            Collider gateCol;
            NavMeshObstacle gateObs;
            GameObject gateObj = CreateExpansionGate("Gate_Level4", new Vector3(-7.0f, 1.3f, 7.5f), new Vector3(0.12f, 2.6f, 2.9f), areaRoot.transform, out gateCol, out gateObs);

            // 3D Lock Indicator facing into base store (East)
            GameObject indicator = CreateExpansionIndicator("Indicator_Level4", new Vector3(-6.82f, 1.6f, 7.5f), Quaternion.Euler(0f, 90f, 0f), 4, areaRoot.transform);

            // Shelves in Level 4 Expansion (Upright Commercial Chillers: Soda & Juice along West wall X: -13.45m)
            List<ShelfVisualController> shelves = new List<ShelfVisualController>();
            var s1 = CreateChillerUnit("Expansion_Cooler_Soda", new Vector3(-13.45f, 0f, 3.0f), "CHILLED SODAS II", contentGroup, productsGroup, ProductType.Soda, "prod_soda");
            var s2 = CreateChillerUnit("Expansion_Cooler_Juice", new Vector3(-13.45f, 0f, 7.0f), "FRESH JUICES II", contentGroup, productsGroup, ProductType.Juice, "prod_juice");
            if (s1 != null) shelves.Add(s1);
            if (s2 != null) shelves.Add(s2);

            var areaComp = areaRoot.AddComponent<MarketExpansionArea>();
            areaComp.Configure(4, "Popular Superette", "West Chiller Bay", gateObj, gateCol, gateObs, indicator, contentGroup.gameObject, lights, shelves);
        }

        private void BuildExpansionLevel5(Transform parent, Transform productsGroup)
        {
            GameObject areaRoot = new GameObject("Expansion_Level5");
            areaRoot.transform.parent = parent;
            areaRoot.transform.localPosition = Vector3.zero;

            Transform contentGroup = CreateChildGroup("Content", areaRoot.transform);

            // Floor (0.0m, -0.1m, 14.0m) - Size: 16m x 8m x 0.2m (Area = 128 m²)
            CreateBox("Floor_Lvl5", new Vector3(0.0f, -0.1f, 14.0f), new Vector3(16.0f, 0.2f, 8.0f), _floorMaterial, contentGroup, addCollider: true);

            // Perimeter Walls
            CreateBox("Wall_Lvl5_North", new Vector3(0.0f, 1.75f, 18.0f), new Vector3(16.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl5_East", new Vector3(8.0f, 1.75f, 14.0f), new Vector3(0.3f, 3.5f, 8.0f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl5_West", new Vector3(-8.0f, 1.75f, 14.0f), new Vector3(0.3f, 3.5f, 8.0f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl5_South_Left", new Vector3(-5.5f, 1.75f, 10.0f), new Vector3(5.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);
            CreateBox("Wall_Lvl5_South_Right", new Vector3(5.0f, 1.75f, 10.0f), new Vector3(6.0f, 3.5f, 0.3f), _wallMaterial, contentGroup, addCollider: true);

            // Ceiling & Lighting
            CreateBox("Ceiling_Lvl5", new Vector3(0.0f, 3.55f, 14.0f), new Vector3(16.0f, 0.15f, 8.0f), _ceilingMaterial, contentGroup);

            List<GameObject> lights = new List<GameObject>();
            GameObject t1 = CreateBox("LED_Troffer_Lvl5_A", new Vector3(-4.0f, 3.48f, 14.0f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            GameObject t2 = CreateBox("LED_Troffer_Lvl5_B", new Vector3(0.0f, 3.48f, 14.0f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            GameObject t3 = CreateBox("LED_Troffer_Lvl5_C", new Vector3(4.0f, 3.48f, 14.0f), new Vector3(1.4f, 0.05f, 2.6f), _lightFixtureMaterial, contentGroup);
            lights.Add(t1);
            lights.Add(t2);
            lights.Add(t3);

            // Gate at Portal 4 (X: -0.5f, Z: 10.0f)
            Collider gateCol;
            NavMeshObstacle gateObs;
            GameObject gateObj = CreateExpansionGate("Gate_Level5", new Vector3(-0.5f, 1.3f, 10.0f), new Vector3(4.9f, 2.6f, 0.12f), areaRoot.transform, out gateCol, out gateObs);

            // 3D Lock Indicator facing into base store (South)
            GameObject indicator = CreateExpansionIndicator("Indicator_Level5", new Vector3(-0.5f, 1.6f, 9.82f), Quaternion.Euler(0f, 180f, 0f), 5, areaRoot.transform);

            // Shelves in Level 5 Expansion (Grand Tycoon Shelves: Chocolate, Rice, Sauce)
            List<ShelfVisualController> shelves = new List<ShelfVisualController>();
            var s1 = CreateDoubleGondola("Expansion_Shelf_Chocolate", new Vector3(-4.0f, 0f, 14.0f), "GRAND CHOCOLATE", contentGroup, productsGroup, ProductType.Chocolate, "prod_chocolate");
            var s2 = CreateDoubleGondola("Expansion_Shelf_Rice", new Vector3(0.0f, 0f, 14.0f), "GRAND RICE", contentGroup, productsGroup, ProductType.Rice, "prod_rice");
            var s3 = CreateDoubleGondola("Expansion_Shelf_Sauce", new Vector3(4.0f, 0f, 14.0f), "GRAND SAUCE", contentGroup, productsGroup, ProductType.Sauce, "prod_sauce");
            if (s1 != null) shelves.Add(s1);
            if (s2 != null) shelves.Add(s2);
            if (s3 != null) shelves.Add(s3);

            var areaComp = areaRoot.AddComponent<MarketExpansionArea>();
            areaComp.Configure(5, "Grand Tycoon Mart", "Grand Rear Pavilion", gateObj, gateCol, gateObs, indicator, contentGroup.gameObject, lights, shelves);
        }

        private GameObject CreateExpansionGate(string name, Vector3 pos, Vector3 shutterScale, Transform parent, out Collider gateCol, out NavMeshObstacle gateObs)
        {
            GameObject gateRoot = new GameObject(name);
            gateRoot.transform.parent = parent;
            gateRoot.transform.position = pos;

            // Shutter slab
            GameObject shutter = CreateBox("Gate_Shutter", pos, shutterScale, _doorMetalMaterial, gateRoot.transform, addCollider: true);
            gateCol = shutter.GetComponent<Collider>();

            gateObs = shutter.AddComponent<NavMeshObstacle>();
            gateObs.size = shutterScale + new Vector3(0.2f, 0f, 0.2f);
            gateObs.carving = true;

            // Top housing
            Vector3 housingPos = pos + new Vector3(0f, shutterScale.y * 0.5f + 0.15f, 0f);
            Vector3 housingScale = new Vector3(shutterScale.x > shutterScale.z ? shutterScale.x + 0.2f : 0.35f, 0.35f, shutterScale.z > shutterScale.x ? shutterScale.z + 0.2f : 0.35f);
            CreateBox("Shutter_Housing", housingPos, housingScale, _chromeMaterial, gateRoot.transform);

            return shutter;
        }

        private GameObject CreateExpansionIndicator(string name, Vector3 pos, Quaternion rot, int targetLevel, Transform parent)
        {
            GameObject indicator = new GameObject(name);
            indicator.transform.parent = parent;
            indicator.transform.position = pos;
            indicator.transform.rotation = rot;

            // Backing plaque
            CreateBox("Frame", pos, new Vector3(1.6f, 0.95f, 0.05f), _shelfAccentMaterial, indicator.transform);
            CreateBox("Plate", pos + indicator.transform.forward * 0.02f, new Vector3(1.5f, 0.85f, 0.03f), _doorMetalMaterial, indicator.transform);

            // TextMesh
            GameObject textObj = new GameObject("Text_Display");
            textObj.transform.parent = indicator.transform;
            textObj.transform.position = pos + indicator.transform.forward * 0.045f;
            textObj.transform.rotation = rot * Quaternion.Euler(0, 180, 0);

            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.text = $"MARKET EXPANSION\n[LEVEL {targetLevel}]\nUPGRADE TO UNLOCK";
            tm.fontSize = 32;
            tm.characterSize = 0.038f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.white;
            tm.fontStyle = FontStyle.Bold;

            return indicator;
        }

        // ==========================================
        // PRODUCT PLACEMENT HELPERS
        // ==========================================
        public enum ProductType
        {
            Water,
            Milk,
            Soda,
            Chips,
            Chocolate,
            Canned,
            Juice,
            Yogurt,
            Cookies,
            Candy,
            Sauce,
            Rice
        }

        private void StockShelfRow(Vector3 trayCenter, ProductType type, string productId, Transform parent, List<GameObject> collector, int count = 4)
        {
            Vector3 size = GetProductSize(type);
            PrimitiveType primitive = GetProductPrimitive(type);
            Material mat = GetProductMaterial(type);
            ProductData pData = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;

            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) * 0.5f) * 0.28f;
                Vector3 itemPos = new Vector3(trayCenter.x + offset, trayCenter.y + size.y * 0.5f, trayCenter.z);
                GameObject item = CreateProductItem($"Prod_{type}_{i}", itemPos, size, primitive, mat, parent, pData);
                if (collector != null && item != null)
                {
                    collector.Add(item);
                }
            }
        }

        private void StockWallRow(Vector3 trayCenter, ProductType type, string productId, Transform parent, List<GameObject> collector, int count = 4)
        {
            Vector3 size = GetProductSize(type);
            PrimitiveType primitive = GetProductPrimitive(type);
            Material mat = GetProductMaterial(type);
            ProductData pData = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;

            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) * 0.5f) * 0.32f;
                Vector3 itemPos = new Vector3(trayCenter.x, trayCenter.y + size.y * 0.5f, trayCenter.z + offset);
                GameObject item = CreateProductItem($"WallProd_{type}_{i}", itemPos, size, primitive, mat, parent, pData);
                if (collector != null && item != null)
                {
                    collector.Add(item);
                }
            }
        }

        private void StockCoolerRow(Vector3 rackCenter, ProductType type, string productId, Transform parent, List<GameObject> collector, int count = 4)
        {
            Vector3 size = GetProductSize(type);
            PrimitiveType primitive = GetProductPrimitive(type);
            Material mat = GetProductMaterial(type);
            ProductData pData = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;

            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) * 0.5f) * 0.32f;
                Vector3 itemPos = new Vector3(rackCenter.x, rackCenter.y + size.y * 0.5f, rackCenter.z + offset);
                GameObject item = CreateProductItem($"ColdProd_{type}_{i}", itemPos, size, primitive, mat, parent, pData);
                if (collector != null && item != null)
                {
                    collector.Add(item);
                }
            }
        }

        private Vector3 GetProductSize(ProductType type)
        {
            switch (type)
            {
                case ProductType.Water: return new Vector3(0.10f, 0.28f, 0.10f);
                case ProductType.Milk: return new Vector3(0.13f, 0.26f, 0.13f);
                case ProductType.Soda: return new Vector3(0.11f, 0.19f, 0.11f);
                case ProductType.Chips: return new Vector3(0.18f, 0.28f, 0.08f);
                case ProductType.Chocolate: return new Vector3(0.14f, 0.04f, 0.22f);
                case ProductType.Canned: return new Vector3(0.14f, 0.16f, 0.14f);
                case ProductType.Juice: return new Vector3(0.12f, 0.28f, 0.12f);
                case ProductType.Yogurt: return new Vector3(0.16f, 0.10f, 0.16f);
                case ProductType.Cookies: return new Vector3(0.24f, 0.09f, 0.09f);
                case ProductType.Candy: return new Vector3(0.12f, 0.15f, 0.06f);
                case ProductType.Sauce: return new Vector3(0.13f, 0.20f, 0.13f);
                case ProductType.Rice: return new Vector3(0.17f, 0.22f, 0.12f);
                default: return new Vector3(0.15f, 0.25f, 0.15f);
            }
        }

        private PrimitiveType GetProductPrimitive(ProductType type)
        {
            switch (type)
            {
                case ProductType.Water:
                case ProductType.Soda:
                case ProductType.Canned:
                case ProductType.Yogurt:
                case ProductType.Sauce:
                    return PrimitiveType.Cylinder;

                case ProductType.Milk:
                case ProductType.Chips:
                case ProductType.Chocolate:
                case ProductType.Juice:
                case ProductType.Cookies:
                case ProductType.Candy:
                case ProductType.Rice:
                default:
                    return PrimitiveType.Cube;
            }
        }

        private Material GetProductMaterial(ProductType type)
        {
            switch (type)
            {
                case ProductType.Water: return _matProdWater ?? _productsAtlasMaterial;
                case ProductType.Milk: return _matProdMilk ?? _productsAtlasMaterial;
                case ProductType.Chips: return _matProdChips ?? _productsAtlasMaterial;
                case ProductType.Chocolate: return _matProdChocolate ?? _productsAtlasMaterial;
                case ProductType.Soda: return _matProdSoda ?? _productsAtlasMaterial;
                case ProductType.Canned: return _matProdCanned ?? _productsAtlasMaterial;
                case ProductType.Juice: return _matProdJuice ?? _productsAtlasMaterial;
                case ProductType.Yogurt: return _matProdYogurt ?? _productsAtlasMaterial;
                case ProductType.Cookies: return _matProdCookies ?? _productsAtlasMaterial;
                case ProductType.Candy: return _matProdCandy ?? _productsAtlasMaterial;
                case ProductType.Sauce: return _matProdSauce ?? _productsAtlasMaterial;
                case ProductType.Rice: return _matProdRice ?? _productsAtlasMaterial;
                default: return _productsAtlasMaterial;
            }
        }

        private GameObject CreateProductItem(string name, Vector3 pos, Vector3 scale, PrimitiveType primitive, Material mat, Transform parent, ProductData pData)
        {
            GameObject prod = CreateShape(name, pos, scale, primitive, mat, parent, addCollider: false);
            ProductVisual visual = prod.AddComponent<ProductVisual>();
            if (pData != null)
            {
                visual.Initialize(pData);
            }
            return prod;
        }

        private Transform CreateSubGroup(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        private Transform CreateChildGroup(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        private GameObject CreateBox(string name, Vector3 position, Vector3 localScale, Material mat, Transform parent, bool addCollider = false)
        {
            return CreateShape(name, position, localScale, PrimitiveType.Cube, mat, parent, addCollider);
        }

        private GameObject CreateShape(string name, Vector3 position, Vector3 localScale, PrimitiveType primitive, Material mat, Transform parent, bool addCollider = false)
        {
            GameObject go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.parent = parent;
            go.transform.position = position;
            go.transform.localScale = localScale;

            if (!addCollider)
            {
                Collider col = go.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
            }

            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null && mat != null)
            {
                rend.sharedMaterial = mat;
            }
            return go;
        }
    }
}
