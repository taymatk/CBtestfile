using UnityEngine;

namespace StatSystem
{
    public class StatDefinition : ScriptableObject
    {
        // Serialized fields
        [SerializeField] private Dice m_StartValue;
        [SerializeField] private int m_Cap;
        [SerializeField] private int m_Median;
        [SerializeField] private int m_Floor;

        // Public properties with copies of the private variables
        public Dice baseValue => m_StartValue;
        public int cap => m_Cap;
        public int median => m_Median;
        public int floor => m_Floor;
    }
}