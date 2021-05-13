using UnityEngine;

// Structure representing a photon to be stored in the map
public struct Photon
{
    // The position of the photon
    public Vector3 Position;
    // The power/color of the photon
    public Color Power;
    // The direction the photon was traveling before collision
    public Vector3 IncidentDirection;
}
