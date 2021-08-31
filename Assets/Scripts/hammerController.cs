using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.UI;

public class hammerController : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject hammer;

    public XRController controllerLeft;
    public XRController controllerRight;
    public XRRayInteractor interactorLeft;
    public XRRayInteractor interactorRight;
    public bool rightPress;
    public bool leftPress;

    public Text debugText;

    private float magnetspeed;
    private float magnetmultiplier=1.1f;
    private float magnetminimum=2f;

    public List<Vector3> rightHoldPositions;

    public UnityEngine.XR.InputDevice lefty;
    public UnityEngine.XR.InputDevice righty;

    private Rigidbody hammerRB;
    private XRGrabInteractable hammerGrabScript;
    private float updateControllerTimer = 2f;
    private int releaseCounter = 0;
    private float distance;

    void updatecontroller()
    {
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
        lefty = leftHandDevices[0];
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
        righty = rightHandDevices[0];
    }

    // Start is called before the first frame update
    void Start()
    {
        controllerLeft = leftHand.GetComponent<XRController>();
        controllerRight = rightHand.GetComponent<XRController>();
        interactorLeft = leftHand.GetComponent<XRRayInteractor>();
        interactorRight = rightHand.GetComponent<XRRayInteractor>();
        hammerRB = hammer.GetComponent<Rigidbody>();
        hammerGrabScript = hammer.GetComponent<XRGrabInteractable>();
        updatecontroller();
        rightHoldPositions = new List<Vector3>();
    }
    // Update is called once per frame
    void Update()
    {
        lefty.IsPressed(InputHelpers.Button.Trigger,out leftPress);
        righty.IsPressed(InputHelpers.Button.Trigger, out rightPress);
        //lefty.IsPressed(InputHelpers.Button.PrimaryButton, out leftPress);
        //righty.IsPressed(InputHelpers.Button.PrimaryButton, out rightPress);
        //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller & InputDeviceCharacteristics.TrackedDevice, _inputDevices);
        if ( rightPress )
        { // press
            distance = Vector3.Distance(hammer.transform.position, rightHand.transform.position);
            if ( distance > 0.5f)
            {
                //hammer.transform.position = rightHand.transform.position;
                hammer.transform.position = Vector3.MoveTowards(hammer.transform.position, rightHand.transform.position, Time.deltaTime * magnetspeed);
                hammerRB.velocity = Vector3.zero;
                hammerRB.angularVelocity = Vector3.zero;
                magnetspeed *= magnetmultiplier;
            } else {
                hammer.transform.position = rightHand.transform.position;
                hammer.transform.rotation = rightHand.transform.rotation;
                hammer.transform.Rotate(-90, -90, 0);
                rightHoldPositions.Add(rightHand.transform.position);
            }
            
        } else if ( leftPress ) {
            hammer.transform.position = leftHand.transform.position;
            hammer.transform.rotation = leftHand.transform.rotation;
            hammer.transform.Rotate(-90, 0, 0);
            hammerRB.velocity = Vector3.zero;
            hammerRB.angularVelocity = Vector3.zero;
            rightHoldPositions.Add(leftHand.transform.position);
        } else 
//        if ( !rightPress && !leftPress )
        { // not pressed
            magnetspeed = magnetminimum;
            
            if (rightHoldPositions.Count > 0)
            { // just released, have list of held positions
                debugText.text = "Debug: hammerpos held " + rightHoldPositions.Count.ToString();
                int framesBack = 100;
                if (framesBack < rightHoldPositions.Count) framesBack = rightHoldPositions.Count;
                Vector3 force = rightHoldPositions[rightHoldPositions.Count - framesBack] - rightHoldPositions[rightHoldPositions.Count];
                //force = Vector3.Normalize(force);
                force = Vector3.forward;
                hammerRB.velocity = Vector3.zero;
                hammerRB.angularVelocity = Vector3.zero;
                hammerRB.AddForce(force*100f);
                debugText.text += "\n"+"Force "+hammerRB.velocity;
                hammer.transform.position = Vector3.zero;
                rightHoldPositions = new List<Vector3>();
                magnetspeed = magnetminimum;
            }
        }
        updateControllerTimer -= Time.deltaTime;
        if ( updateControllerTimer < 0f )
        {
            updatecontroller();
            updateControllerTimer = 2f;
        }
    }
}
