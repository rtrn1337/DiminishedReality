using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation; 

public class HumanInpaint : MonoBehaviour
    {
       
        public TextureGenerator TextureGenerator;
        public ARHumanBodyManager m_HumanBodyManager;
        private bool initDone = false;
  
        private void OnEnable()
        {
            m_HumanBodyManager.humanBodiesChanged += DoHumanInpaint;
        }

        private void OnDisable()
        { 
            m_HumanBodyManager.humanBodiesChanged -= DoHumanInpaint;
        }

        public void DoHumanInpaint(ARHumanBodiesChangedEventArgs eventArgs)
        { 
            if (TextureGenerator.skinnedMeshRenderer == null)
            {
                foreach (var humanBody in eventArgs.updated)
                { 
                    if(initDone) return; 
                    TextureGenerator.skinnedMeshRenderer = humanBody.GetComponentInChildren<SkinnedMeshRenderer>();
                    if(TextureGenerator.skinnedMeshRenderer == null) return; 
                    TextureGenerator.SafeMatrixProjection();
                    TextureGenerator.GenerateTextures();
                    initDone = true;
                }
            }
        }
 
    }
