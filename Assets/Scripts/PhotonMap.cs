using DataStructures.ViliWonka.KDTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Actual storage for the photons
public class PhotonMap : MonoBehaviour
{
    private List<Photon> _photons = new List<Photon>();

    private KDTree _tree;
    private KDQuery _query;

    // Parallel array with the one in the KDTree
    private Photon[] _mapping;

    public bool TreeBuilt { get; private set; } = false;

    public float FilterConstant = 1.0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        if (_tree == null)
        {
            int drawn = 0;
            foreach (Photon p in _photons)
            {
                Gizmos.DrawSphere(p.Position, 0.01f);
                drawn++;
                if (drawn > 500)
                    break;
            }
        }
        else
        {
            // Visualize KD-tree query
            List<int> results = new List<int>();
            _query.KNearest(_tree, Vector3.zero, 100, results);
            foreach (int index in results)
            {
                Gizmos.DrawSphere(_mapping[index].Position, 0.01f);
            }

            // Visualize radiance estimate
            Gizmos.color = EstimateRadiance(Vector3.zero, Vector3.up);
            Gizmos.DrawSphere(Vector3.zero, 0.05f);
        }
    }

    public void AddPhoton(Photon photon)
    {
        _photons.Add(photon);
    }

    public Color EstimateRadiance(Vector3 point, Vector3 normal)
    {
        // Find the nearest 100 photons
        List<int> nearest = new List<int>();
        _query.KNearest(_tree, point, 100, nearest);

        // Find furthest photon in radius
        float maxRadius = 0;
        foreach (int index in nearest)
        {
            Photon p = _mapping[index];
            var dist = (p.Position - point).magnitude;
            if (dist > maxRadius)
            {
                maxRadius = dist;
            }
        }

        // Calculate power
        Color totalPower = Color.black;
        foreach (int index in nearest)
        {
            Photon p = _mapping[index];
            var dist = (p.Position - point).magnitude;
            // Cone filter weight
            // See "A Practical Guide to Global Illumination using Photon Maps"
            var weight = 1.0f - dist / (FilterConstant * maxRadius);
            totalPower += p.Power * weight * Mathf.Abs(Vector3.Dot(p.IncidentDirection, normal));
        }

        // Divide by sphere area
        totalPower /= (Mathf.PI * maxRadius * maxRadius);
        // Divide by filter distribution
        totalPower /= 1.0f - 2.0f / (3.0f * FilterConstant);

        return totalPower * 100.0f;
    }

    public void BuildTree()
    {
        // Per KD-tree documentation: Higher maxPointsPerLeafNode makes construction of tree faster, but querying slower.
        int maxPointsPerLeafNode = 32;

        // Build parallel arrays of points and photons
        var pointArray = new Vector3[_photons.Count];
        _mapping = new Photon[_photons.Count];
        for (int i = 0; i < _photons.Count; i++)
        {
            pointArray[i] = _photons[i].Position;
            _mapping[i] = _photons[i];
        }

        // Actually build the KD tree
        _tree = new KDTree(pointArray, maxPointsPerLeafNode);
        _query = new KDQuery();
        TreeBuilt = true;
    }
}
