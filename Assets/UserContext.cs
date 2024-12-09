using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

[System.Serializable]
public class TagInfo
{
    //Object that the script will target (e.g. box user is looking at with controller)
    public string tag; 
    //This description will show up on the users screen to explain what the object is.
    public string description; 
    //Audio clip plays when the user presses the button on a valid object.
    public AudioClip audioClip; 
}

public class UserContext : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI infoText; 

    [Header("Audio")]
    public AudioSource audioSource; 

    [Header("Raycast Settings")]
    public Transform rightControllerTransform; 
    public float raycastDistance = 12f;

    [Header("Tag Information")]
    public List<TagInfo> tagInfoList; 

    private InputDevice rightHandDevice;
    private bool primaryButtonPressed = false;
    private bool secondaryButtonPressed = false;

    void Start()
    {
        //Attempts to get the right hand controller, if an Occulus is not connected then it should
        //fallback into using the unity XR Device Simulator.
        rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (!rightHandDevice.isValid)
        {
            Debug.LogWarning("Right-hand controller is not valid. Falling back to simulator.");
        }

        // Ensure the fallback transform is assigned for the XR Device Simulator
        if (rightControllerTransform == null)
        {
            Debug.LogError("RightControllerTransform is not assigned. Please assign it in the Inspector.");
        }
        else
        {
            Debug.Log("Using fallback rightControllerTransform for XR Device Simulator.");
        }
    }

    void Update()
    {
        //Attempts to get the right hand controller, if an Occulus is not connected then it should
        //fallback into using the unity XR Device Simulator.
        if (!rightHandDevice.isValid)
        {
            rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (!rightHandDevice.isValid)
            {
                Debug.LogWarning("Right-hand controller is still not valid. Continuing with fallback.");
            }
        }
        else
        {
            //Input values for primary and secondary buttons
            rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
            rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed);
        }

        //Simulates button presses for XR Device Simulator with left-click fallback (this is used only
        //if no VR device is connected)
        if (!rightHandDevice.isValid)
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                primaryButtonPressed = true;
            }
            else
            {
                primaryButtonPressed = false;
            }
        }

        Debug.Log($"Primary Button: {primaryButtonPressed}, Secondary Button: {secondaryButtonPressed}");


        //Rays used to "hit" objects with tags, such as a box. Note items that use this must
        //have a collider attached!
        Ray ray = new Ray(rightControllerTransform.position, rightControllerTransform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}, Tag: {hit.collider.tag}");

            if (primaryButtonPressed || secondaryButtonPressed)
            {
                Debug.Log("Button pressed, handling interaction...");
                HandleObjectTag(hit.collider.tag);
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object.");
        }
    }

    private void HandleObjectTag(string objectTag)
    {
        Debug.Log($"Handling tag: {objectTag}");

        TagInfo tagInfo = tagInfoList.Find(info => info.tag == objectTag);

        if (tagInfo != null)
        {
            UpdateUIAndPlayAudio(tagInfo.description, tagInfo.audioClip);
        }
        else
        {
            Debug.LogWarning($"No TagInfo found for tag: {objectTag}");
            infoText.text = "";
        }
    }

    private void UpdateUIAndPlayAudio(string description, AudioClip clip)
    {
        if (infoText != null)
        {
            infoText.text = description;
            Debug.Log($"Updated UI text: {description}");
        }
        else
        {
            Debug.LogError("InfoText is not assigned!");
        }
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"Playing audio clip: {clip.name}");
        }
        else
        {
            Debug.LogError("AudioSource or AudioClip is missing!");
        }
    }
}
