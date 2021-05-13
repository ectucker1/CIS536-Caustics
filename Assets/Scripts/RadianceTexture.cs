using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UVWorld;

// Script applied to generate a radiance filter
public class RadianceTexture : MonoBehaviour
{
    // Photon map to query from
    public GameObject PhotonMap;
    // The size of the texture to generate
    public int TextureSize = 256;

    // Reference to UV world component
    private AbstractUVWorld _UVWorld;
    // Reference to photon map components
    private PhotonMap _map;

    // The current U coordinate to work on
    private int _currentU = 0;
    // The texture being generated
    private Texture2D _texture;

    // Scaling value for radiance
    public float RadianceAmount = 1.0f;
    private float _appliedRadiance = 1.0f;

    // Get components on start
    void Start()
    {
        _UVWorld = GetComponent<AbstractUVWorld>();
        _map = PhotonMap.GetComponent<PhotonMap>();
    }

    // Work on texture each update
    void Update()
    {
        // If everything is ready to build texture
        if (_map.TreeBuilt && _currentU < TextureSize)
        {
            // If this is our first frame
            if (_currentU == 0)
            {
                // Create a new texture of the set size
                _texture = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
                for (int y = 0; y < _texture.height; y++)
                {
                    for (int x = 0; x < _texture.width; x++)
                    {

                        _texture.SetPixel(x, y, Color.black);
                    }
                }

                // Create new material copying the old properties
                var renderer = GetComponent<Renderer>();
                Material blended = new Material(Shader.Find("Custom/BlendingShader"));
                blended.SetTexture("_PhotonMap", _texture);
                blended.SetFloat("_RadianceAmount", RadianceAmount);
                _appliedRadiance = RadianceAmount;
                blended.color = renderer.material.color;
                blended.SetFloat("_Specular", renderer.material.GetFloat("_Specular"));
                blended.SetFloat("_Glossiness", renderer.material.GetFloat("_Smoothness"));
                blended.SetFloat("_Diffuse", renderer.material.GetFloat("_Diffuse"));
                renderer.material = blended;
            }

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
                    Color radiance = _map.EstimateRadiance(position, normal);
                    _texture.SetPixel(_currentU, v, radiance);
                }
            }

            _currentU++;

            // Apply changes to texture
            _texture.Apply();
        }

        // Apply changes in radiance amount
        if (_appliedRadiance != RadianceAmount)
        {
            var renderer = GetComponent<Renderer>();
            var material = renderer.material;
            if (material.HasProperty("_RadianceAmount"))
            {
                material.SetFloat("_RadianceAmount", RadianceAmount);
                _appliedRadiance = RadianceAmount;
            }
        }
    }
}
