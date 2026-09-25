using UnityEngine;

namespace MiniMarketTycoon.Economy
{
    /// <summary>
    /// ScriptableObject defining store, worker, or shelf upgrade properties.
    /// </summary>
    [CreateAssetMenu(fileName = "NewUpgradeData", menuName = "MiniMarket/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;

        [Header("Upgrade Properties")]
        [SerializeField] private double _cost = 100.0;
        [SerializeField] private int _level = 1;
        [SerializeField] private int _maxLevel = 10;
        [SerializeField] private float _multiplier = 1.25f;

        public string ID => _id;
        public string DisplayName => _displayName;
        public double Cost => _cost;
        public int Level => _level;
        public int MaxLevel => _maxLevel;
        public float Multiplier => _multiplier;

        public double CalculateCostForLevel(int targetLevel)
        {
            return _cost * System.Math.Pow(_multiplier, targetLevel - 1);
        }
    }
}
