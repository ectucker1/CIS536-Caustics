using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Actual storage for the photons
public class PhotonMap : MonoBehaviour
{
    private List<Photon> _photons = new List<Photon>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        int drawn = 0;
        foreach (Photon p in _photons)
        {
            Gizmos.DrawSphere(p.Position, 0.01f);
            drawn++;
            if (drawn > 500)
                break;
        }
    }

    public void AddPhoton(Photon photon)
    {
        _photons.Add(photon);
    }
}
