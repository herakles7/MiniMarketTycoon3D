using UnityEngine;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Base interface for any object in the supermarket that the player can interact with
    /// (shelves, cash register, wholesale PC, delivery boxes, trash dumpster, etc.).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Display text shown on the player's 3D floating interaction prompt (e.g. "[E] Restock Shelf", "[E] Pick Up Box").
        /// </summary>
        string GetInteractionPrompt();

        /// <summary>
        /// World position where the floating interaction prompt should float.
        /// </summary>
        Vector3 GetPromptWorldPosition();

        /// <summary>
        /// Whether the object is currently available for interaction.
        /// </summary>
        bool CanInteract(PlayerManagerController player);

        /// <summary>
        /// Executes primary interaction (short press [E] or tap).
        /// </summary>
        void OnInteract(PlayerManagerController player);

        /// <summary>
        /// Optional hold interaction for continuous actions (like continuous stocking).
        /// Returns true if interaction is complete/consumed.
        /// </summary>
        bool OnHoldInteract(PlayerManagerController player, float deltaTime);

        /// <summary>
        /// Called when the player enters the interaction range of this object.
        /// </summary>
        void OnFocusEnter(PlayerManagerController player);

        /// <summary>
        /// Called when the player leaves the interaction range of this object.
        /// </summary>
        void OnFocusExit(PlayerManagerController player);
    }
}
