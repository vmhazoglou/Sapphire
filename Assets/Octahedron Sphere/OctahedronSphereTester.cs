﻿using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class OctahedronSphereTester : MonoBehaviour
{
    public int subdivisions = 0;

    public float radius = 1f;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = OctahedronSphereCreator.Create(subdivisions, radius);
        //GetComponent<MeshCollider>().sharedMesh = OctahedronSphereCreator.Create(subdivisions, radius);
    }
}