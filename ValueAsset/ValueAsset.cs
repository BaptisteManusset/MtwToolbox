using UnityEngine;

#pragma warning disable 0649

//https://stackoverflow.com/questions/54112813/how-to-create-a-dynamic-variable-system-with-scriptable-objects
public class ValueAsset<T> : ScriptableObject {
    [SerializeField] private T value;
    [SerializeField] private T defaultValue;

    [Space(60)] [TextArea] [SerializeField]
    private string description;

    public void ResetValue() {
        SetValue(defaultValue);
    }

    public void SetValue(T value) {
        this.value = value;
    }

    public T GetValue() {
        return value;
    }
}

[CreateAssetMenu(fileName = "new int", menuName = "ValueAssets/int")]
public class IntValue : ValueAsset<int> { }

[CreateAssetMenu(fileName = "new float", menuName = "ValueAssets/float")]
public class FloatValue : ValueAsset<float> { }
#pragma warning restore 0649