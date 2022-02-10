using System;
using TMPro;
using UnityEngine;

namespace ItsBaptiste {
    public class GameVersion : MonoBehaviour {
        [SerializeField] private TMP_Text text;
        private string _versionNomenclature;

        private static readonly Rect Pos = new Rect(0, 0, 180, 50);

        private void Awake() {
            _versionNomenclature = $"<size=24><b>{Application.productName}</b></size>\n {Application.version} ({DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day})";
            if (text) text.text = _versionNomenclature;
        }

        void OnGUI() =>
            GUI.Box(Pos, _versionNomenclature);
    }
}