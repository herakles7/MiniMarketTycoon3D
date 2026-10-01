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
        Customer_A, // Human Male
        Customer_B, // Human Female
        Customer_C, // Skater Male
        Customer_D, // Skater Female
        Customer_E, // Criminal Male
        Customer_F, // Cyborg Female
        Customer_G, // Business Male (Suit & Tie)
        Customer_H, // Business Female (Blazer & Smile)
        Customer_I, // Grandpa (Cardigan & Glasses)
        Customer_J, // Student (Hoodie & Headphones)
        Customer_K, // Survivor Female
        Customer_L  // Survivor Male
    }
}
