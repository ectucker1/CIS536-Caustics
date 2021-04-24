using UnityEngine;
using System.Collections;

namespace UVWorld {

    [ExecuteInEditMode]
    public class NormalizedPosition : MonoBehaviour {
        public const float SEED_SCALE = 100f;
        public AbstractUVWWorld uvwWorld;
		public Transform[] points;
        public float freq = 0.1f;

        Vector3 _seed;

        void OnEnable() {
            _seed = new Vector3 (Random.value, Random.value, Random.value) * SEED_SCALE;
        }
    	void Update () {
            var t = Time.timeSinceLevelLoad * freq;
            var uvw = new Vector3 (
                Mathf.PerlinNoise (t + _seed.x, _seed.y),
                Mathf.PerlinNoise (t + _seed.y, _seed.z),
                Mathf.PerlinNoise (t + _seed.z, _seed.x));
            
			foreach (var p in points) {
            	p.position = uvwWorld.World (uvw);
				uvw = uvwWorld.UVW (p.position);
			}
    	}
    }
}
