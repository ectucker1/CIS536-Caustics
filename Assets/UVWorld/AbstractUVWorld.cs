using UnityEngine;
using System.Collections;

namespace UVWorld {

    public abstract class AbstractUVWorld : MonoBehaviour {
        public Vector2 extrude = Vector2.one;

        public abstract bool World (Vector2 uv, out Vector3 pos, out Vector3 normal, bool extrude = true);
        public abstract bool UV(Vector3 pos, out Vector2 uv, bool extrude = true);

        protected Vector2 Extrude(Vector2 uv) {
            return new Vector2 (
                extrude.x * (uv.x - 0.5f) + 0.5f,
                extrude.y * (uv.y - 0.5f) + 0.5f);
        }
        protected Vector2 Intrude(Vector2 uv) {
            return new Vector2 (
                (uv.x - 0.5f) / extrude.x + 0.5f,
                (uv.y - 0.5f) / extrude.y + 0.5f);
        }
    }
}
