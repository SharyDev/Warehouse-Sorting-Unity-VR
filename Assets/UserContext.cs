using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

[System.Serializable]
public class TagInfo
{
    public string tag;
    public string description;
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

    [Header("Player Rig")]
    public Transform playerRig; 

    private InputDevice rightHandDevice;
    private InputDevice leftHandDevice;

    private bool primaryButtonPressed = false;
    private bool secondaryButtonPressed = false;
    private bool leftPrimaryButtonPressed = false;   
    private bool leftSecondaryButtonPressed = false; 

    void Start()
    {
        // Get right hand device
        rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!rightHandDevice.isValid)
        {
            Debug.LogWarning("Right-hand controller is not valid. Falling back to simulator.");
        }

        // Get left hand device
        leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (!leftHandDevice.isValid)
        {
            Debug.LogWarning("Left-hand controller is not valid. Some VR features may not work.");
        }

        // Ensure fallback transform is assigned
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
        // Re-acquire devices if not valid
        if (!rightHandDevice.isValid)
        {
            rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (!rightHandDevice.isValid)
            {
                Debug.LogWarning("Right-hand controller is still not valid. Continuing with fallback.");
            }
        }

        if (!leftHandDevice.isValid)
        {
            leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (!leftHandDevice.isValid)
            {
                Debug.LogWarning("Left-hand controller is still not valid.");
            }
        }

        //For when using Occulus.
        if (rightHandDevice.isValid && leftHandDevice.isValid)
        {
            //Gets the button states from the right hand controller.
            rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
            rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed);

            //Gets the button states from the left hand controller.
            leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out leftPrimaryButtonPressed);
            leftHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out leftSecondaryButtonPressed);

            // If left X button is pressed
            if (leftPrimaryButtonPressed)
            {
                if (infoText != null)
                {
                    infoText.text = "ADD INSTRUCTIONS HERE";
                    Debug.Log("Updated UI text to ADD INSTRUCTIONS HERE due to X press on left controller.");
                }
                else
                {
                    Debug.LogError("InfoText is not assigned!");
                }
            }

            // If left Y button is pressed -> reset player to (0,0,0)
            if (leftSecondaryButtonPressed)
            {
                if (playerRig != null)
                {
                    playerRig.position = Vector3.zero;
                    Debug.Log("Player/Cam Rig position reset to (0,0,0) due to Y press on left controller.");
                }
                else
                {
                    Debug.LogError("PlayerRig not assigned! Cannot reset position.");
                }
            }

        }
        else
        {
            // This is the fallback (XR simulator) mode
            // Simulates button presses with left-click fallback on PC
            if (Input.GetMouseButtonDown(0))
            {
                primaryButtonPressed = true;
            }
            else
            {
                primaryButtonPressed = false;
            }

            // In fallback mode, we do not perform the new X/Y logic.
        }

        Debug.Log($"Primary Button (Right): {primaryButtonPressed}, Secondary Button (Right): {secondaryButtonPressed}, Left Primary: {leftPrimaryButtonPressed}, Left Secondary: {leftSecondaryButtonPressed}");

        // Raycasting for object interactions using right controller
        Ray ray = new Ray(rightControllerTransform.position, rightControllerTransform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}, Tag: {hit.collider.tag}");

            if (primaryButtonPressed || secondaryButtonPressed)
            {
                Debug.Log("Right controller button pressed, handling interaction...");
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
            if (infoText != null)
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
