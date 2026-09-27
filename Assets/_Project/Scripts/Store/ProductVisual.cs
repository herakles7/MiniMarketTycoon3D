using UnityEngine;

namespace MiniMarketTycoon.Store
{
    /// <summary>
    /// Visual representation of retail products placed on store shelves and refrigerators.
    /// Acts as the architectural bridge between 3D scene representation and ProductData metadata.
    /// </summary>
    [SelectionBase]
    public class ProductVisual : MonoBehaviour
    {
        [Header("Product Configuration")]
        [SerializeField] private ProductData _productData;
        [SerializeField] private MeshRenderer _meshRenderer;

        public ProductData ProductData => _productData;

        public void Initialize(ProductData data)
        {
            _productData = data;
            UpdateVisuals();
        }

        private void Awake()
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
        }

        public void UpdateVisuals()
        {
            if (_productData == null) return;
            gameObject.name = $"Product_{_productData.ID}";
        }
    }
}
