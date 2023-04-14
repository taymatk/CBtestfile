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
            /*  m_Definition is an instance of the StatDefinition scriptable object */

        public virtual Dice baseValue => m_Definition.baseValue;
        public virtual int cap => m_Definition.cap;
        public virtual int floor => m_Definition.floor;
        public virtual int median => m_Definition.median;
            /*  We create copies of the key values Serialized through StatDefinition */
        
        protected List<StatModifier> m_Modifiers = new List<StatModifier>();
            /*  m_Modifiers will house modifiers of the baseValue brough in by weapons, armor, powers, etc. */
        
        protected Dictionary<DiceType, int> m_FinalValue;
        public Dictionary<DiceType, int> finalValue => m_FinalValue;
            /*  m_FinalValue will house the modified baseValue of the Stat. finalValue is it's public reference.  */

        public int Intensity => m_FinalValue.Sum(pair => pair.Key == DiceType.Flat ? pair.Value : pair.Value * (new Dice { DiceType = pair.Key }).MaximalValue);
            /*  Intensity is the whole number conversion of m_FinalValue.
                The .Sum() extension method finds the sum of the dictionary's parts.
                The next section "pair => pair.Key == Dice.Flat ? pair.Value" is checking to see if the Key (dicetype) in the pair (dicenumber,dicetype) is "Flat". If it is, it's taken at face value when being added to the Sum()
                Following that " : pair.Value * (new Dice { DiceType = pair.Key }).MaximalValue)" is multiplying the value (dicenumber) by the key's (dicetype) MaximalValue (which is defined in the Dice class)
            */

        public int Damage = 0;
        private int m_currentValue => Intensity - Damage;
        public int CurrentValue => m_currentValue;
            /*  Damage and MaxDamage will house the Stat 
            */



        public int MaxDamage = 0;
        private Dictionary<DiceType, int> m_MaxDice;
        public Dictionary<DiceType, int> maxDice => m_MaxDice;


        public int MaxValue => m_MaxDice.Sum(pair => pair.Key == DiceType.Flat ? pair.Value : pair.Value * (new Dice { DiceType = pair.Key }).MaximalValue);

        public event Action valueChanged;

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

                remainingDamage -= smallestDice.MaximalValue;
            }

    m_MaxDice = maxDice;
    valueChanged?.Invoke();
    }

        private DiceType GetSmallestNonFlatDice(IEnumerable<DiceType> diceTypes)
        {
            DiceType smallestDiceType = DiceType.Flat;
            int smallestMaxValue = int.MaxValue;
            foreach (DiceType diceType in diceTypes)
            {
                if (diceType == DiceType.Flat)
                {
                    continue;
                }
                Dice tempDice = new Dice { DiceType = diceType };
                int tempMaxValue = tempDice.MaximalValue;
                if (smallestDiceType == DiceType.Flat || tempMaxValue < smallestMaxValue)
                {
                    smallestDiceType = diceType;
                    smallestMaxValue = tempMaxValue;
                }
            }
            return smallestDiceType;
        }
    }
}
