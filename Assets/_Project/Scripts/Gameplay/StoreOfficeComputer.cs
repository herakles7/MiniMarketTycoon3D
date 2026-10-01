using UnityEngine;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Manager's desktop terminal in the supermarket office.
    /// Provides access to wholesale supply ordering, store upgrades,
    /// staff hiring, and financial accounting.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StoreOfficeComputer : MonoBehaviour, IInteractable
    {
        [Header("Terminal Visuals")]
        [SerializeField] private Renderer _screenRenderer;
        [SerializeField] private Light _screenGlow;

        public string GetInteractionPrompt()
        {
            return "[E] Toptancı Bilgisayarı (Sipariş & Yönetim)";
        }

        public Vector3 GetPromptWorldPosition()
        {
            return transform.position + Vector3.up * 0.9f;
        }

        public bool CanInteract(PlayerManagerController player)
        {
            return true;
        }

        public void OnInteract(PlayerManagerController player)
        {
            if (UIManager.HasInstance)
            {
                UIManager.Instance.OpenRestock();
            }
        }

        public bool OnHoldInteract(PlayerManagerController player, float deltaTime) => false;

        public void OnFocusEnter(PlayerManagerController player)
        {
            if (_screenGlow != null) _screenGlow.intensity = 2.5f;
        }

        public void OnFocusExit(PlayerManagerController player)
        {
            if (_screenGlow != null) _screenGlow.intensity = 1.0f;
        }
    }
}
