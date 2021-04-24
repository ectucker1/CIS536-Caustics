using UnityEngine;
using System.Collections;

namespace UVWorld {

    [RequireComponent(typeof(MeshFilter))]
    public class MeshUVWorld : AbstractUVWorld {
    	Mesh _mesh;
    	Vector3[] _vertices;
    	int[] _triangles;
    	Vector2[] _uvs;
    	Vector3[] _normals;
    	Triangle2D[] _uvTris;

    	void Awake() {
    		_mesh = GetComponent<MeshFilter>().sharedMesh;
    		Init();
    	}

        public void Init() {
    		_triangles = _mesh.triangles;
    		_vertices = _mesh.vertices;
    		_normals = _mesh.normals;
    		_uvs = _mesh.uv;

    		var triangleCount = _triangles.Length / 3;
    		_uvTris = new Triangle2D[triangleCount];
    		for (var i = 0; i < triangleCount; i++)
                _uvTris [i] = new Triangle2D (i, _vertices, _uvs, _normals, _triangles);
    	}
        public bool Local(Vector2 uv, out Vector3 pos, out Vector3 normal, bool extrude = true) {
            if (extrude)
                uv = Extrude (uv);
    		for (var i = 0; i < _uvTris.Length; i++) {
    			var uvTri = _uvTris[i];
    			float s, t;
    			if (uvTri.UvIn(uv, out s, out t)) {
    				var r = 1f - (s + t);
    				pos = uvTri.InterpolatePosition(s, t, r);
    				normal = uvTri.InterpolateNormal(s, t, r);
    				return true;
    			}
    		}

    		pos = default(Vector3);
    		normal = default(Vector3);
    		return false;
    	}

        #region implemented abstract members of AbstractUVWorld
        public override bool World(Vector2 uv, out Vector3 pos, out Vector3 normal, bool extrude) {
            var result = Local (uv, out pos, out normal, extrude);
            if (result) {
                pos = transform.TransformPoint (pos);
                normal = transform.TransformDirection (normal);
            }
            return result;
        }
        public override bool UV (Vector3 pos, out Vector2 uv, bool extrude = true) {
            throw new System.NotImplementedException ();
        }
        #endregion

    	public class Triangle2D {
            public int i;
            public int i3;
            public Vector3[] _vertices;
            public Vector2[] _uvs;
            public Vector3[] _normals;
            public int[] _triangles;

    		public Vector2 a;
    		public Vector2 ab;
    		public Vector2 ac;
    		public float inv_det_abac;
    		public bool valid;

            public Triangle2D(int i, Vector3[] vertices, Vector2[] uvs, Vector3[] normals, int[] triangles) {
                this.i = i;
                this.i3 = i * 3;
                this._vertices = vertices;
                this._uvs = uvs;
                this._normals = normals;
                this._triangles = triangles;

                Vector2 a, b, c;
                Uvs(out a, out b, out c);

    			this.a = a;
    			this.ab = b - a;
    			this.ac = c - a;
    			var det_abac = (ab.x * ac.y - ab.y * ac.x);
    			this.inv_det_abac = 1f / det_abac;
    			this.valid = (det_abac < -Mathf.Epsilon || Mathf.Epsilon < det_abac);
    		}

    		public bool UvIn(Vector2 uv, out float s, out float t) {
    			if (!valid) {
    				s = 0f;
    				t = 0f;
    				return false;
    			}

    			var ap = uv - a;
    			var det_apac = ap.x * ac.y - ap.y * ac.x;
    			if ((s = det_apac * inv_det_abac) < 0f) {
    				t = 0f;
    				return false;
    			}

    			var det_abap = ab.x * ap.y - ab.y * ap.x;
    			if ((t = det_abap * inv_det_abac) < 0f)
    				return false;

    			var r = 1f - (s + t);
    			return r >= 0f;
    		}
            public void Uvs(out Vector2 uva, out Vector2 uvb, out Vector2 uvc) {
                uva = _uvs [_triangles [i3]];
                uvb = _uvs [_triangles [i3 + 1]];
                uvc = _uvs [_triangles [i3 + 2]];
            }
            public void Vertices(out Vector3 va, out Vector3 vb, out Vector3 vc) {
                va = _vertices [_triangles [i3]];
                vb = _vertices [_triangles [i3 + 1]];
                vc = _vertices [_triangles [i3 + 2]];
            }
            public void Normals(out Vector3 na, out Vector3 nb, out Vector3 nc) {
                na = _normals [_triangles [i3]];
                nb = _normals [_triangles [i3 + 1]];
                nc = _normals [_triangles [i3 + 2]];
            }

            public Vector3 InterpolatePosition (float s, float t, float r) {
                Vector3 va, vb, vc;
                Vertices(out va, out vb, out vc);
                return r * va + s * vb + t * vc;
            }
            public Vector3 InterpolateNormal(float s, float t, float r) {
                Vector3 na, nb, nc;
                Normals (out na, out nb, out nc);
                return r * na + s * nb + t * nc;
            }
    	}
    }
}
