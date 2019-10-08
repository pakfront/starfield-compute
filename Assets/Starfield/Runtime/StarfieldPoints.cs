using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class StarfieldPoints : MonoBehaviour
{

    // ==============================
    #region // Defines
    public enum EMeshType { Point, Triangle, Quad };
    #endregion // Defines

    // --------------------------------------------------
    public EMeshType meshType = EMeshType.Point;
    public Material _material;

    #region // Serialize Fields
    #pragma warning disable 0649

    [SerializeField]
    Vector3 _BoundCenter = Vector3.zero;

    /// 表示領域のサイズ
    [SerializeField]
    Vector3 _BoundSize = new Vector3(300f, 300f, 300f);
#pragma warning restore 0649

    #endregion // Serialize Fields

    // --------------------------------------------------
    #region // Private Fields
        Starfield _starfield;

    /// GPU Instancingの為の引数
    uint[] _GPUInstancingArgs = new uint[5] { 0, 0, 0, 0, 0 };

    /// GPU Instancingの為の引数バッファ
    ComputeBuffer _GPUInstancingArgsBuffer;

    // point for particle
    Mesh _pointMesh;
    #endregion // Private Fields

    // --------------------------------------------------
    #region // MonoBehaviour Methods

    void Awake()
    {
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {

        this._starfield = GetComponent<Starfield>();

        this._GPUInstancingArgsBuffer = new ComputeBuffer(1, this._GPUInstancingArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        _pointMesh = new Mesh();
        switch (meshType)
        {
            case EMeshType.Point:
                _pointMesh.vertices = new Vector3[] {
                    new Vector3 (0, 0),
                };
                _pointMesh.normals = new Vector3[] {
                    new Vector3 (0, 1, 0),
                };
                _pointMesh.SetIndices(new int[] { 0 }, MeshTopology.Points, 0);
                break;

            default:
                _pointMesh.vertices = new Vector3[] {
                new Vector3 (0, 0),
                new Vector3 (0, 1),
                new Vector3 (1, 0)
                };
                _pointMesh.normals = new Vector3[] {
                new Vector3 (0, 1, 0),
                new Vector3 (0, 1, 0),
                new Vector3 (0, 1, 0)
                };
                _pointMesh.SetIndices(new int[] { 0, 1, 2 }, MeshTopology.Triangles, 0);
                break;
        }

        Debug.Log( " StarfieldPoints Create _pointMesh ");
    }

    void Update()
    {

        // GPU Instancing - requires special vertex shader
        this._GPUInstancingArgs[0] = (this._pointMesh != null) ? this._pointMesh.GetIndexCount(0) : 0;
        this._GPUInstancingArgs[1] = (uint)this._starfield._particleCount;
        this._GPUInstancingArgsBuffer.SetData(this._GPUInstancingArgs);
        this._material.SetBuffer("_SrcParticleBuffer", this._starfield._SrcParticleBuffer);
        // this._material.SetVector("_MeshScale", this._MeshScale);

        Graphics.DrawMeshInstancedIndirect(
            this._pointMesh, 0, this._material,
            new Bounds(this._BoundCenter, this._BoundSize),
            this._GPUInstancingArgsBuffer);
    }

    void OnDestroy()
    {
        if (this._GPUInstancingArgsBuffer != null)
        {
            this._GPUInstancingArgsBuffer.Release();
            this._GPUInstancingArgsBuffer = null;
        }
    }

    #endregion // MonoBehaviour Method
}
