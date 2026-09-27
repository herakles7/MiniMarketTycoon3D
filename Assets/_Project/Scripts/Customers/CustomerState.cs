namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Lifecycle states of a retail customer NPC.
    /// </summary>
    public enum CustomerState
    {
        Idle,
        Entering,
        Browsing,
        GoingToShelf,
        Shopping,
        GoingToCheckout,
        WaitingInQueue,
        CheckingOut,
        Leaving,
        Exited,
        Stuck
    }

    /// <summary>
    /// Stylized character visual variations.
    /// </summary>
    public enum CustomerVariationType
    {
        Customer_A, // Male, casual polo, khaki pants, brown hair
        Customer_B, // Female, blouse, navy jeans, dark hair
        Customer_C  // Casual urban, hoodie, dark trousers, blonde hair
    }
}
