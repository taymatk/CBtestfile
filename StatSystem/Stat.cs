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

        public int Damage { get; set; }
        public int MaxDamage { get; set; }

        public int CurrentValue => Mathf.Max(Intensity - Damage, 0);

        protected Dictionary<DiceType, int> m_MaxDice;
        public Dictionary<DiceType, int> maxDice => m_MaxDice;

        public int MaxValue => m_MaxDice.Sum(pair => pair.Value * pair.Key.MaximalValue());

        public event Action valueChanged;

        protected List<StatModifier> m_Modifiers = new List<StatModifier>();

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
            Dictionary<DiceType, int> maxDice = new Dictionary<DiceType, int>(m_FinalValue);

            // Subtract dice from maxDice based on MaxDamage
            int remainingDamage = MaxDamage;
            while (remainingDamage > 0)
            {
                DiceType smallestDice = GetSmallestNonFlatDice(maxDice.Keys);
                if (smallestDice == DiceType.Flat)
                {
                    // If there are no non-flat dice, break out of the loop
                    break;
                }
                int maxDiceCount = maxDice[smallestDice];
                int damageToDice = Mathf.Min(remainingDamage -= Mathf.Min(remainingDamage, maxDiceCount * smallestDice.MaximalValue()));
                int newMaxDiceCount = maxDiceCount - (remainingDamage / smallestDice.MaximalValue());
                maxDice[smallestDice] = newMaxDiceCount;
            }

            m_MaxDice = maxDice;
        }

        private DiceType GetSmallestNonFlatDice(IEnumerable<DiceType> diceTypes)
        {
            return diceTypes.Where(type => type != DiceType.Flat).OrderBy(type => type.MaximalValue()).FirstOrDefault();
        }
    }
}

