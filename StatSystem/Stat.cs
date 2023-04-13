using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StatSystem
{
    public class Stat
    {
        protected StatDefinition m_Definition;
        public virtual Dice baseValue => m_Definition.baseValue;

        protected Dictionary<DiceType, int> m_FinalValue;
        public Dictionary<DiceType, int> finalValue => m_FinalValue;

        public int Intensity => m_FinalValue.Sum(pair => pair.Key == DiceType.Flat ? pair.Value : pair.Value * pair.Key.MaximalValue());

        public int Damage { get; private set; } = 0;
        public int MaxDamage => m_Definition.maxDamage;

        public int CurrentValue => Intensity - Damage;

        public int MaxValue => m_MaxDice.Sum(pair => pair.Key == DiceType.Flat ? pair.Value : pair.Value * pair.Key.MaximalValue());

        public event Action valueChanged;

        protected List<StatModifier> m_Modifiers = new List<StatModifier>();

        private Dictionary<DiceType, int> m_MaxDice;
        public Dictionary<DiceType, int> maxDice => m_MaxDice;

        public Stat(StatDefinition definition)
        {
            m_Definition = definition;
            CalculateFinalValue();
            CalculateMaxDice();
        }

        public void AddModifier(StatModifier modifier)
        {
            m_Modifiers.Add(modifier);
            CalculateFinalValue();
            CalculateMaxDice();
        }

        public void RemoveModifierFromSource(Object source)
        {
            m_Modifiers = m_Modifiers.Where(m => m.source.GetInstanceID() != source.GetInstanceID()).ToList();
            CalculateFinalValue();
            CalculateMaxDice();
        }

        public void ApplyDamage(int damage)
        {
            Damage = Mathf.Clamp(Damage + damage, 0, Intensity);
            CalculateMaxDice();
        }

        protected void CalculateFinalValue()
        {
            Dictionary<DiceType, int> diceTotals = new Dictionary<DiceType, int>();

            // Initialize the dictionary with the baseValue
            if (!diceTotals.ContainsKey(m_Definition.baseValue.DiceType))
            {
                diceTotals[m_Definition.baseValue.DiceType] = m_Definition.baseValue.DiceNumber;
            }
            else
            {
                diceTotals[m_Definition.baseValue.DiceType] += m_Definition.baseValue.DiceNumber;
            }

            m_Modifiers.Sort((x, y) => x.type.CompareTo(y.type));

            for (int i = 0; i < m_Modifiers.Count; i++)
            {
                StatModifier modifier = m_Modifiers[i];

                if (modifier.type == ModifierOperationType.Additive)
                {
                    if (!diceTotals.ContainsKey(modifier.dice.DiceType))
                    {
                        diceTotals[modifier.dice.DiceType] = modifier.dice.DiceNumber;
                    }
                    else
                    {
                        diceTotals[modifier.dice.DiceType] += modifier.dice.DiceNumber;
                    }
                }
            }

            if (!m_FinalValue.SequenceEqual(diceTotals))
            {
                m_FinalValue = diceTotals;
                valueChanged?.Invoke();
            }
        }

        protected void CalculateMaxDice()
        {
            int remainingDamage = MaxDamage;
            Dictionary<DiceType, int> maxDice = new Dictionary<DiceType, int>(finalValue);

            while (remainingDamage > 0 && maxDice.Count > 0)
            {
                DiceType smallestDiceType = GetSmallestNonFlatDice(maxDice.Keys);
                Dice smallestDice = new Dice { DiceNumber = 1, DiceType = smallestDiceType };
            if (maxDice[smallestDiceType] == 1)
            {
                maxDice.Remove(smallestDiceType);
            }
            else
            {
                maxDice[smallestDiceType] -= 1;
            }

            remainingDamage -= smallestDice.MaximalValue();
        }

        m_MaxDice = maxDice;
        valueChanged?.Invoke();
    }

    private DiceType GetSmallestNonFlatDice(IEnumerable<DiceType> diceTypes)
    {
        DiceType smallestDiceType = DiceType.Flat;
        foreach (DiceType diceType in diceTypes)
        {
            if (diceType == DiceType.Flat)
            {
                continue;
            }
            if (smallestDiceType == DiceType.Flat || diceType.MaximalValue() < smallestDiceType.MaximalValue())
            {
                smallestDiceType = diceType;
            }
        }
        return smallestDiceType;
    }
}
