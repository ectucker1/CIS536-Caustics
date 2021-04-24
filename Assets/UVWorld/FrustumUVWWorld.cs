using UnityEngine;
using System.Collections;

namespace UVWorld {

    [ExecuteInEditMode]
    public class FrustumUVWWorld : AbstractUVWWorld {
        public enum DebugModeEnum { GizmoSelected = 0, Gizmo }
        public enum FitEnum { Frustum = 0, Far }
        public static readonly Vector3[] FRUSTUM_VERTICES = new Vector3[]{
            new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f),
            new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f), new Vector3(0f, 1f, 1f), new Vector3(1f, 1f, 1f),
        };
        public static readonly int[] FRUSTUM_QUADS = new int[] { 
            0, 2, 3, 1,  4, 0, 1, 5,  1, 3, 7, 5,  4, 6, 2, 0,  2, 6, 7, 3,  5, 7, 6, 4
        };
		public const float MIN_CLIP_DIST = 1e-3f;

        public DebugModeEnum debugMode;
        public FitEnum fit;
        public Camera targetCam;
        public float nearPlane = 1f;
        public float farPlane = 100f;
        public Color frustumColor = Color.gray;

        Mesh _frustumMesh;
        Vector3[] _vertices;
        void OnEnable() {
            _vertices = new Vector3[8];
            _frustumMesh = new Mesh ();
            _frustumMesh.vertices = _vertices;
            _frustumMesh.normals = new Vector3[8];
            _frustumMesh.SetIndices (FRUSTUM_QUADS, MeshTopology.Quads, 0);
            _frustumMesh.RecalculateBounds ();
            _frustumMesh.RecalculateNormals ();
        }
        void OnDisable() {
            DestroyImmediate (_frustumMesh);
        }
        void OnDrawGizmos() {
            if (debugMode == DebugModeEnum.Gizmo)
                DrawGizmos ();
        }
        void OnDrawGizmosSelected() {
            if (debugMode == DebugModeEnum.GizmoSelected)
                DrawGizmos ();
        }

        #region implemented abstract members of AbstractUVWWorld
        public override Vector3 World (Vector3 uvw, bool extrude = true) {
            switch (fit) {
            case FitEnum.Far:
                return WorldFar (uvw, extrude);
            default:
                return WorldFrastum (uvw, extrude);
            }
        }
		public override Vector3 UVW (Vector3 world, bool intrude = true) {
			switch (fit) {
			case FitEnum.Far:
				return UVWFar (world, intrude);
			default:
				return UVWFrastum (world, intrude);
			}
		}
        #endregion

        public Vector3 WorldFrastum(Vector3 uvw, bool extrude) {
            if (extrude)
                uvw = Extrude (uvw);
            uvw.z = Mathf.LerpUnclamped (nearPlane, farPlane, uvw.z);
            return targetCam.ViewportToWorldPoint (uvw);
        }
		public Vector3 UVWFrastum(Vector3 world, bool intrude) {
			CheckClipPlanes ();
			var uvw = targetCam.WorldToViewportPoint (world);
			uvw.z = (uvw.z - nearPlane) / (farPlane - nearPlane);
			if (intrude)
				uvw = Intrude(uvw);
			return uvw;
		}

        public Vector3 WorldFar(Vector3 uvw, bool extrude) {
            if (extrude)
                uvw = Extrude (uvw);
            var z = Mathf.LerpUnclamped (nearPlane, farPlane, uvw.z);
            uvw.z = farPlane;
            var localPos = targetCam.transform.InverseTransformPoint (targetCam.ViewportToWorldPoint (uvw));
            localPos.z = z;
            return targetCam.transform.TransformPoint (localPos);
        }
		public Vector3 UVWFar(Vector3 world, bool intrude) {
			CheckClipPlanes ();
			var local = targetCam.transform.InverseTransformPoint (world);
			var z = (local.z - nearPlane) / (farPlane - nearPlane);
			local.z = farPlane;
			var uvw = targetCam.WorldToViewportPoint (targetCam.transform.TransformPoint (local));
			uvw.z = z;
			if (intrude)
				uvw = Intrude (uvw);
			return uvw;
		}

		void CheckClipPlanes() {
			if ((farPlane - nearPlane) < MIN_CLIP_DIST)
				farPlane = nearPlane + MIN_CLIP_DIST;
		}
        void UpdateFrustumMesh() {
            for (var i = 0; i < FRUSTUM_VERTICES.Length; i++)
                _vertices [i] = World (FRUSTUM_VERTICES [i]);
            _frustumMesh.vertices = _vertices;
            _frustumMesh.RecalculateBounds ();
            _frustumMesh.RecalculateNormals ();
        }

        void DrawGizmos () {
            if (targetCam == null || _vertices == null || _frustumMesh == null)
                return;
            UpdateFrustumMesh ();
            Gizmos.color = frustumColor;
            Gizmos.DrawMesh (_frustumMesh);
            Gizmos.color = 1.1f * frustumColor;
            Gizmos.DrawWireMesh (_frustumMesh);
        }
    }
}
