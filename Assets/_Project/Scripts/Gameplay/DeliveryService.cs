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
        [SerializeField] private Vector3 _defaultDropPosition = new Vector3(2.5f, 0.1f, -5.5f);
        [SerializeField] private float _dropSpread = 0.65f;

        [Header("Prefabs & Materials")]
        [SerializeField] private GameObject _boxPrefab;
        [SerializeField] private Material _cardboardMaterial;

        private readonly List<ProductBoxController> _activeBoxes = new List<ProductBoxController>();

        public IReadOnlyList<ProductBoxController> ActiveBoxes => _activeBoxes;
        public Vector3 DropPosition => _dropZoneTransform != null ? _dropZoneTransform.position : _defaultDropPosition;

        protected override void OnInitialized()
        {
            EnsureCardboardMaterial();
        }

        private void EnsureCardboardMaterial()
        {
            if (_cardboardMaterial == null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                _cardboardMaterial = new Material(s);
                _cardboardMaterial.name = "Mat_DeliveryCardboard";
                _cardboardMaterial.color = new Color(0.76f, 0.60f, 0.42f); // Realistic kraft cardboard brown
                _cardboardMaterial.SetFloat("_Smoothness", 0.15f);
                _cardboardMaterial.SetFloat("_Metallic", 0.0f);
            }
        }

        /// <summary>
        /// Orders a batch of products and spawns a physical box at the delivery drop zone.
        /// </summary>
        public ProductBoxController DeliverBox(string productId, int count = 10, bool instant = false)
        {
            EnsureCardboardMaterial();

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
            root.transform.rotation = Quaternion.Euler(0f, Random.Range(-25f, 25f), 0f);

            // Cube body
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "BoxMesh";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(0.55f, 0.42f, 0.45f);
            visual.transform.localPosition = new Vector3(0f, 0.21f, 0f);

            var rend = visual.GetComponent<Renderer>();
            if (rend != null && _cardboardMaterial != null)
            {
                rend.sharedMaterial = _cardboardMaterial;
            }

            // Remove visual collider, add box collider to root
            Collider visualCol = visual.GetComponent<Collider>();
            if (visualCol != null) Destroy(visualCol);

            BoxCollider rootCol = root.AddComponent<BoxCollider>();
            rootCol.size = new Vector3(0.55f, 0.42f, 0.45f);
            rootCol.center = new Vector3(0f, 0.21f, 0f);

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 3.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // ProductBoxController
            ProductBoxController pbc = root.AddComponent<ProductBoxController>();

            return root;
        }
    }
}
