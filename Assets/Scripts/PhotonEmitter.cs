using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to attach to lights so they emit photons
public class PhotonEmitter : MonoBehaviour
{
    // The maxiumum number of lights emitted
    public int MaxEmitted = 100000;
    // Reference to the light component
    private Light _light;

    // Whether or not to filter to only caustic photons
    public bool CausticsOnly = true;

    // Reference to the photon map to store photons in
    public GameObject PhotonMap;
    private PhotonMap _map;

    // The number of photons emitted so far
    private int _numEmitted = 0;

    // Get components on start
    void Start()
    {
        _light = GetComponent<Light>();
        _map = PhotonMap.GetComponent<PhotonMap>();
    }

    // Display number of photons left to emit on GUI
    void OnGUI()
    {
        if (_numEmitted < MaxEmitted)
        {
            GUI.Label(new Rect(0, 0, 500, 50), $"Emitting photons {_numEmitted}/{MaxEmitted}");
        }
    }

    // Emit more photons each frame
    void Update()
    {
        // If everything is initialized right, and we haven't emitted enough photons
        if (_map != null && _light != null && _numEmitted < MaxEmitted)
        {
            // Emit 100 photons
            for (int i = 0; i < 100; i++)
            {
                // Create initial photon state
                Photon photon;
                photon.Position = transform.position;
                photon.IncidentDirection = GetPhotonDirection();
                photon.Power = _light.intensity * _light.color / MaxEmitted;

                // Actually shoot the photon
                EmitPhoton(photon);
                _numEmitted++;
            }
        }
        // If the map hasn't been built yet, tell it to do so
        else if (!_map.TreeBuilt)
        {
            _map.BuildTree();
        }
    }

    // Emit the given photon
    private void EmitPhoton(Photon photon, int bounces = 3, float lastIOR = 1.0f, bool reverse = false, bool isCaustic = false)
    {
        // If there are no bounces remaining, return
        if (bounces < 0)
            return;

        // Check ray against scene
        Ray ray = new Ray(photon.Position, photon.IncidentDirection);

        // Information about the hit
        RaycastHit hit;
        RaycastHit? maybe = null;
        bool hasCollision = false;

        // The distance traveled and normal at the hit point
        float distance = 0.0f;
        Vector3 normal = Vector3.zero;

        // If we are not emitting in reverse
        if (!reverse)
        {
            // Send a normal raycast
            hasCollision = Physics.Raycast(ray.origin, ray.direction, out hit);
            maybe = hit;
            distance = hit.distance;
            normal = hit.normal;
        }
        // If we are emitting in reverse (e.g. from within transparent object)
        else if (reverse)
        {
            // Reverse the ray
            ray.origin = ray.GetPoint(100.0f);
            ray.direction = -ray.direction;

            // Get all hit points
            RaycastHit[] results = Physics.RaycastAll(ray);
            RaycastHit? min = null;
            // Iterate to find the actual closest hit point
            float minDist = 0.0f;
            foreach (RaycastHit result in results)
            {
                if (result.distance > minDist && result.distance < 100.0f)
                {
                    min = result;
                    minDist = result.distance;
                }
            }

            // If there was a reverse hit, store it's information
            if (min != null)
            {
                hasCollision = true;
                maybe = min;
                hit = (RaycastHit)maybe;
                distance = 100.0f - hit.distance;
                normal = -hit.normal;
            }
        }

        // If the photon hit something
        if (hasCollision)
        {
            hit = (RaycastHit)maybe;

            // Draw the path for debugging
            Debug.DrawRay(photon.Position, photon.IncidentDirection * distance, Color.green);

            // Update position to the hit position
            photon.Position = hit.point;

            // Get the material information
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

            // Refraction
            if (choice1 < probTransmission)
            {
                // Transmission -> caustic
                isCaustic = true;

                // Inset position into the surface somewhat
                photon.Position -= hit.normal * 0.01f;

                // Refract direction
                // See https://raytracing.github.io/books/RayTracingInOneWeekend.html#dielectrics/refraction for math
                float ior = GetRefractiveIndex(material);
                float n1_over_n2 = lastIOR / ior;
                // Assume we're leaving surface if ior is equal to the last
                if (reverse) {
                    n1_over_n2 = ior / lastIOR;
                }
                float cos_theta = Mathf.Min(Vector3.Dot(-photon.IncidentDirection, normal), 1.0f);
                Vector3 r_out_perp = n1_over_n2 * (photon.IncidentDirection + cos_theta * normal);
                Vector3 r_out_parallel = -Mathf.Sqrt(Mathf.Abs(1.0f - r_out_perp.sqrMagnitude)) * normal;
                photon.IncidentDirection = r_out_perp + r_out_parallel;

                // Update photon power
                photon.Power = (photon.Power * materialColor);

                // Emit new photon
                EmitPhoton(photon, bounces - 1, ior, !reverse, isCaustic: isCaustic);
            }
            // Reflection or absorbtion
            else
            {
                // Diffuse bounce
                if (choice2 < probDiffuse)
                {
                    photon.Power = (photon.Power * materialColor);
                    photon.IncidentDirection = Random.onUnitSphere;
                    if (Vector3.Dot(photon.IncidentDirection, normal) < 0)
                        photon.IncidentDirection = -photon.IncidentDirection;
                    EmitPhoton(photon, bounces - 1, isCaustic: isCaustic);
                }
                // Specular bounce
                else if (choice2 < probReflection)
                {
                    isCaustic = true;
                    photon.Power = (photon.Power * materialColor);
                    photon.IncidentDirection = Vector3.Reflect(photon.IncidentDirection, normal);
                    EmitPhoton(photon, bounces - 1, isCaustic: isCaustic);
                }
                // Absorption
                else
                {
                    // Store only if caustic or not filtering
                    if (isCaustic || !CausticsOnly)
                    {
                        _map.AddPhoton(photon);
                    }
                }
            }
        }
        else
        {
            // Red ray for miss
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
        return ProbDiffuseReflection(material) + ProbSpecularReflection(material);
    }

    private float ProbDiffuseReflection(Material material)
    {
        return material.GetFloat("_Diffuse");
    }

    private float ProbSpecularReflection(Material material)
    {
        return material.GetFloat("_Specular");
    }

    private float ProbTransmission(Material material)
    {
        return 1.0f - GetColor(material).a;
    }

    private float GetRefractiveIndex(Material material)
    {
        return material.GetFloat("_IOR");
    }

    private Color GetColor(Material material)
    {
        return material.GetColor("_Color");
    }
}
