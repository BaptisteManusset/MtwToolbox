using ItsBaptiste.Interface;
using ItsBaptiste.Main;
using UnityEngine;

namespace Toolbox.Procedural.Tentacle {
    [DisallowMultipleComponent]
    public class BodyPartsHealthSystem : MonoBehaviour, IDamageable {
        private IDamageable _damageable;

        private void Awake() {
            _damageable = transform.root.GetComponent<IDamageable>();
        }

        public void TakeDamage(float value, TeamsInfo.Team shooterTeamsInfo) {
            _damageable.TakeDamage(value, shooterTeamsInfo);
        }
    }
}