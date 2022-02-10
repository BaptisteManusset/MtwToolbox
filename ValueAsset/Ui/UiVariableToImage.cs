using ItsBaptiste.Toolbox.ValueAsset;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace ItsBaptiste.Toolbox.UIScript {
#pragma warning disable 0649

    public class UiVariableToImage : MonoBehaviour {
        enum Select {
            FillAmount = 1,
            Opacity = 2
        }

        [SerializeField] private Select elementToModif;

        [Header("Image")] [SerializeField] private Image image;

        [SerializeField] private FloatValue valueAsset;


        [SerializeField] private bool invert;


        [Space] [SerializeField] private bool useCurve = false;
        [ShowIf("useCurve")] [SerializeField] private AnimationCurve curve;

        private Color _color;

        private void Awake() {
            _color = image.color;

            OnValueChange();
        }

        private void OnEnable() {
            valueAsset.OnValueChange += OnValueChange;
            OnValueChange();
        }

        private void OnDisable() {
            valueAsset.OnValueChange -= OnValueChange;
            OnValueChange();
        }


        private void OnValueChange() {
            float value = valueAsset.Value / valueAsset.initialValue;
            if (invert) {
                value = 1 - value;
            }

            if (useCurve) {
                value = curve.Evaluate(value);
                value = Mathf.Clamp(value, 0, 1);
            }

            switch (elementToModif) {
                case Select.FillAmount:
                    image.fillAmount = value;
                    break;
                case Select.Opacity:


                    _color.a = value;
                    image.color = _color;
                    break;
            }
        }
    }
#pragma warning restore 0649
}