using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DrawPolygonOnPlane : MonoBehaviour
{
    private Camera _mCam;

    //Previous and current Vector for comparison
    private Vector2 _previous = Vector2.zero;

    public Vector2 _current;

    //List all Position for drawing
    public List<Vector3> _positions;

    //LineRenderer
    public LineRenderer lineRender;

    //Threshold for min. moving distance for next point
    public float threshold = 0.5f;

    //Vectors for triangulation
    private Vector2[] vertices2D;

    public MeshRenderer mr;
    public MeshFilter mf;

    [SerializeField] private TextureGenerator textGen;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    public ARRaycastManager m_RaycastManager;

    public Vector2[] uv;

    public bool isMeshdrawingAllowed = true;

    // Start is called before the first frame update
    private void Start()
    {
        lineRender = GetComponent<LineRenderer>();

        _mCam = Camera.main;
        if (m_RaycastManager == null)
        {
            Debug.LogWarning("Raycast Manager is not set.");
        }

        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
#if UNITY_EDITOR
        if (_positions.Count > 0)
        {
            // GenerateMesh();
        }
#endif
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isMeshdrawingAllowed) return;
#if UNITY_EDITOR
        //Handle Mouse Input
        if (Input.GetMouseButtonDown(0) && lineRender.positionCount > 0 &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            mr.enabled = false;
            textGen.loopInpaint = false;
            ResetLineRenderer();
        }

        if (Input.GetKey(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            _current = (new Vector3(Input.mousePosition.x, Input.mousePosition.y, _mCam.nearClipPlane));
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (lineRender.positionCount > 3)
            {
                GenerateMesh();
                textGen.loopInpaint = true;
                textGen.GenerateTextures();
                mr.enabled = true;
            }
        }

#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(0)) return;
            // Handle finger movements based on TouchPhase
            switch (touch.phase)
            {
                //When a touch has first been detected, change the message and record the starting position
                case TouchPhase.Began:
                    // Record initial touch position.
                    if (lineRender.positionCount > 0 && !EventSystem.current.IsPointerOverGameObject())
                    {
                        mr.enabled = false;
                        ResetLineRenderer();
                    }
                    break;

                //Determine if the touch is a moving touch
                case TouchPhase.Moved:
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }

                    _current = touch.position;
                    break;

                case TouchPhase.Ended:
                    // Report that the touch has ended when it ends
                    if (lineRender.positionCount > 3)
                    {
                        GenerateMesh();
                        textGen.loopInpaint = true;
                        textGen.GenerateTextures();
                        mr.enabled = true;
                    }

                    break;
            }
        }

#endif


//Handle Threshold for not overfilling Array
        if (Vector3.Distance(_previous, _current) < threshold)
        {
            return;
        }

        //Check if position is on tracking plane

        if (m_RaycastManager.Raycast(_current, s_Hits, TrackableType.PlaneWithinInfinity))
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            // Debug.Log("true");

            _positions.Add(s_Hits[0].pose.position);
        else
            Debug.Log("Hit not on Plane");

        //Fill Line Renderer with Touch Position
        lineRender.positionCount = _positions.Count;
        for (var i = 0; i < _positions.Count; i++) lineRender.SetPosition(i, _positions[i]);
        _previous = _current;

        //Convert World Position to Tracked Plane
    }

    private Vector2[] ConvertTo2DVectorArray(Vector3[] _3dVector)
    {
        Vector2[] result = new Vector2[_3dVector.Length];
        for (int i = 0; i < _3dVector.Length; i++)
        {
            result[i] = new Vector2(_3dVector[i].x, _3dVector[i].z);
        }

        return result;
    }

    private void GenerateMesh()
    {
        //   if (mf.mesh != null) return;

        Debug.Log("Generate Mesh..");
        // Use the triangulator to get indices for creating triangles
        vertices2D = ConvertTo2DVectorArray(_positions.ToArray());
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = _positions.ToArray();

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;

        msh.uv = BuildUVs(vertices);

        msh.RecalculateNormals();
        msh.RecalculateBounds();
        mr.receiveShadows = false;
        mr.shadowCastingMode = ShadowCastingMode.Off;

        // Set up game object with mesh;
        mf.mesh = msh;
        // calculate the current model view projection matrix
        // Matrix4x4 MVP = GL.GetGPUProjectionMatrix(_mCam.projectionMatrix, false) * _mCam.worldToCameraMatrix * transform.GetComponent<Renderer>().worldToLocalMatrix;

        // Debug.Log(worldPivot.x + " " + worldPivot.y + " " + worldPivot.z);
        //  textGen._renderer.material.SetVector("_MeshPivot", worldPivot);
    }

    //Reset LineRenderer
    private void ResetLineRenderer()
    {
        lineRender.positionCount = 0;
        _positions.Clear();
        _current = Vector3.zero;
        _previous = Vector3.zero;
    }


    public void UpdateTexture(Camera camera, Mesh msh)
    {
        Matrix4x4 vp = camera.projectionMatrix * camera.worldToCameraMatrix;

        int size = msh.vertices.Length;
        Vector2[] uvs = new Vector2[size];
        for (int i = 0; i < size; i++)
        {
            uvs[i] = vertexToUVPosition(vp, msh, i);
            Debug.Log(uvs[i].x + " " + uvs[i].y);
        }

        msh.SetUVs(0, uvs);
    }

    private Vector2 vertexToUVPosition(Matrix4x4 vp, Mesh msh, int index)
    {
        Vector3 vertex = msh.vertices[index];
        Vector4 worldPos = new Vector4(vertex.x, vertex.y, vertex.z, 1f);
        Vector4 clipPos = vp * worldPos;
        // clipPos.Scale(new Vector3(1, 0.5f, 1));
        return clipPos;
    }


    Vector2[] BuildUVs(Vector3[] vertices)
    {
        List<Vector3> normVertices = new List<Vector3>();
        float width = Screen.width;
        float height = Screen.height;
        foreach (var vec in vertices)
        {
            normVertices.Add(new Vector3(vec.x / width, vec.y / height, vec.z));
        }

        float xMin = 0;
        float yMin = 0;
        float xMax = width;
        float yMax = height;

        foreach (Vector3 v3 in normVertices)
        {
            if (v3.x < xMin)
                xMin = v3.x;
            if (v3.z < yMin)
                yMin = v3.z;
            if (v3.x > xMax)
                xMax = v3.x;
            if (v3.z > yMax)
                yMax = v3.z;
        }

        float xRange = xMax - xMin;
        float yRange = yMax - yMin;

        Vector2[] uvs = new Vector2[normVertices.Count];
        for (int i = 0; i < normVertices.Count; i++)
        {
            uvs[i].x = (normVertices[i].x - xMin) / xRange;
            uvs[i].y = (normVertices[i].z - yMin) / yRange;
            //  Debug.Log(uvs[i].x + " " + uvs[i].y);
        }

        return uvs;
    }

    public void SetMeshDrawingAllowed(bool val)
    {
        isMeshdrawingAllowed = val;
    }
}