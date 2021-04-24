using UnityEngine;
using System.Collections;

namespace UVWorld {

    [RequireComponent(typeof(Collider))]
    public class RayUVWorld : AbstractUVWorld {
        public Camera targetCam;

        Collider _attachedCollider;

    	void Awake() {
            _attachedCollider = GetComponent<Collider> ();
        }

        #region implemented abstract members of AbstractUVWorld
        public override bool World(Vector2 uv, out Vector3 pos, out Vector3 normal, bool extrude = true) {
            if (extrude)
                uv = Extrude (uv);
            var ray = targetCam.ViewportPointToRay (uv);

            RaycastHit hit;
            if (_attachedCollider.Raycast (ray, out hit, float.MaxValue)) {
                pos = hit.point;
                normal = hit.normal;
                return true;
            }

            pos = Vector3.zero;
            normal = Vector3.up;
            return false;
        }
        public override bool UV (Vector3 pos, out Vector2 uv, bool extrude = true) {
            uv = (Vector2)targetCam.WorldToViewportPoint (pos);
            if (extrude)
                uv = Intrude (uv);
            return 0f <= uv.x && uv.x <= 1f && 0f <= uv.y && uv.y <= 1f;
        }
        #endregion
    }
}
