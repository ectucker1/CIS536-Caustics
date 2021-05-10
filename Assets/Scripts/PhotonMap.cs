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

    public bool UseFilter = true;
    public float FilterConstant = 1.0f;
    public int NumSamples = 100;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        // While we are building
        if (_tree == null)
        {
            // Visualize some of the photons
            int drawn = 0;
            foreach (Photon p in _photons)
            {
                Gizmos.DrawSphere(p.Position, 0.01f);
                drawn++;
                if (drawn > 500)
                    break;
            }
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
        _query.KNearest(_tree, point, NumSamples, nearest);

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
            if (!UseFilter)
                weight = 1.0f;
            totalPower += p.Power * weight * Mathf.Abs(Vector3.Dot(p.IncidentDirection, normal));
        }

        // Divide by sphere area
        totalPower /= (Mathf.PI * maxRadius * maxRadius);
        // Divide by filter distribution
        if (UseFilter)
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
