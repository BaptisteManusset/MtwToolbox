using System;
using UnityEngine;
using UnityEngine.UI;

namespace Toolbox.UIScript {
    public class VariableToUi : MonoBehaviour {
        enum Select {
            FillAmount = 1,
            Opacity = 2
        }

        [SerializeField] private Select elementToModif;

        [Header("Image")] [SerializeField] private Image image;

        [SerializeField] private FloatValue valueAsset;


        [SerializeField] private bool invert;


        [Space] [SerializeField] private bool useCurve = false;
        [SerializeField] private AnimationCurve curve;

        private Color _color;

        private void Awake() {
            valueAsset.OnValueChange += OnValueChange;
            _color = image.color;

            OnValueChange();
        }

        private void OnValueChange() {
            MtwTools.Log("Value Change");
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
}