using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class ARPlacementController : MonoBehaviour
{
    public ARPlacementInteractable ArPlacementInteractable;

    private bool _objectPlaced = false;
  
    public void AllowPlacement()
    {
        if (_objectPlaced) return;
        ArPlacementInteractable.enabled = true;
        _objectPlaced = true;
    }
   
    
}
