using UnityEngine;
using System.Collections;

namespace UVWorld {

    public class Pin : MonoBehaviour {
        public AbstractUVWorld uvMesh;
        public Camera targetCam;

    	void Update () {
            var uv = targetCam.ScreenToViewportPoint (Input.mousePosition);
            Vector3 pos;
            Vector3 normal;
            if (uvMesh.World (uv, out pos, out normal)) {
                transform.position = pos;
                transform.up = normal;
            }
    	}
    }
}