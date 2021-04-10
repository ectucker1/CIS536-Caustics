using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to attach to lights so they emit photons
public class PhotonEmitter : MonoBehaviour
{
    const int MAX_EMITTED = 100000;

    private Light _light;

    public GameObject PhotonMap;
    private PhotonMap _map;

    private int _numEmitted = 0;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
        _map = PhotonMap.GetComponent<PhotonMap>();
    }

    // Emit more photons each frame
    void Update()
    {
        if (_map != null && _light != null && _numEmitted < MAX_EMITTED)
        {
            for (int i = 0; i < 100; i++)
            {
                // Create initial photon state
                Photon photon;
                photon.Position = transform.position;
                photon.IncidentDirection = GetPhotonDirection();
                photon.Power = _light.intensity * _light.color;

                EmitPhoton(photon);
                _numEmitted++;
            }
        }
    }

    // Emit the given photon
    private void EmitPhoton(Photon photon, int bounces = 5)
    {
        // If there are no bounces remaining, return
        if (bounces < 0)
            return;

        // Check ray against scene
        RaycastHit hit;
        if (Physics.Raycast(photon.Position, photon.IncidentDirection, out hit))
        {
            Debug.DrawRay(photon.Position, photon.IncidentDirection * hit.distance, Color.green);

            photon.Position = hit.point;

            var renderer = hit.collider.GetComponentInParent<MeshRenderer>();
            var material = renderer.material;

            // Calculate probabilities of various kinds of reflections
            float probReflection = ProbReflection(material);
            float probDiffuse = ProbDiffuseReflection(material);
            float probSpecular = ProbSpecularReflection(material);

            Color materialColor = GetColor(material);

            // Perform Russian Roulette to determine how photon bounces
            float choice = Random.Range(0.0f, 1.0f);
            // Diffuse bounce
            if (choice < probDiffuse)
            {
                photon.Power = (photon.Power * materialColor) / probDiffuse;
                photon.IncidentDirection = Random.onUnitSphere;
                if (Vector3.Dot(photon.IncidentDirection, hit.normal) < 0)
                    photon.IncidentDirection = -photon.IncidentDirection;
                EmitPhoton(photon, bounces - 1);
            }
            // Specular bounce
            else if (choice < probReflection)
            {
                photon.Power = (photon.Power * materialColor) / probSpecular;
                photon.IncidentDirection = Vector3.Reflect(photon.IncidentDirection, hit.normal);
                EmitPhoton(photon, bounces - 1);
            }
            // Absorption
            else
            {
                _map.AddPhoton(photon);
            }
        }
        else
        {
            Debug.DrawRay(photon.Position, photon.IncidentDirection, Color.red);
        }
    }

    private Vector3 GetPhotonDirection()
    {
        // If there is no light, return
        if (_light == null)
            return Vector3.down;

        // Determine which direction to emit photons based on the kind of this light
        switch (_light.type)
        {
            // For point lights, return a random direction
            case LightType.Point:
                return Random.onUnitSphere;
            default:
                return Vector3.down;
        }
    }

    private float ProbReflection(Material material)
    {
        // TODO Calculate reflection proability from PBR material properties
        return 0.6f;
    }

    private float ProbDiffuseReflection(Material material)
    {
        // TODO Calculate reflection proability from PBR material properties
        return 0.3f;
    }

    private float ProbSpecularReflection(Material material)
    {
        // TODO Calculate reflection proability from PBR material properties
        return ProbReflection(material) - ProbDiffuseReflection(material);
    }

    private Color GetColor(Material material)
    {
        // TODO Get color from material
        return Color.white;
    }
}
