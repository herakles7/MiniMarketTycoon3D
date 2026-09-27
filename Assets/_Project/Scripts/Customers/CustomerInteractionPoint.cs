using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Defined standing location in front of a shelf, refrigerator, or checkout counter.
    /// Ensures customers stand in aisle walkways and face the merchandise naturally without clipping inside geometry.
    /// </summary>
    [SelectionBase]
    public class CustomerInteractionPoint : MonoBehaviour
    {
        [Header("Interaction Configuration")]
        [Tooltip("Identifier for the product category or shelf type.")]
        [SerializeField] private string _category = "Grocery";

        [Tooltip("The direction the customer should face while interacting with this spot.")]
        [SerializeField] private Vector3 _customFacingDirection = Vector3.forward;

        [SerializeField] private bool _useTransformForward = true;

        private bool _isOccupied;

        public string Category => _category;
        public bool IsOccupied => _isOccupied;

        public Vector3 Position => transform.position;
        public Vector3 FacingDirection => _useTransformForward ? transform.forward : _customFacingDirection.normalized;

        public void SetOccupied(bool occupied)
        {
            _isOccupied = occupied;
        }

        public void Initialize(string category, Vector3 facing)
        {
            _category = category;
            _customFacingDirection = facing;
            _useTransformForward = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _isOccupied ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, FacingDirection * 0.7f);
        }
#endif
    }
}
