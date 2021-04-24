using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UVWorld;

public class RadianceTexture : MonoBehaviour
{
    public GameObject PhotonMap;
    public int TextureSize = 256;

    private AbstractUVWorld _UVWorld;
    private PhotonMap _map;

    private int _currentU = 0;
    private Texture2D _texture;

    void Start()
    {
        _UVWorld = GetComponent<AbstractUVWorld>();
        _map = PhotonMap.GetComponent<PhotonMap>();
    }

    void Update()
    {
        if (_map.TreeBuilt && _currentU < TextureSize)
        {
            // Create a new texture of the set size
            if (_currentU == 0)
                _texture = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);

            // For each pixel in the current row of the texture
            for (int v = 0; v < TextureSize; v++)
            {
                // Get the UV coordinates
                Vector2 uv = new Vector2((float)_currentU / TextureSize, (float)v / TextureSize);

                // Get the global position and normal
                Vector3 position;
                Vector3 normal;
                bool onSurface = _UVWorld.World(uv, out position, out normal);

                // If the position is actually on the surface
                if (onSurface)
                {
                    // Set that point to the radiance estimate
                    Color radiance = _map.EstimateRadiance(position);
                    _texture.SetPixel(_currentU, v, radiance);
                }
            }

            _currentU++;

            // Apply changes to texture
            _texture.Apply();
            
            // Apply texture as material
            Material unlit = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)"));
            unlit.mainTexture = _texture;
            GetComponent<Renderer>().material = unlit;
        }
    }
}
