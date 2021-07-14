using System;
#if UNITY_EDITOR
using JetBrains.Annotations;
using NaughtyAttributes;
#endif
using UnityEngine;

#pragma warning disable 0649

//https://stackoverflow.com/questions/54112813/how-to-create-a-dynamic-variable-system-with-scriptable-objects
public class ValueAsset<T> : ScriptableObject, ISerializationCallbackReceiver {
    public T initialValue;

    #region MyRegion

    [Header("Runtime Variable")] [SerializeField] private T value;


    public T Value {
        get => value;
        set {
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

    public void OnAfterDeserialize() {
        Value = initialValue;
    }

    public void OnBeforeSerialize() { }

    private void OnEnable() {
        Value = initialValue;
    }
}