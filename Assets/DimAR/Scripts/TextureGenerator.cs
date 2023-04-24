using System;
using System.Collections;
using System.Collections.Generic;
using GRT;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;

public class TextureGenerator : MonoBehaviour
{
    //Render Textures
    public RenderTexture backgroundCamInput, maskCamRT;
    public Texture2D finalMask2D, _background2DImage, inpaintresult;

    //Mesh Components
    public MeshFilter drawnMesh;
    public MeshRenderer mr;

    public SkinnedMeshRenderer skinnedMeshRenderer;

    //Cameras
    public Camera mainCamera, MaskCamera;

    //Stencil Shader
    public Shader StencilShader;

    //BackgroundMaterial
    public ARCameraBackground _arCameraBackground;

    //Exposure Controller
    public CameraExposureController _cameraExposureController;
    //Debug purpose
    public bool useDebugMonitoring;
    public RawImage riCaminput, maskTextureRawImage, riResult;
    int _debugInteration = 0;
    public List<float> _timings;

    //Debug Time Measure
    private float startTime;
    public Text averageTime;

    public int TextureDivider = 1;
    private bool firstTextureInit = false;
    private bool textureInitDone = false;
    private bool inpaintDone;
    public bool loopInpaint = false;
    public bool lidarInpaint = false;
    public bool humanInpaint = false;
    public bool useImageTrackingInpaint = false;
    public bool usePoissonBlending = false;



    Matrix4x4 _projectionMatrix = Matrix4x4.identity;

    private string _prefix = "";
    private string _data = "";

    //Poisson Blending
    public PoissonBlending.PoissonBlending _poisson;

    // Start is called before the first frame update
    void Start()
    {
        InitRenderTextures();
        if (!lidarInpaint && !useImageTrackingInpaint)
        {
            drawnMesh = GameObject.FindGameObjectWithTag("Drawer").GetComponent<MeshFilter>();
            mr = drawnMesh.GetComponent<MeshRenderer>();
        }
        if(_cameraExposureController.isActiveAndEnabled) _cameraExposureController.SetCameraLock(false);

        MaskCamera = GameObject.FindGameObjectWithTag("MaskCam").GetComponent<Camera>();
        MaskCamera.depthTextureMode = DepthTextureMode.Depth;


        MaskCamera.SetReplacementShader(StencilShader, "");
        MaskCamera.targetTexture = maskCamRT;
        MaskCamera.Render();
        _arCameraBackground = mainCamera.GetComponent<ARCameraBackground>();
        BaseGRTClient.Instance.Connect();
        RequestHelper.InPaintData connect_data = new RequestHelper.InPaintData();
        connect_data.raw_data = "handshake:Connecting To server==";
        connect_data.request = RequestHelper.Request.Connect;
        BaseGRTClient.Instance.Send(ref connect_data);

        if (useDebugMonitoring) FillDebugPanels();

        GenerateTextures();
        if (!lidarInpaint && !useImageTrackingInpaint) mr.sharedMaterial.SetTexture("_MainTex", inpaintresult);
        _poisson = GetComponent<PoissonBlending.PoissonBlending>();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        RequestHelper.InPaintData connect_data = new RequestHelper.InPaintData();
        connect_data.raw_data = "close:EndConnection==";
        connect_data.request = RequestHelper.Request.Connect;
        BaseGRTClient.Instance.Send(ref connect_data);
        BaseGRTClient.Instance.RequestReceived -= HandleReceiveResult;
    }


    private void OnApplicationQuit()
    {
        inpaintresult = Texture2D.whiteTexture;
        inpaintresult.Apply();

        RequestHelper.InPaintData connect_data = new RequestHelper.InPaintData();
        connect_data.raw_data = "close:EndConnection==";
        connect_data.request = RequestHelper.Request.Connect;
        BaseGRTClient.Instance.Send(ref connect_data);
        BaseGRTClient.Instance.RequestReceived -= HandleReceiveResult;
        BaseGRTClient.Instance.StopServer();
    }

    private void OnEnable()
    {
        BaseGRTClient.Instance.RequestReceived += HandleReceiveResult;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        if (PlayerPrefs.HasKey("texturequality"))
        {
            TextureDivider = PlayerPrefs.GetInt("texturequality");
            if (TextureDivider == 0) TextureDivider = 1;
        }

        Debug.Log("Tex Qaulity " + TextureDivider);
    }

    private void OnDisable()
    {
        //arCamManager.frameReceived -= AddColorCorrection;

        ShowAverageTimings();
    }

    private void ShowAverageTimings()
    {
        float _averageTiming = 0;
        foreach (var stamp in _timings)
        {
            _averageTiming += stamp;
        }

        Debug.Log("Average Timing in Seconds is " + (_averageTiming / _debugInteration) + " with a divider of " +
                  TextureDivider + " and " + _debugInteration + " iterations in total.");
    }

    private void HandleReceiveResult(RequestHelper.InPaintData data)
    {
        _prefix = "";
        _data = "";
        try
        {
            _prefix = data.raw_data.Split(":")[0];
            _data = data.raw_data.Split(":")[1];  
            //Debug.Log("Result received" +data.raw_data);
            switch (_prefix)
            {
                case "handshake":
                    data.request = RequestHelper.Request.Connect;
                   // Debug.Log(">>>>> Client connected successful.");
                    _prefix = "";
                    _data = "";
                    break;
                case "background":
                    _cameraExposureController.SetCameraLock(true);
                    data.request = RequestHelper.Request.SendBackground;
                    
                    SafeMatrixProjection();
                   // Debug.Log("Background was sucessful send, sending Mask");
                    SendMask();
                    _prefix = "";
                    _data = "";
                    break;
                case "mask":
                    //Debug.Log("Mask was sucessful send - waiting for inpaint result");
                    data.request = RequestHelper.Request.SendMask;
                    //Debug.Log("Mask was sucessful send, wait for result");
                    _prefix = "";
                    _data = "";
                    break;
                case "inpaint":
                   // Debug.Log("RESULT RECEIVED");
                    //data.request = RequestHelper.Request.GetInpaintedImage;
                    inpaintresult.LoadImage(Convert.FromBase64String(_data));
                    SetMatrixProjection();
                    if (usePoissonBlending)
                    {
                        FillPoisson();
                    }

                    _debugInteration++;

                    _timings.Add(Time.time - startTime);

                    ShowAverageTimings();
                    if (mr.enabled && loopInpaint)
                    {
                        GenerateTextures();
                    }

                    inpaintDone = true;
                    _prefix = "";
                    _data = "";
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e + data.raw_data);
        }
    }

    private void AddColorCorrection(ARCameraFrameEventArgs args)
    {
        if (usePoissonBlending)
        {
            ARCameraBackgroundToTexture();
            MaskToTexture();
        }

        if (inpaintDone)
        {
            if (usePoissonBlending) _poisson.DoPoissonBlending();


            // calculate the current model view projection matrix
            var MVP = Matrix4x4.identity;
            MVP = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false) * mainCamera.worldToCameraMatrix *
                  mr.worldToLocalMatrix;

            // pass the matrix to the shader by setting it on the material
            mr.material.SetMatrix("_MyProjectionMatrix", MVP);
        }
    }

    void FillPoisson()
    {
        _poisson.mask = finalMask2D;
        _poisson.source = inpaintresult;
        _poisson.target = _background2DImage;
        _poisson.DoPoissonBlending();
        mr.sharedMaterial.SetTexture("_MainTex", _poisson.tex);
    }

    private void FillDebugPanels()
    {
        riCaminput.enabled = true;
        riCaminput.texture = _background2DImage;

        maskTextureRawImage.enabled = true;
        maskTextureRawImage.texture = finalMask2D;

        riResult.enabled = true;
        riResult.texture = inpaintresult;
    }

    void InitRenderTextures()
    {
        backgroundCamInput = new RenderTexture(Screen.width / TextureDivider, Screen.height / TextureDivider, 16);
        backgroundCamInput.name = "BackgroundInput";
        backgroundCamInput.enableRandomWrite = true;

        _background2DImage = new Texture2D(Screen.width / TextureDivider, Screen.height / TextureDivider,
            TextureFormat.RGB24, 1, false);
        _background2DImage.name = "background2DImage";

        maskCamRT = new RenderTexture(Screen.width / TextureDivider, Screen.height / TextureDivider, 16);
        maskCamRT.name = "maskCamRT";
        maskCamRT.enableRandomWrite = true;

        finalMask2D = new Texture2D(Screen.width / TextureDivider, Screen.height / TextureDivider,
            TextureFormat.RGB24, 1, false);
        finalMask2D.name = "MaskTexture2D";

        inpaintresult = new Texture2D(Screen.width / TextureDivider, Screen.height / TextureDivider,
            TextureFormat.RGB24, false);
        inpaintresult.wrapMode = TextureWrapMode.Clamp;
        inpaintresult.name = "inpaintresult";

        textureInitDone = true;
    }


    public void GenerateTextures()
    {
        if (textureInitDone)
        {
            inpaintDone = false;
            StartCoroutine(BlitTextures());
            if (!lidarInpaint && !useImageTrackingInpaint) SetMatrixProjection();
            if (skinnedMeshRenderer != null) skinnedMeshRenderer.material.mainTexture = inpaintresult;
        }
        else
        {
            InitRenderTextures();
        }
    }

    public void SendMask()
    { 
        RequestHelper.InPaintData mask_data = new RequestHelper.InPaintData();
        mask_data.raw_data = "mask:" + Convert.ToBase64String(finalMask2D.EncodeToPNG());
        mask_data.request = RequestHelper.Request.SendMask;
        BaseGRTClient.Instance.Send(ref mask_data);
    }

    public void SendBackground()
    {
        
        RequestHelper.InPaintData background_data = new RequestHelper.InPaintData();
        background_data.raw_data =
            "background:" + Convert.ToBase64String(_background2DImage.EncodeToJPG()) + "==";
        background_data.request = RequestHelper.Request.SendBackground;
        BaseGRTClient.Instance.Send(ref background_data);
    }

    private void ARCameraBackgroundToTexture()
    {
        //BackgroundTexture 
        RenderTexture.active = backgroundCamInput;
        backgroundCamInput.enableRandomWrite = true;
        Graphics.Blit(null, backgroundCamInput, _arCameraBackground.material);
        _background2DImage.ReadPixels(new Rect(0, 0, backgroundCamInput.width, backgroundCamInput.height), 0, 0);
        Graphics.CopyTexture(backgroundCamInput, _background2DImage);
    }

    private void MaskToTexture()
    {
        //Mask RenderTexture
        RenderTexture.active = maskCamRT;
        maskCamRT.enableRandomWrite = true;
        finalMask2D.ReadPixels(new Rect(0, 0, maskCamRT.width, maskCamRT.height), 0, 0);
        Graphics.CopyTexture(maskCamRT, finalMask2D);
    }

    public void SafeMatrixProjection()
    {
        if (lidarInpaint)
        {
            // calculate the current model view projection matrix with created lidar mesh from local to world
            _projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false) *
                                mainCamera.worldToCameraMatrix *
                                mr.localToWorldMatrix;
        }
        else if (humanInpaint)
        {
            // calculate the current model view projection matrix with skinnedMeshRenderer from humanoid tracking
            _projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false) *
                                mainCamera.worldToCameraMatrix *
                                skinnedMeshRenderer.localToWorldMatrix;
        }
        else if(useImageTrackingInpaint)
        {
            _projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false) *
                                mainCamera.worldToCameraMatrix *
                                mr.localToWorldMatrix; 
        }
        else
        {
            // calculate the current model view projection matrix from default drawn plane world to local
            _projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false) *
                                mainCamera.worldToCameraMatrix *
                                mr.worldToLocalMatrix;
        }
    }

    public void SetMatrixProjection()
    {
        // pass the matrix to the shader by setting it on the material
        mr.material.SetMatrix("_ProjectionMatrix", _projectionMatrix);
        //Debug.Log("setting matrix to " + mr.name);
        if (skinnedMeshRenderer)
        {
            skinnedMeshRenderer.material.SetMatrix("_ProjectionMatrix", _projectionMatrix);
        }
    }

    public IEnumerator BlitTextures()
    {
        yield return null;
        startTime = Time.time;
        ARCameraBackgroundToTexture();
        MaskToTexture();
        if (skinnedMeshRenderer != null) skinnedMeshRenderer.material.mainTexture = inpaintresult;
        /*
         * Send Background and Mask to NN after first init
         */
        if (firstTextureInit)
        {
            mr.sharedMaterial.SetTexture("_MainTex", inpaintresult);
            SendBackground();
        }

        firstTextureInit = true;
        yield return null;
    }
}