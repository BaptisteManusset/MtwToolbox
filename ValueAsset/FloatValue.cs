using NaughtyAttributes;
using UnityEngine;

namespace ItsBaptiste.Toolbox.ValueAsset {
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

        public void ResetValue() {

            Value = initialValue;
        }
    }
}
#pragma warning restore 0649