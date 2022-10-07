using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.XR;

public class InputController : MonoBehaviour
{

    public float grabForce = 10;
    public float _raiseHeight;
    public float _conveyorLevel;
    public LayerMask ignoreMeLayer;


    private Rigidbody _grabbedRB;
    private Vector3 _initialPos;

    public float _spawnTime = 0.1f;
    private float _spawnTimer;

    public Rigidbody LHandGrab, RHandGrab;

    Vector3 LStartGrabOffset;

    Vector3 RStartGrabOffset;

    Quaternion LStartGrabRotation;

    Quaternion RStartGrabRotation;

    public Grabber lhand, rhand;

    void Start()
    {

    }

    private void Update()
    {
        _spawnTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {

        bool GrabButton = UnityXRInputBridge.instance.GetButton(XRButtonMasks.triggerButton, XRHandSide.RightHand) || UnityXRInputBridge.instance.GetButton(XRButtonMasks.triggerButton, XRHandSide.LeftHand);
        bool LGrabButtonDown = UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.LeftHand);
        bool LGrabButtonUp = UnityXRInputBridge.instance.GetButtonUp(XRButtonMasks.triggerButton, XRHandSide.LeftHand);
        bool RGrabButtonDown = UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.RightHand);
        bool RGrabButtonUp = UnityXRInputBridge.instance.GetButtonUp(XRButtonMasks.triggerButton, XRHandSide.RightHand);

        if (!GameManager.INSTANCE.shiftStarted)
        {
            if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.RightHand) || UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.LeftHand))
            {
                GameManager.INSTANCE.StartShift();
            }
        }

        if (GameManager.INSTANCE.GAMEOVER)
        {
            if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.RightHand) || UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.LeftHand))
            {
                GameManager.INSTANCE.Restart();
            }
        }

        if (lhand.InteractObjects.Count > 0 && LGrabButtonDown)
        {
            while (lhand.InteractObjects[0] == null && lhand.InteractObjects.Count > 1)
            {
                lhand.InteractObjects.RemoveAt(0);
            }
            if (lhand.InteractObjects[0] != null)
            {
                LHandGrab = lhand.InteractObjects[0].GetComponent<Rigidbody>();
                LStartGrabOffset = lhand.transform.InverseTransformPoint(LHandGrab.transform.position);
                LStartGrabRotation = Quaternion.Inverse(lhand.transform.rotation) * LHandGrab.transform.rotation;
                lhand.ClearLists();
            }
            else
            {
                lhand.ClearLists();
            }
        }

        if (rhand.InteractObjects.Count > 0 && RGrabButtonDown)
        {
            while (rhand.InteractObjects[0] == null && rhand.InteractObjects.Count > 1)
            {
                rhand.InteractObjects.RemoveAt(0);
            }
            if (rhand.InteractObjects[0] != null)
            {
                RHandGrab = rhand.InteractObjects[0].GetComponent<Rigidbody>();
                RStartGrabOffset = rhand.transform.InverseTransformPoint(RHandGrab.transform.position);
                RStartGrabRotation = Quaternion.Inverse(rhand.transform.rotation) * RHandGrab.transform.rotation;
                rhand.ClearLists();
            }
            else
            {
                rhand.ClearLists();
            }
        }

        if (RGrabButtonUp && RHandGrab != null)
        {
            RHandGrab = null;
        }

        if (LGrabButtonUp && LHandGrab != null)
        {
            LHandGrab = null;
        }

        if (GrabButton && !GameManager.INSTANCE.GAMEOVER)
        {
            if (RHandGrab != null)
            {
                Vector3 PositionOffset = (rhand.transform.forward - rhand.transform.up * 2f).normalized * 0.01f;

                Vector3 DeltaPos = ((rhand.transform.TransformPoint(RStartGrabOffset) - RHandGrab.position) + PositionOffset) * Time.fixedDeltaTime * 2500f;

                RHandGrab.velocity = Vector3.MoveTowards(RHandGrab.velocity, DeltaPos, 10f);

                RHandGrab.maxAngularVelocity = 1000f;
                if (!RHandGrab.CompareTag("hinged"))
                {
                    Quaternion rotationDelta = (rhand.transform.rotation * RStartGrabRotation) * Quaternion.Inverse(RHandGrab.transform.rotation);
                    float angle;
                    Vector3 axis;
                    rotationDelta.ToAngleAxis(out angle, out axis);
                    if (angle > 180)
                    {
                        angle -= 360;
                    }

                    if (angle != 0)
                    {
                        Vector3 angularTarget = angle * axis;
                        if (float.IsNaN(angularTarget.x) == false)
                        {
                            angularTarget = (angularTarget * 100f) * Time.fixedDeltaTime;
                            angularTarget = Vector3.MoveTowards(RHandGrab.angularVelocity, angularTarget, 5f);
                            RHandGrab.angularVelocity = angularTarget;
                        }
                    }
                }
            }

            if (LHandGrab != null)
            {
                Vector3 PositionOffset = (lhand.transform.forward - lhand.transform.up * 2f).normalized * 0.01f;

                Vector3 DeltaPos = ((lhand.transform.TransformPoint(LStartGrabOffset) - LHandGrab.position) + PositionOffset) * Time.fixedDeltaTime * 2500f;

                LHandGrab.velocity = Vector3.MoveTowards(LHandGrab.velocity, DeltaPos, 10f);

                LHandGrab.maxAngularVelocity = 1000f;

                if (!LHandGrab.CompareTag("hinged"))
                {
                    Quaternion rotationDelta = (lhand.transform.rotation * LStartGrabRotation) * Quaternion.Inverse(LHandGrab.transform.rotation);
                    float angle;
                    Vector3 axis;
                    rotationDelta.ToAngleAxis(out angle, out axis);
                    if (angle > 180)
                    {
                        angle -= 360;
                    }

                    if (angle != 0)
                    {
                        Vector3 angularTarget = angle * axis;
                        if (float.IsNaN(angularTarget.x) == false)
                        {
                            angularTarget = (angularTarget * 100f) * Time.fixedDeltaTime;
                            angularTarget = Vector3.MoveTowards(LHandGrab.angularVelocity, angularTarget, 10f);
                            LHandGrab.angularVelocity = angularTarget;
                        }
                    }
                }
            }

            /*if (_spawnTimer > _spawnTime && rhand.StaticObjects.Count > 0)
            {
                if (rhand.StaticObjects[0].gameObject.tag == "button_box")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.NewBox();
                }
                if (rhand.StaticObjects[0].gameObject.tag == "button_tape")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.SpawnTape();
                }
                if (rhand.StaticObjects[0].gameObject.tag == "button_label")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.SpawnLabel();
                }

                _spawnTimer = 0;
            }

            if (_spawnTimer > _spawnTime && lhand.StaticObjects.Count > 0)
            {
                if (lhand.StaticObjects[0].gameObject.tag == "button_box")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.NewBox();
                }
                if (lhand.StaticObjects[0].gameObject.tag == "button_tape")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.SpawnTape();
                }
                if (lhand.StaticObjects[0].gameObject.tag == "button_label")
                {
                    GameManager.INSTANCE.audio.PlayClip(6);
                    GameManager.INSTANCE.SpawnLabel();
                }

                _spawnTimer = 0;
            }*/

        }

    }
}
