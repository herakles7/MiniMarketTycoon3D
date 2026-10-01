using UnityEngine;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Recycling and trash dumpster located outside the supermarket.
    /// The player can throw away empty delivery boxes here to keep the store clean,
    /// earning a small eco-recycling cash incentive.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TrashDumpsterController : MonoBehaviour, IInteractable
    {
        [Header("Recycle Settings")]
        [SerializeField] private double _recyclingReward = 1.00;
        [SerializeField] private ParticleSystem _trashVFX;

        public string GetInteractionPrompt()
        {
            var player = PlayerManagerController.Instance;
            if (player != null && player.IsCarryingBox)
            {
                if (player.CarriedItem is ProductBoxController box)
                {
                    if (box.IsEmpty)
                    {
                        return $"[E] Boş Koliyi Çöpe At (+${_recyclingReward:F2} Geri Dönüşüm)";
                    }
                    return "Koli Dolu! (Önce rafları doldur)";
                }
                return "[E] Çöpe At";
            }

            return "Geri Dönüşüm Konteyneri";
        }

        public Vector3 GetPromptWorldPosition()
        {
            return transform.position + Vector3.up * 1.2f;
        }

        public bool CanInteract(PlayerManagerController player)
        {
            return player != null && player.IsCarryingBox;
        }

        public void OnInteract(PlayerManagerController player)
        {
            if (player == null || !player.IsCarryingBox) return;

            var carried = player.DropCarriedItem();
            if (carried != null)
            {
                if (carried is ProductBoxController box && box.IsEmpty)
                {
                    if (CurrencyManager.HasInstance)
                    {
                        CurrencyManager.Instance.AddCash(_recyclingReward);
                    }

                    if (FloatingFeedbackManager.HasInstance)
                    {
                        FloatingFeedbackManager.Instance.ShowMoodFeedback(
                            transform.position + Vector3.up * 1.4f,
                            $"♻ +${_recyclingReward:F2} GERİ DÖNÜŞÜM",
                            new Color(0.2f, 0.95f, 0.4f, 1f));
                    }
                }

                if (_trashVFX != null)
                {
                    _trashVFX.Play();
                }

                // Destroy physical box object
                if (carried is Component comp)
                {
                    Destroy(comp.gameObject);
                }
            }
        }

        public bool OnHoldInteract(PlayerManagerController player, float deltaTime) => false;
        public void OnFocusEnter(PlayerManagerController player) { }
        public void OnFocusExit(PlayerManagerController player) { }
    }
}
