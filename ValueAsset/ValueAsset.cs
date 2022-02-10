using System;
using UnityEngine;

#pragma warning disable 0649

//https://stackoverflow.com/questions/54112813/how-to-create-a-dynamic-variable-system-with-scriptable-objects
namespace ItsBaptiste.Toolbox.ValueAsset {
    public class ValueAsset<T> : ScriptableObject, ISerializationCallbackReceiver {
        public T initialValue;


        [Header("Runtime Variable")] [SerializeField] private T value;


        public T Value {
            get => value;
            set {
                OnValueChange?.Invoke();
                this.value = value;
            }
        }

#if UNITY_EDITOR
        [Header("Editor Description")] [Space] [TextArea] [SerializeField] private string description;
#endif


        public Action OnValueChange;

        public void OnAfterDeserialize() {
            Value = initialValue;
        }

        public void OnBeforeSerialize() { }

        private void OnEnable() {
            Value = initialValue;
        }
    }
}