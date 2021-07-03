using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class SerializableDictionaryExample : MonoBehaviour {
    // The dictionaries can be accessed throught a property
    [SerializeField] private StringStringDictionary m_stringStringDictionary;

    public ObjectColorDictionary m_objectColorDictionary;
    public StringColorArrayDictionary m_objectColorArrayDictionary;

    public IDictionary<string, string> StringStringDictionary {
        get => m_stringStringDictionary;
        set => m_stringStringDictionary.CopyFrom(value);
    }

    private void Reset() {
        // access by property
        StringStringDictionary = new Dictionary<string, string>
            {{"first key", "value A"}, {"second key", "value B"}, {"third key", "value C"}};
        m_objectColorDictionary = new ObjectColorDictionary {{gameObject, Color.blue}, {this, Color.red}};
    }
}
#pragma warning restore 0649