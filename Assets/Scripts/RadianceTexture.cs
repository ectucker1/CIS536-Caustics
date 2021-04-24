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

    private bool _textureBuilt = false;

    void Start()
    {
        _UVWorld = GetComponent<AbstractUVWorld>();
        _map = PhotonMap.GetComponent<PhotonMap>();
    }

    void Update()
    {
        if (_map.TreeBuilt && !_textureBuilt)
        {
            // Create a new texture of the set size
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);

            // For each pixel in the texture
            for (int u = 0; u < TextureSize; u++)
            {
                for (int v = 0; v < TextureSize; v++)
                {
                    // Get the UV coordinates
                    Vector2 uv = new Vector2((float)u / TextureSize, (float)v / TextureSize);

                    // Get the global position and normal
                    Vector3 position;
                    Vector3 normal;
                    bool onSurface = _UVWorld.World(uv, out position, out normal);

                    // If the position is actually on the surface
                    if (onSurface)
                    {
                        // Set that point to the radiance estimate
                        Color radiance = _map.EstimateRadiance(position);
                        texture.SetPixel(u, v, radiance);
                    }
                }
            }

            // Apply changes to texture
            texture.Apply();
            
            // Apply texture as material
            Material unlit = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)"));
            unlit.mainTexture = texture;
            GetComponent<Renderer>().material = unlit;

            _textureBuilt = true;
        }
    }
}
