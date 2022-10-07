using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.XR;

public class Grabber : MonoBehaviour
{

    public List<GameObject> InteractObjects = new List<GameObject>();

    public List<GameObject> StaticObjects = new List<GameObject>();

    public float _spawnTimer;
    public float _spawnTime = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("nograb"))
        {
            return;
        }
        if (other.gameObject.tag == "grab" || other.gameObject.tag == "boxtape" || other.gameObject.tag == "boxlabel")
        {
            if (!InteractObjects.Contains(other.attachedRigidbody.gameObject))
            {
                InteractObjects.Add(other.attachedRigidbody.gameObject);
            }
            if (transform.parent.name.Contains("L"))
            {
                UnityXRInputBridge.instance.SetHaptics(0.5f, XRHandSide.LeftHand);
            }
            else
            {
                UnityXRInputBridge.instance.SetHaptics(0.5f, XRHandSide.RightHand);
            }
        }
        else if (other.gameObject.tag == "button_box" || other.gameObject.tag == "button_tape" || other.gameObject.tag == "button_label")
        {
            if (_spawnTimer > _spawnTime)
            {
                if (other.gameObject.tag == "button_box")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.NewBox();
                }
                if (other.gameObject.tag == "button_tape")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.SpawnTape();
                }
                if (other.gameObject.tag == "button_label")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.SpawnLabel();
                }

                _spawnTimer = 0;
            }
        }
    }

    void Update()
    {
        _spawnTimer += Time.deltaTime;
    }

    //Clear them to make sure trigger exit doesn't miss one
    public void ClearLists()
    {
        StaticObjects.Clear();
        InteractObjects.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "button_box" || other.gameObject.tag == "button_tape" || other.gameObject.tag == "button_label")
        {
            if (transform.parent.name.Contains("L"))
            {
                UnityXRInputBridge.instance.SetHaptics(0.1f, XRHandSide.LeftHand);
            }
            else
            {
                UnityXRInputBridge.instance.SetHaptics(0.1f, XRHandSide.RightHand);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "grab" || other.gameObject.tag == "boxtape" || other.gameObject.tag == "boxlabel")
        {
            if (InteractObjects.Contains(other.attachedRigidbody.gameObject))
            {
                InteractObjects.Remove(other.attachedRigidbody.gameObject);
            }
        }
    }
}
