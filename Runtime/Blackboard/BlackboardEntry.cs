using System;
using UnityEngine;

namespace Schema.Internal
{
    /// <summary>
    ///     ScriptableObject representation for BlackboardEntry
    /// </summary>
    [Serializable]
    public class BlackboardEntry : ScriptableObject
    {
        [SerializeField] private string m_description;
        [SerializeField] private string m_typeString;
        [SerializeField] private Blackboard m_blackboard;
        private Type _type;
        private string lastTypeString = "";

        /// <summary>
        ///     Description of this entry
        /// </summary>
        public string description => m_description;

        /// <summary>
        ///     Type string for this entry
        /// </summary>
        public string typeString => m_typeString;

        /// <summary>
        ///     Type of this entry
        /// </summary>
        public Type type
        {
            get
            {
                if (_type == null || !lastTypeString.Equals(m_typeString))
                {
                    _type = Type.GetType(m_typeString);
                    lastTypeString = m_typeString;
                }

                return _type;
            }
            set
            {
                m_typeString = value.AssemblyQualifiedName;
                _type = value;
            }
        }

        /// <summary>
        ///     Blackboard that this entry is attached to
        /// </summary>
        public Blackboard blackboard
        {
            get => m_blackboard;
            internal set => m_blackboard = value;
        }
    }
}