using UnityEngine;
using System.Collections.Generic;

namespace StatSystem
{
    public enum DiceType
    {
        D2, D4, D6, D8, D10, D12, D20, D100, Flat
    }

    public class Dice : ScriptableObject
    {
        // Serialized fields
        [SerializeField] private int m_DiceNumber;
        [SerializeField] private DiceType m_DiceType;

        // Public properties
        public int DiceNumber
        {
            get => m_DiceNumber;
            set => m_DiceNumber = value;
        }
        public DiceType DiceType
        {
            get => m_DiceType;
            set => m_DiceType = value;
        }
        public int MaximalValue
        {
            get
            {
                switch (m_DiceType)
                {
                    case DiceType.D2:
                        return 2;
                    case DiceType.D4:
                        return 4;
                    case DiceType.D6:
                        return 6;
                    case DiceType.D8:
                        return 8;
                    case DiceType.D10:
                        return 10;
                    case DiceType.D12:
                        return 12;
                    case DiceType.D20:
                        return 20;
                    case DiceType.D100:
                        return 100;
                    case DiceType.Flat:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        public void SetDiceProperties(Dictionary<DiceType, int> diceTotals)
        {
            int maxValue = 0;
            DiceType maxDiceType = DiceType.Flat;

            foreach (var pair in diceTotals)
            {
                if (pair.Value * pair.Key.MaximalValue() > maxValue)
                {
                    maxValue = pair.Value * pair.Key.MaximalValue();
                    maxDiceType = pair.Key;
                }
            }

            DiceNumber = maxValue / maxDiceType.MaximalValue();
            DiceType = maxDiceType;
        }
    }
}
