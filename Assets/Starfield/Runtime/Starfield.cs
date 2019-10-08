using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Starfield : MonoBehaviour
{

    // ==============================
    #region // Defines

    const int ThreadBlockSize = 256;

    struct Particle
    {
        public Vector3 CurrentPosition;
        public Vector3 OldPosition;
        public Vector3 Velocity;
        public Vector3 Color;
        public float Scale;

    }

    struct Attractor
    {
        public Vector3 Position;
        public Vector3 Destination;
        public Vector3 Color;
        public float Velocity;
        public float Strength;
        public float MinAttractorDistance;
    }
#endregion
    public ComputeBuffer _SrcParticleBuffer;
    public ComputeBuffer _AttractorsBuffer;
    public int _particleCount = 1000000;
    public int _attractorCount = 4;
    public float attractorStrength = 1;
    public float timeStep = 0.01f;


    #region 
    #pragma warning disable 0649

    [SerializeField]
    public ComputeShader _ComputeShader;

    #pragma warning restore 0649
    #endregion // Serialize Fields



    // point for particle
    #region // Private Fields
    int kernelId;
    Attractor[] _attractorDataArr;
    #endregion // Private Fields

    // --------------------------------------------------
    #region // MonoBehaviour Methods


    void Start()
    {

        this._SrcParticleBuffer = new ComputeBuffer(this._particleCount, Marshal.SizeOf(typeof(Particle)));
        var particleDataArr = new Particle[this._particleCount];
        // set default position
        for (int i = 0; i < _particleCount; i++)
        {
            particleDataArr[i].CurrentPosition = Random.insideUnitSphere; //new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 10.0f));
            particleDataArr[i].OldPosition = particleDataArr[i].CurrentPosition;
            particleDataArr[i].Velocity = Vector3.zero;
            Color color = Random.ColorHSV(0f, 1f, .1f, .2f, .5f,1f);
            particleDataArr[i].Color = new Vector3(color.r, color.g, color.b) * Random.Range(1,4);
            particleDataArr[i].Scale = Random.Range(1, 4);
        }
        this._SrcParticleBuffer.SetData(particleDataArr);
        particleDataArr = null;

        this._AttractorsBuffer = new ComputeBuffer(this._attractorCount, Marshal.SizeOf(typeof(Attractor)));
        this._attractorDataArr = new Attractor[this._attractorCount];
        // set default position
        for (int i = 0; i < _attractorCount; i++)
        {
            _attractorDataArr[i].Position = Random.insideUnitSphere; //new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));
            _attractorDataArr[i].Destination = Random.insideUnitSphere; //new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));
            _attractorDataArr[i].Color = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            _attractorDataArr[i].Velocity = Random.Range(0.1f, 1);
            _attractorDataArr[i].Strength = Random.Range(0.1f, 1) * attractorStrength;
            _attractorDataArr[i].MinAttractorDistance = Random.Range(0.001f, 0.01f);
        }
        this._AttractorsBuffer.SetData(_attractorDataArr);

        kernelId = this._ComputeShader.FindKernel("CSMain");

        Debug.Log("Starfield: Particles:" + _SrcParticleBuffer.count + " Attractors:" + _AttractorsBuffer.count);
    }

    void Update()
    {
        // update on cpu unless we have a lot of attractors
        for (int i = 0; i < _attractorCount; i++)
        {
            _attractorDataArr[i].Position = Vector3.MoveTowards(_attractorDataArr[i].Position, _attractorDataArr[i].Destination, timeStep * _attractorDataArr[i].Velocity);

        }
        this._AttractorsBuffer.SetData(_attractorDataArr);

        // ComputeShader
        if (timeStep > 0)
        {
            this._ComputeShader.SetFloat("_timeStep", timeStep);
            this._ComputeShader.SetInt("_attractorCount", this._attractorCount);
            this._ComputeShader.SetBuffer(kernelId, "_SrcParticleBuffer", this._SrcParticleBuffer);
            this._ComputeShader.SetBuffer(kernelId, "_AttractorsBuffer", this._AttractorsBuffer);
            this._ComputeShader.Dispatch(kernelId, (Mathf.CeilToInt(this._particleCount / ThreadBlockSize) + 1), 1, 1);
        }
    }

    void OnDestroy()
    {
        if (this._SrcParticleBuffer != null)
        {
            this._SrcParticleBuffer.Release();
            this._SrcParticleBuffer = null;
        }
        if (this._AttractorsBuffer != null)
        {
            this._AttractorsBuffer.Release();
            this._AttractorsBuffer = null;
        }
    }

    #endregion // MonoBehaviour Method
}
