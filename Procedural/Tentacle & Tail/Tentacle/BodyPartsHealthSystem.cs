using ItsBaptiste._Scripts.Core;
using ItsBaptiste.Core;
using ItsBaptiste.Interface;
using UnityEngine;

namespace Toolbox.Procedural.Tentacle {
    [DisallowMultipleComponent]
    public class BodyPartsHealthSystem : MonoBehaviour, IDamageable {
        private HealthManager _healthManager;

        private void Awake() {
            _healthManager = GetComponentInParent<HealthManager>();
        }

        public void TakeDamage(float value, TeamsInfo.Team shooterTeamsInfo) {
            _healthManager.TakeDamage(value, shooterTeamsInfo);
        }
    }
}