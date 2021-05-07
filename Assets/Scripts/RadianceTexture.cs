﻿using System.Collections;
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
            // If this is our first frame
            if (_currentU == 0)
            {
                // Create a new texture of the set size
                _texture = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
                // Create new material copying the old properties
                var renderer = GetComponent<Renderer>();
                Material blended = new Material(Shader.Find("Custom/BlendingShader"));
                blended.SetTexture("_PhotonMap", _texture);
                blended.color = renderer.material.color;
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
    }
}
