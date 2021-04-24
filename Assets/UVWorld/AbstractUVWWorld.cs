using UnityEngine;
using System.Collections;

namespace UVWorld {

    public abstract class AbstractUVWWorld : MonoBehaviour {
        public Vector3 extrude = Vector3.one;

        public abstract Vector3 World (Vector3 uvw, bool extrude = true);
		public abstract Vector3 UVW (Vector3 world, bool intrude = true);

        protected Vector3 Extrude(Vector3 uvw) {
            return new Vector3 (
                extrude.x * (uvw.x - 0.5f) + 0.5f,
                extrude.y * (uvw.y - 0.5f) + 0.5f,
                extrude.z * (uvw.z - 0.5f) + 0.5f);
        }
		protected Vector3 Intrude(Vector3 uvw) {
			return new Vector3 (
				(uvw.x - 0.5f) / extrude.x + 0.5f,
				(uvw.y - 0.5f) / extrude.y + 0.5f,
				(uvw.z - 0.5f) / extrude.z + 0.5f);
		}
    }
}
