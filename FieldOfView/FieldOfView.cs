using System;
using System.Collections.Generic;
using System.Linq;
using ItsBaptiste.Core;
using UnityEditor;
using UnityEngine;

namespace ItsBaptiste.Toolbox.FieldOfView {
    public class FieldOfView : MonoBehaviour {
        [Header("Parametres")] [SerializeField] public FieldOfViewParam fovParam;


        private List<Transform> visibleTargets = new List<Transform>();

        [Serializable]
        public class FieldOfViewParam {
            public float viewRadius;
            [Range(0, 360)] public float viewAngle;
            public LayerMask targetMask;
            public LayerMask obstacleMask;
        }

        public void SetFieldOfView(FieldOfViewParam fieldOfViewParam) {
            fovParam = fieldOfViewParam;
        }

        public List<Transform> FindVisibleTargets() {
            visibleTargets.Clear();

            var slt = Physics.OverlapSphere(transform.position, fovParam.viewRadius, fovParam.targetMask).ToList();
            List<Collider> targetsInViewRadius = slt;

            for (int i = 0; i < targetsInViewRadius.Count; i++) {
                Transform target = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                //verifie si l'on est dans la zone de vue
                if (Vector3.Angle(transform.forward, dirToTarget) < fovParam.viewAngle / 2) {
                    float dstToTarget = Vector3.Distance(transform.position, target.position);

                    //verifie que la cible est visible 
                    if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, fovParam.obstacleMask)) {
                        if (!visibleTargets.Contains(target))
                            visibleTargets.Add(target);
                    }
                }
            }

            return visibleTargets;
        }


        public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
            if (!angleIsGlobal) {
                angleInDegrees += transform.eulerAngles.y;
            }

            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            //Draws view reach
            Handles.color = Color.white;
            var position = transform.position;
            Handles.DrawWireArc(position, Vector3.up, Vector3.forward, 360, fovParam.viewRadius);

            //Draws cone of view
            Vector3 viewAngleA = DirFromAngle(-fovParam.viewAngle / 2, false);
            Vector3 viewAngleB = DirFromAngle(fovParam.viewAngle / 2, false);
            Handles.DrawLine(position, position + viewAngleA * fovParam.viewRadius);
            Handles.DrawLine(position, position + viewAngleB * fovParam.viewRadius);

            Gizmos.color = Color.red;
            foreach (Transform visibleTarget in visibleTargets) {
                Gizmos.DrawLine(transform.position, visibleTarget.position);
            }
        }
#endif
    }

    public static class FieldOfViewTools {
        public static List<Transform> IsInAnotherTeam(this List<Transform> list, TeamsInfo.Team team) {
            return list.Where(x =>
                x.GetComponent<TeamsInfo>() != null &&
                x.GetComponent<TeamsInfo>().GetTeam() != team).ToList();
        }
    }
}