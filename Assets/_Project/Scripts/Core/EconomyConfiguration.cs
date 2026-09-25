using UnityEngine;

namespace MiniMarketTycoon.Core
{
    /// <summary>
    /// Configuration for game economy balancing and idle offline earnings.
    /// </summary>
    [CreateAssetMenu(fileName = "EconomyConfiguration", menuName = "MiniMarket/Configuration/Economy Configuration")]
    public class EconomyConfiguration : ScriptableObject
    {
        [Header("Starting Economy")]
        [SerializeField] private double _startingCash = 1000.0;

        [Header("Offline Earnings")]
        [SerializeField] private float _offlineEarningsMaxHours = 6.0f;
        [SerializeField] private float _offlineEarningsEfficiency = 0.5f;

        public double StartingCash => _startingCash;
        public float OfflineEarningsMaxHours => _offlineEarningsMaxHours;
        public float OfflineEarningsEfficiency => _offlineEarningsEfficiency;
    }
}
