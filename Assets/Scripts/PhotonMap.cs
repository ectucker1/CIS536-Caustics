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
        }
    }

    public void AddPhoton(Photon photon)
    {
        _photons.Add(photon);
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
