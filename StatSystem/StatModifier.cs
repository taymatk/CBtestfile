using UnityEngine;

namespace StatSystem
{
    public enum ModifierOperationType
    {
        Additive,
        Multiplicative,
        Override
    }

    public class StatModifier
    {
        public Object source { get; set; }
        public Dice dice { get; set; }
        public ModifierOperationType type { get; set; }
    }
}