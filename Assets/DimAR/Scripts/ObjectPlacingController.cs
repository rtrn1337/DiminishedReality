using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class ObjectPlacingController : MonoBehaviour
{
  public ARPlacementInteractable arPlacementInteractable;
  private bool _objectPlaced;

  private ARObjectPlacementEvent objectPlacedEvent = new ARObjectPlacementEvent();
  private void OnEnable()
  {
    arPlacementInteractable.objectPlaced.AddListener(ObjectPlaced);
  }


  public void EnablePlacement()
  {
    //prevent placeing objects several times 
    if(_objectPlaced) return;
    StartCoroutine(ActivatePlacingAfterNextFrame());
  }

  private void ObjectPlaced(ARObjectPlacementEventArgs args)
  {
    Debug.Log("Asset Placed");
    _objectPlaced = true;
  }
  public void DisablePlacement()
  {
    arPlacementInteractable.enabled = false;
  }

  IEnumerator ActivatePlacingAfterNextFrame()
  {
    yield return new WaitForSecondsRealtime(1); 
    arPlacementInteractable.enabled = true;
  
  }
  
}
