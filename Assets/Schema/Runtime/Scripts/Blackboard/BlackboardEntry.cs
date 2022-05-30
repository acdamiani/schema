using System;
using UnityEngine;

namespace Schema.Internal
{
    /// <summary>
    /// ScriptableObject representation for BlackboardEntry
    /// </summary>
    [Serializable]
    public class BlackboardEntry : ScriptableObject
    {
        /// <summary>
        /// Description of this entry
        /// </summary>
        public string description { get { return m_description; } }
        [SerializeField] private string m_description;
        /// <summary>
        /// Type string for this entry
        /// </summary>
        public string typeString { get { return m_typeString; } }
        [SerializeField] private string m_typeString;
        /// <summary>
        /// Type of this entry
        /// </summary>
        public Type type
        {
            get
            {
                if (_type == null)
                    _type = Type.GetType(m_typeString);

                return _type;
            }
            set
            {
                m_typeString = value.AssemblyQualifiedName;
                _type = value;
            }
        }
        private Type _type;
        /// <summary>
        /// Blackboard that this entry is attached to
        /// </summary>
        public Blackboard blackboard { get { return m_blackboard; } internal set { m_blackboard = value; } }
        [SerializeField] private Blackboard m_blackboard;
    }
}