using ItsBaptiste.Interface;
using ItsBaptiste.Main;
using UnityEngine;

namespace Toolbox.Procedural.Tentacle {
    [DisallowMultipleComponent]
    public class BodyPartsHealthSystem : MonoBehaviour, IDamageable {
        private HealthSystem _healthSystem;

        private void Awake() {
            _healthSystem = GetComponentInParent<HealthSystem>();
        }

        public void TakeDamage(float value, TeamsInfo.Team shooterTeamsInfo) {
            _healthSystem.TakeDamage(value, shooterTeamsInfo);
        }
    }
}