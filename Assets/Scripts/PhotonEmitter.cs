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
    private void EmitPhoton(Photon photon, int bounces = 3, float lastIOR = 1.0f, bool reverse = false)
    {
        // If there are no bounces remaining, return
        if (bounces < 0)
            return;

        // Check ray against scene
        Ray ray = new Ray(photon.Position, photon.IncidentDirection);

        RaycastHit hit;
        RaycastHit? maybe = null;
        bool hasCollision = false;

        float distance = 0.0f;
        Vector3 normal = Vector3.zero;

        if (!reverse)
        {
            hasCollision = Physics.Raycast(ray.origin, ray.direction, out hit);
            maybe = hit;
            distance = hit.distance;
            normal = hit.normal;
        }
        else if (reverse)
        {
            ray.origin = ray.GetPoint(100.0f);
            ray.direction = -ray.direction;

            RaycastHit[] results = Physics.RaycastAll(ray);
            RaycastHit? min = null;
            float minDist = 0.0f;
            foreach (RaycastHit result in results)
            {
                if (result.distance > minDist && result.distance < 100.0f)
                {
                    min = result;
                    minDist = result.distance;
                }
            }

            if (min != null)
            {
                hasCollision = true;
                maybe = min;
                hit = (RaycastHit)maybe;
                distance = 100.0f - hit.distance;
                normal = -hit.normal;
            }
        }

        if (hasCollision)
        {
            hit = (RaycastHit)maybe;

            Debug.DrawRay(photon.Position, photon.IncidentDirection * distance, Color.green);
            if (reverse)
                photon.IncidentDirection = -photon.IncidentDirection;

            photon.Position = hit.point;

            var renderer = hit.collider.GetComponentInParent<MeshRenderer>();
            var material = renderer.material;

            // Calculate probabilities of various kinds of reflections
            float probReflection = ProbReflection(material);
            float probDiffuse = ProbDiffuseReflection(material);
            float probSpecular = ProbSpecularReflection(material);
            float probTransmission = ProbTransmission(material);

            Color materialColor = GetColor(material);

            // Perform Russian Roulette to determine how photon bounces
            float choice1 = Random.Range(0.0f, 1.0f);
            float choice2 = Random.Range(0.0f, 1.0f);

            // TODO Validate correct handling of transmission
            // Refraction
            if (choice1 < probTransmission)
            {
                // Inset position into the surface somewhat
                photon.Position -= hit.normal * 0.01f;
                // See https://raytracing.github.io/books/RayTracingInOneWeekend.html#dielectrics/refraction for math
                float ior = GetRefractiveIndex(material);
                float etai_over_etat = lastIOR / ior;
                // Assume we're leaving surface if ior is equal to the last
                if (reverse) {
                    etai_over_etat = ior / lastIOR;
                }
                float cos_theta = Mathf.Min(Vector3.Dot(-photon.IncidentDirection, normal), 1.0f);
                Vector3 r_out_perp = etai_over_etat * (photon.IncidentDirection + cos_theta * normal);
                Vector3 r_out_parallel = -Mathf.Sqrt(Mathf.Abs(1.0f - r_out_perp.sqrMagnitude)) * normal;
                photon.IncidentDirection = r_out_perp + r_out_parallel;
                photon.Power = (photon.Power * materialColor) / probTransmission;
                EmitPhoton(photon, bounces - 1, ior, !reverse);
            }
            // Reflection or absorbtion
            else
            {
                // Diffuse bounce
                if (choice2 < probDiffuse)
                {
                    photon.Power = (photon.Power * materialColor) / probDiffuse;
                    photon.IncidentDirection = Random.onUnitSphere;
                    if (Vector3.Dot(photon.IncidentDirection, normal) < 0)
                        photon.IncidentDirection = -photon.IncidentDirection;
                    EmitPhoton(photon, bounces - 1);
                }
                // Specular bounce
                else if (choice2 < probReflection)
                {
                    photon.Power = (photon.Power * materialColor) / probSpecular;
                    photon.IncidentDirection = Vector3.Reflect(photon.IncidentDirection, normal);
                    EmitPhoton(photon, bounces - 1);
                }
                // Absorption
                else
                {
                    _map.AddPhoton(photon);
                }
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

    private float ProbTransmission(Material material)
    {
        return 1.0f - GetColor(material).a;
    }

    private float GetRefractiveIndex(Material material)
    {
        // TODO Get IOR from material
        return 1.5f;
    }

    private Color GetColor(Material material)
    {
        // TODO Get color from material
        return new Color(1.0f, 1.0f, 1.0f, 0.5f);
    }
}
