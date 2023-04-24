using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation; 

public class ImageTrackingInpaint : MonoBehaviour
    {
       
        public TextureGenerator TextureGenerator;
        public ARTrackedImageManager m_ARTrackedImageManager;
 
  
        private void OnEnable()
        {
            m_ARTrackedImageManager.trackedImagesChanged += DoMarkerInpaint;
        }

        private void OnDisable()
        { 
            m_ARTrackedImageManager.trackedImagesChanged -= DoMarkerInpaint;
        }

        public void DoMarkerInpaint(ARTrackedImagesChangedEventArgs eventArgs)
        { 
           
            if (TextureGenerator.mr == null)
            { 
                foreach (var newImage in eventArgs.updated)
                {  
                    if (TextureGenerator.mr == null)
                    {
                        foreach (var trans in m_ARTrackedImageManager.trackables[newImage.trackableId].GetComponentsInChildren<Transform>())
                        { 
                            if (trans.transform.CompareTag("Drawer"))
                            {
                                TextureGenerator.drawnMesh = trans.GetComponent<MeshFilter>();
                                TextureGenerator.mr = trans.GetComponent<MeshRenderer>();
                            } 
                        }
                    } 
                    TextureGenerator.GenerateTextures(); 
                }

            
            }
        }
 
    }
