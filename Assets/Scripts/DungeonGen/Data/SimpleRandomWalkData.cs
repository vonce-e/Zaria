// Stores reusable parameters that configure the simple random walk generation algorithm.
// Written by Andrew Burke.

using UnityEngine;

[CreateAssetMenu(fileName = "SimpleRandomWalkParameters_", menuName = "PCG/Simple Random Walk Data")]
public class SimpleRandomWalkData : ScriptableObject
{
    public int iterations = 10, walkLength = 10;
    public bool startRandomlyEachIteration = true;
}
