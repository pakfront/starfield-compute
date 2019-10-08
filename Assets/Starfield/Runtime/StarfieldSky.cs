using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarfieldSky : MonoBehaviour
{
    public Material _material;
    public int numberOfSides = 24;
    Starfield _starfield;
    void Start()
    {
        this._starfield = GetComponent<Starfield>();
        if (this._material == null) 
        { 
            _material = GetComponent<Renderer>().sharedMaterial;
        }

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = IcoSphereCreator.Create(numberOfSides, 1);
    }

    void Update()
    {
    // Start is called before the first frame update
        this._material.SetFloat("_ParticleCount", this._starfield._particleCount);
        this._material.SetBuffer("_SrcParticleBuffer", this._starfield._SrcParticleBuffer);
    }
}
