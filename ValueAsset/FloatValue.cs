using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "new float", menuName = "ValueAssets/float")]
public class FloatValue : ValueAsset<float> {
    [Button]
    void ButtonDecrease() {
        Value -= 1;
    }

    [Button]
    void ButtonIncrease() {
        Value += 1;
    }
}
#pragma warning restore 0649