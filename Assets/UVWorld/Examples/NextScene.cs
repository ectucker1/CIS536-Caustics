using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace UVWorld {

    public class NextScene : MonoBehaviour {
        public KeyCode switchKey = KeyCode.N;

    	void Start () {
            DontDestroyOnLoad (gameObject);
    	}
    	
    	void Update () {
            if (Input.GetKeyDown (switchKey)) {
                var currScene = SceneManager.GetActiveScene ();
                SceneManager.LoadScene ((currScene.buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
            }
    	}
    }
}