using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Utilities;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Handles physical delivery of wholesale inventory boxes to the supermarket.
    /// When the player orders supplies from the manager PC or wholesale terminal,
    /// delivery boxes arrive at the designated loading dock / entrance drop zone.
    /// </summary>
    public class DeliveryService : MonoBehaviourSingleton<DeliveryService>
    {
        [Header("Delivery Drop Zone")]
        [SerializeField] private Transform _dropZoneTransform;
        [SerializeField] private Vector3 _defaultDropPosition = new Vector3(4.2f, 0.16f, -11.5f);
        [SerializeField] private float _dropSpread = 0.35f;

        [Header("Prefabs & Materials")]
        [SerializeField] private GameObject _boxPrefab;
        [SerializeField] private Material _cardboardMaterial;

        private readonly List<ProductBoxController> _activeBoxes = new List<ProductBoxController>();

        public IReadOnlyList<ProductBoxController> ActiveBoxes => _activeBoxes;
        public Vector3 DropPosition => _dropZoneTransform != null ? _dropZoneTransform.position : _defaultDropPosition;

        protected override void OnInitialized()
        {
            EnsureCardboardMaterial();
            EnsureDeliveryPallet();
        }

        private void EnsureCardboardMaterial()
        {
            if (_cardboardMaterial == null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                _cardboardMaterial = new Material(s);
                _cardboardMaterial.name = "Mat_DeliveryCardboard";
                _cardboardMaterial.color = new Color(0.74f, 0.58f, 0.40f); // Realistic kraft cardboard brown
                _cardboardMaterial.SetFloat("_Smoothness", 0.12f);
                _cardboardMaterial.SetFloat("_Metallic", 0.0f);
            }
        }

        private void EnsureDeliveryPallet()
        {
            GameObject existingPallet = GameObject.Find("Delivery_Shipping_Pallet");
            if (existingPallet == null)
            {
                existingPallet = new GameObject("Delivery_Shipping_Pallet");
                existingPallet.transform.position = new Vector3(4.2f, 0.06f, -11.5f);

                // Wooden Pallet Base
                GameObject basePlank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                basePlank.name = "Pallet_Body";
                basePlank.transform.SetParent(existingPallet.transform, false);
                basePlank.transform.localScale = new Vector3(1.4f, 0.12f, 1.2f);
                basePlank.transform.localPosition = Vector3.zero;

                var rend = basePlank.GetComponent<Renderer>();
                if (rend != null)
                {
                    Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    Material woodMat = new Material(s);
                    woodMat.color = new Color(0.55f, 0.42f, 0.28f); // Wood plank timber
                    woodMat.SetFloat("_Smoothness", 0.10f);
                    rend.sharedMaterial = woodMat;
                }

                // Add Drop Zone Marker Text
                GameObject marker = new GameObject("DropZone_Marker");
                marker.transform.SetParent(existingPallet.transform, false);
                marker.transform.localPosition = new Vector3(0f, 0.065f, 0f);
                marker.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var tm = marker.AddComponent<TextMesh>();
                tm.text = "MAL KABUL / TESLİMAT";
                tm.alignment = TextAlignment.Center;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.characterSize = 0.03f;
                tm.fontSize = 24;
                tm.color = new Color(0.95f, 0.85f, 0.4f, 0.9f);
            }
        }

        /// <summary>
        /// Orders a batch of products and spawns a physical box at the delivery drop zone.
        /// </summary>
        public ProductBoxController DeliverBox(string productId, int count = 10, bool instant = false)
        {
            EnsureCardboardMaterial();
            EnsureDeliveryPallet();

            Vector3 spawnPos = DropPosition + new Vector3(
                Random.Range(-_dropSpread, _dropSpread),
                0.2f,
                Random.Range(-_dropSpread, _dropSpread)
            );

            GameObject boxGo = CreateBoxGameObject(spawnPos);
            ProductBoxController boxController = boxGo.GetComponent<ProductBoxController>();
            boxController.Initialize(productId, count);

            _activeBoxes.Add(boxController);

            if (FloatingFeedbackManager.HasInstance)
            {
                string prodName = productId.Replace("prod_", "").ToUpper();
                FloatingFeedbackManager.Instance.ShowMoodFeedback(
                    spawnPos + Vector3.up * 0.8f,
                    $"📦 TESLİMAT: {count}x {prodName}",
                    new Color(1f, 0.82f, 0.2f, 1f));
            }

            return boxController;
        }

        public GameObject CreateBoxGameObject(Vector3 worldPos)
        {
            // Create a clean cardboard box model
            GameObject root = new GameObject("Delivery_Box");
            root.transform.position = worldPos;
            root.transform.rotation = Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f);

            // Realistic grocery delivery carton dimensions (width 0.44m, height 0.28m, depth 0.34m)
            Vector3 boxDimensions = new Vector3(0.44f, 0.28f, 0.34f);

            // Cube body
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "BoxMesh";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = boxDimensions;
            visual.transform.localPosition = new Vector3(0f, boxDimensions.y * 0.5f, 0f);

            var rend = visual.GetComponent<Renderer>();
            if (rend != null && _cardboardMaterial != null)
            {
                rend.sharedMaterial = _cardboardMaterial;
            }

            // Remove visual collider, add box collider to root
            Collider visualCol = visual.GetComponent<Collider>();
            if (visualCol != null) Destroy(visualCol);

            BoxCollider rootCol = root.AddComponent<BoxCollider>();
            rootCol.size = boxDimensions;
            rootCol.center = new Vector3(0f, boxDimensions.y * 0.5f, 0f);

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 2.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // ProductBoxController
            ProductBoxController pbc = root.AddComponent<ProductBoxController>();

            return root;
        }
    }
}
