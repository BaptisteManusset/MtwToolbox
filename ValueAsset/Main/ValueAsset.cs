using System;
using JetBrains.Annotations;
using UnityEngine;

#pragma warning disable 0649

//https://stackoverflow.com/questions/54112813/how-to-create-a-dynamic-variable-system-with-scriptable-objects
namespace ItsBaptiste.Toolbox.ValueAsset.Main {
    public class ValueAsset<T> : ScriptableObject, ISerializationCallbackReceiver {
        public T initialValue;

        #region MyRegion

        [Header("Runtime Variable")] [SerializeField] private T value;


        public T Value {
            get => value;
            set {
                Debug.Log("Value change");
                OnValueChange?.Invoke();
                this.value = value;
            }
        }

        #endregion


#if UNITY_EDITOR
        [Header("Editor Description")] [UsedImplicitly] [Space] [TextArea] [SerializeField]
        private string description;
#endif


        public Action OnValueChange;


        private void SetValue(T value) {
            Value = value;
        }

        public void OnAfterDeserialize() {
            SetValue(initialValue);
        }

        public void OnBeforeSerialize() { }

        private void OnEnable() {
            Value = initialValue;
        }

        public void ResetValue() {
            value = initialValue;
        }
    }
}