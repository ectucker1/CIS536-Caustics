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

        float maxRadiusSquared = 0;
        Color totalPower = Color.black;

        foreach (int index in nearest)
        {
            Photon p = _mapping[index];
            var distSq = (p.Position - point).sqrMagnitude;
            if (distSq > maxRadiusSquared)
            {
                maxRadiusSquared = distSq;
            }
            // TODO account for incident angle
            totalPower += p.Power;
        }

        return totalPower / (Mathf.PI * maxRadiusSquared) / nearest.Count;
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
