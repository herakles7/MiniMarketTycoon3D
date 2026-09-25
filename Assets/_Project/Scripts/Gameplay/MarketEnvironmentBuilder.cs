using UnityEngine;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Builds and verifies the realistic 3D market store environment with real-world architectural proportions.
    /// Real-world scales:
    /// - Human height reference: 1.75m - 1.80m
    /// - Shelf height: 1.80m, depth: 0.60m, width: 1.40m
    /// - Cashier counter: height: 0.90m, length: 2.20m, depth: 0.80m
    /// - Wall height: 3.50m
    /// - Market floor: 14m x 20m
    /// </summary>
    [ExecuteAlways]
    public class MarketEnvironmentBuilder : MonoBehaviour
    {
        [Header("Materials (Optional / Auto-Fallback)")]
        [SerializeField] private Material _floorMaterial;
        [SerializeField] private Material _wallMaterial;
        [SerializeField] private Material _shelfMaterial;
        [SerializeField] private Material _counterMaterial;
        [SerializeField] private Material _glassMaterial;
        [SerializeField] private Material _sidewalkMaterial;

        [Header("Generation Control")]
        [SerializeField] private bool _rebuildOnStart = false;

        private void Start()
        {
            if (_rebuildOnStart || transform.childCount == 0)
            {
                BuildEnvironment();
            }
        }

        [ContextMenu("Rebuild Environment")]
        public void BuildEnvironment()
        {
            // Clear existing children if rebuilding
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            // 1. Interior Store Floor (14m x 20m)
            GameObject floor = CreatePrimitive("Store_Floor", PrimitiveType.Cube, transform);
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(14f, 0.2f, 20f);
            ApplyMaterial(floor, _floorMaterial, new Color(0.88f, 0.88f, 0.90f));

            // 2. Exterior Sidewalk / Entry Plaza (18m x 6m)
            GameObject sidewalk = CreatePrimitive("Exterior_Sidewalk", PrimitiveType.Cube, transform);
            sidewalk.transform.position = new Vector3(0f, -0.1f, -13f);
            sidewalk.transform.localScale = new Vector3(18f, 0.2f, 6f);
            ApplyMaterial(sidewalk, _sidewalkMaterial, new Color(0.65f, 0.65f, 0.68f));

            // 3. Walls (Height: 3.5m, Thickness: 0.3m)
            // Back Wall
            GameObject backWall = CreatePrimitive("Wall_Back", PrimitiveType.Cube, transform);
            backWall.transform.position = new Vector3(0f, 1.75f, 10f);
            backWall.transform.localScale = new Vector3(14f, 3.5f, 0.3f);
            ApplyMaterial(backWall, _wallMaterial, new Color(0.92f, 0.93f, 0.95f));

            // Left Wall
            GameObject leftWall = CreatePrimitive("Wall_Left", PrimitiveType.Cube, transform);
            leftWall.transform.position = new Vector3(-7f, 1.75f, 0f);
            leftWall.transform.localScale = new Vector3(0.3f, 3.5f, 20f);
            ApplyMaterial(leftWall, _wallMaterial, new Color(0.92f, 0.93f, 0.95f));

            // Right Wall
            GameObject rightWall = CreatePrimitive("Wall_Right", PrimitiveType.Cube, transform);
            rightWall.transform.position = new Vector3(7f, 1.75f, 0f);
            rightWall.transform.localScale = new Vector3(0.3f, 3.5f, 20f);
            ApplyMaterial(rightWall, _wallMaterial, new Color(0.92f, 0.93f, 0.95f));

            // Front Walls with 4m Entrance Gap in center
            GameObject frontWallLeft = CreatePrimitive("Wall_Front_Left", PrimitiveType.Cube, transform);
            frontWallLeft.transform.position = new Vector3(-4.5f, 1.75f, -10f);
            frontWallLeft.transform.localScale = new Vector3(5f, 3.5f, 0.3f);
            ApplyMaterial(frontWallLeft, _wallMaterial, new Color(0.92f, 0.93f, 0.95f));

            GameObject frontWallRight = CreatePrimitive("Wall_Front_Right", PrimitiveType.Cube, transform);
            frontWallRight.transform.position = new Vector3(4.5f, 1.75f, -10f);
            frontWallRight.transform.localScale = new Vector3(5f, 3.5f, 0.3f);
            ApplyMaterial(frontWallRight, _wallMaterial, new Color(0.92f, 0.93f, 0.95f));

            // Entrance Glass Transom Above Doors
            GameObject entranceTransom = CreatePrimitive("Entrance_Transom", PrimitiveType.Cube, transform);
            entranceTransom.transform.position = new Vector3(0f, 2.95f, -10f);
            entranceTransom.transform.localScale = new Vector3(4f, 1.1f, 0.15f);
            ApplyMaterial(entranceTransom, _glassMaterial, new Color(0.6f, 0.8f, 0.95f, 0.4f));

            // Glass Sliding Doors (2.4m height, realistic automatic doors)
            GameObject doorLeft = CreatePrimitive("Glass_Door_Left", PrimitiveType.Cube, transform);
            doorLeft.transform.position = new Vector3(-1f, 1.2f, -10f);
            doorLeft.transform.localScale = new Vector3(1.9f, 2.4f, 0.08f);
            ApplyMaterial(doorLeft, _glassMaterial, new Color(0.7f, 0.85f, 0.95f, 0.35f));

            GameObject doorRight = CreatePrimitive("Glass_Door_Right", PrimitiveType.Cube, transform);
            doorRight.transform.position = new Vector3(1f, 1.2f, -10f);
            doorRight.transform.localScale = new Vector3(1.9f, 2.4f, 0.08f);
            ApplyMaterial(doorRight, _glassMaterial, new Color(0.7f, 0.85f, 0.95f, 0.35f));

            // 4. Ceiling Beams / Modern Industrial Fixtures
            GameObject beam1 = CreatePrimitive("Ceiling_Beam_1", PrimitiveType.Cube, transform);
            beam1.transform.position = new Vector3(0f, 3.45f, -4f);
            beam1.transform.localScale = new Vector3(13.8f, 0.15f, 0.4f);
            ApplyMaterial(beam1, _wallMaterial, new Color(0.3f, 0.32f, 0.35f));

            GameObject beam2 = CreatePrimitive("Ceiling_Beam_2", PrimitiveType.Cube, transform);
            beam2.transform.position = new Vector3(0f, 3.45f, 4f);
            beam2.transform.localScale = new Vector3(13.8f, 0.15f, 0.4f);
            ApplyMaterial(beam2, _wallMaterial, new Color(0.3f, 0.32f, 0.35f));

            // 5. Realistic Shelf Units (Height: 1.8m, Width: 1.4m, Depth: 0.6m)
            CreateShelfUnit("Shelf_Aisle_Left", new Vector3(-3.2f, 0f, 2f));
            CreateShelfUnit("Shelf_Aisle_Right", new Vector3(3.2f, 0f, 2f));
            CreateShelfUnit("Shelf_Aisle_Back", new Vector3(0f, 0f, 6.5f));

            // 6. Cashier Counter Placeholder
            CreateCashierCounter("Cashier_Counter", new Vector3(-2.8f, 0f, -6.5f));

            // 7. Customer Waypoint Points
            GameObject spawnPoint = new GameObject("Customer_Spawn_Point");
            spawnPoint.transform.parent = transform;
            spawnPoint.transform.position = new Vector3(0f, 0f, -14f);

            GameObject entrancePoint = new GameObject("Customer_Entrance_Point");
            entrancePoint.transform.parent = transform;
            entrancePoint.transform.position = new Vector3(0f, 0f, -8f);

            Debug.Log("[MarketEnvironmentBuilder] Realistic market environment successfully constructed!");
        }

        private void CreateShelfUnit(string name, Vector3 position)
        {
            GameObject shelfRoot = new GameObject(name);
            shelfRoot.transform.parent = transform;
            shelfRoot.transform.position = position;

            // Upright backboard
            GameObject backboard = CreatePrimitive("Backboard", PrimitiveType.Cube, shelfRoot.transform);
            backboard.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            backboard.transform.localScale = new Vector3(1.4f, 1.8f, 0.05f);
            ApplyMaterial(backboard, _shelfMaterial, new Color(0.35f, 0.4f, 0.45f));

            // Tier 1 (Base shelf)
            GameObject tier1 = CreatePrimitive("Tier_Base", PrimitiveType.Cube, shelfRoot.transform);
            tier1.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            tier1.transform.localScale = new Vector3(1.4f, 0.08f, 0.7f);
            ApplyMaterial(tier1, _shelfMaterial, new Color(0.85f, 0.85f, 0.88f));

            // Tier 2 (Middle shelf)
            GameObject tier2 = CreatePrimitive("Tier_Middle", PrimitiveType.Cube, shelfRoot.transform);
            tier2.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            tier2.transform.localScale = new Vector3(1.4f, 0.06f, 0.6f);
            ApplyMaterial(tier2, _shelfMaterial, new Color(0.85f, 0.85f, 0.88f));

            // Tier 3 (Top shelf)
            GameObject tier3 = CreatePrimitive("Tier_Top", PrimitiveType.Cube, shelfRoot.transform);
            tier3.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            tier3.transform.localScale = new Vector3(1.4f, 0.06f, 0.5f);
            ApplyMaterial(tier3, _shelfMaterial, new Color(0.85f, 0.85f, 0.88f));
        }

        private void CreateCashierCounter(string name, Vector3 position)
        {
            GameObject counterRoot = new GameObject(name);
            counterRoot.transform.parent = transform;
            counterRoot.transform.position = position;

            // Main counter body: Length 2.2m, Height 0.9m, Width 0.8m
            GameObject body = CreatePrimitive("Counter_Body", PrimitiveType.Cube, counterRoot.transform);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localScale = new Vector3(2.2f, 0.9f, 0.8f);
            ApplyMaterial(body, _counterMaterial, new Color(0.28f, 0.32f, 0.38f));

            // Conveyor belt placeholder on top
            GameObject conveyor = CreatePrimitive("Conveyor_Belt", PrimitiveType.Cube, counterRoot.transform);
            conveyor.transform.localPosition = new Vector3(-0.2f, 0.92f, 0f);
            conveyor.transform.localScale = new Vector3(1.6f, 0.04f, 0.65f);
            ApplyMaterial(conveyor, null, new Color(0.15f, 0.15f, 0.16f));

            // Cash register terminal box
            GameObject register = CreatePrimitive("Cash_Register_Terminal", PrimitiveType.Cube, counterRoot.transform);
            register.transform.localPosition = new Vector3(0.8f, 1.05f, 0f);
            register.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
            ApplyMaterial(register, null, new Color(0.2f, 0.22f, 0.25f));
        }

        private GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.parent = parent;
            return go;
        }

        private void ApplyMaterial(GameObject obj, Material mat, Color fallbackColor)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null) return;

            if (mat != null)
            {
                rend.sharedMaterial = mat;
            }
            else
            {
                // Assign a clean default standard/URP shader material
                Material runtimeMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                runtimeMat.color = fallbackColor;
                rend.sharedMaterial = runtimeMat;
            }
        }
    }
}
