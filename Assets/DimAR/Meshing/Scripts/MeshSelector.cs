using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MeshSelector : MonoBehaviour
    {
        public ARRaycastManager m_RaycastManager;
        public Color defaultColor = Color.cyan;
        public Color selectedColor = Color.red;
        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        private Vector3 _current;
        private Camera _mCam;
        public ARMeshManager m_MeshManager;  
        public TextureGenerator TextureGenerator;
        public Material inpaintMaterial, overlayMaterial;
        private bool _doingInpaint;
        private bool _alloMeshselection = true;
        public List<GameObject> selectedSubGameObjects;
        private GameObject _mergedObject;
        private void Start()
        { 
            _mCam = Camera.main; 
            _doingInpaint = false;
        }
  
        private void OnEnable()
        {
            m_MeshManager.meshesChanged += DeactivateAllOtherAddedTrackables;
        }

        private void OnDisable()
        {
            m_MeshManager.meshesChanged -= DeactivateAllOtherAddedTrackables;
        }

        /******
         *
         * if mesh selected -> set overlayMaterial pulse to zero and color to selected
         * if selected another mesh -> reset selected material back to overlay material pulsing and default color and set material of new mesh to pulse zero and color selected
         * if selected same mesh -> reset selected material of mesh back to overlay pulsing and default color
         *
         * if mesh selected and pressing inpaint
         *  set material to inpainting material
         *  do inpainting and deactivate all other meshes, doinginpaint true
         *
         * if then select the same mesh again reset to overlay material and set pulsing zero and selected color, doinginpaint false
         *  or select another mesh reset all materials and go for selected mesh with pulsing zero and selected color
         *********/
        private void Update()
        { 
            //If no touch detected skipp or MeshSlection through UI is false
            if (Input.touchCount == 0 || !_alloMeshselection) return;
             
            //if touch detected
            var touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
            {
                //if touch is on UI skip
                if(EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return; 
                //define that inpainting is not in progress
                _doingInpaint = false;
                //calculate hit
                RaycastHit hit;
                var ray = _mCam.ScreenPointToRay(touch.position); 
                if (Physics.Raycast(ray, out hit, 10f))
                {
                    TextureGenerator.loopInpaint = false;
                    MeshRenderer hittedObjectMeshRenderer = hit.collider.gameObject.GetComponent<MeshRenderer>(); 
                    Destroy(_mergedObject);
                    ActivateAllExistingTrackables();
                    if (selectedSubGameObjects.Contains(hit.collider.gameObject))
                    {
                        selectedSubGameObjects.Remove(hit.collider.gameObject);  
                        hittedObjectMeshRenderer.material = overlayMaterial;
                        hittedObjectMeshRenderer.material.SetColor("_Color", defaultColor);
                        hittedObjectMeshRenderer.material.SetFloat("_Pulse",1);
                    }
                    else
                    {
                        selectedSubGameObjects.Add(hit.collider.gameObject);
                        // none selected previous so set new as selected 
                        hittedObjectMeshRenderer.material.SetColor("_Color", selectedColor); 
                        hittedObjectMeshRenderer.material.SetFloat("_Pulse",0);
                    } 
                }
                else
                {
                    Debug.Log("Hit nothing");
                }
            }
        }
 
        public void DoMeshingInPaint()
        {
            if(selectedSubGameObjects.Count == 0) return;
            _mergedObject = Merge(selectedSubGameObjects, "inpaintTarget");
            _mergedObject.layer = 10;
            _mergedObject.GetComponent<MeshRenderer>().material = inpaintMaterial;
            TextureGenerator.mr = _mergedObject.GetComponent<MeshRenderer>();
            TextureGenerator.drawnMesh = _mergedObject.GetComponent<MeshFilter>();
            TextureGenerator.lidarInpaint = true; 
            DeactivateAllOtherExistingTrackables();
            StartCoroutine(TextureGenerator.BlitTextures());
            TextureGenerator.loopInpaint = true;
            ResetVisualOfAllExistingTrackables();
            selectedSubGameObjects.Clear();
            _doingInpaint = true;
        }

        public void DeactivateAllOtherExistingTrackables()
        {
          //  MeshFilter selectedMeshFilter = selectedRayObject.GetComponent<MeshFilter>();
            foreach (var meshFilter in m_MeshManager.meshes)
            { 
                    meshFilter.GetComponent<MeshRenderer>().enabled = false; 
            }
        }

        public void ActivateAllExistingTrackables()
        { 
            foreach (var meshFilter in m_MeshManager.meshes)
            { 
                    meshFilter.GetComponent<MeshRenderer>().enabled = true; 
            }
        }
        
        public void ResetVisualOfAllExistingTrackables()
        { 
            foreach (var meshFilter in m_MeshManager.meshes)
            { 
                meshFilter.GetComponent<MeshRenderer>().material = overlayMaterial;
                meshFilter.GetComponent<MeshRenderer>().material.SetColor("_Color", defaultColor);
                meshFilter.GetComponent<MeshRenderer>().material.SetFloat("_Pulse",1);
            }
        }
        
        private void DeactivateAllOtherAddedTrackables(ARMeshesChangedEventArgs arg)
        { 
            if (!_doingInpaint) return;
            foreach (MeshFilter v in arg.added)
            { 
                    v.GetComponent<MeshRenderer>().enabled = false; 
            }
        }

        public void AllowMeshSelection(bool val)
        {
            _alloMeshselection = val;
        }

        GameObject Merge(List<GameObject> objects, string nameNewMesh, bool disbaleOldMeshes = true)
        {
            CombineInstance[] combine = new CombineInstance[objects.Count];
            MeshFilter meshFilter;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject o2m = objects[i];
                meshFilter = o2m.GetComponent<MeshFilter>();
                combine[i].mesh = meshFilter.sharedMesh;
                combine[i].transform = meshFilter.transform.localToWorldMatrix;
                o2m.SetActive(!disbaleOldMeshes);
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);
            GameObject gameObject = new GameObject(nameNewMesh);
            meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Diffuse"));
            meshFilter.sharedMesh = mesh;
            return gameObject;
        }
    }
