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

    [Header("Head Transform")]
    public Transform headTransform;

    [Header("Movement Settings")]
    public float movementSpeed = 2f;

    private InputDevice rightHandDevice;
    private InputDevice leftHandDevice;

    private bool primaryButtonPressed = false;
    private bool secondaryButtonPressed = false;
    private bool leftPrimaryButtonPressed = false;
    private bool leftSecondaryButtonPressed = false;

    private bool prevPrimaryButtonPressed = false;
    private bool prevSecondaryButtonPressed = false;
    private bool prevLeftPrimaryButtonPressed = false;
    private bool prevLeftSecondaryButtonPressed = false;

    void Start()
    {
        rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        if (!rightHandDevice.isValid)
            rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (!leftHandDevice.isValid)
            leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (rightHandDevice.isValid && leftHandDevice.isValid)
        {
            rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
            rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed);
            leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out leftPrimaryButtonPressed);
            leftHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out leftSecondaryButtonPressed);

            bool primaryButtonDown = (primaryButtonPressed && !prevPrimaryButtonPressed);
            bool secondaryButtonDown = (secondaryButtonPressed && !prevSecondaryButtonPressed);
            bool leftPrimaryButtonDown = (leftPrimaryButtonPressed && !prevLeftPrimaryButtonPressed);
            bool leftSecondaryButtonDown = (leftSecondaryButtonPressed && !prevLeftSecondaryButtonPressed);

            if ((leftPrimaryButtonDown || leftSecondaryButtonDown) && infoText != null)
            {
                infoText.text = "Right Joystick to move, Point to any object and click B or A to get a description" +
                    "Left joystick to jump. The right joystick will move in the direction you're moving in." +
                    "Moving the right joystick backwards will make the camera flip to look backwards." +
                    "Press B or A to remove these instructions and press Y or X to see them again.";
            }

            Vector2 leftAxis;
            Vector2 rightAxis;
            leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftAxis);
            rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightAxis);

            if (playerRig != null && headTransform != null)
            {
                Vector3 forward = headTransform.forward;
                forward.y = 0f;
                forward.Normalize();

                Vector3 right = headTransform.right;
                right.y = 0f;
                right.Normalize();

                Vector3 horizontalMovement = right * (rightAxis.x * movementSpeed * Time.deltaTime);
                Vector3 forwardMovement = forward * (rightAxis.y * movementSpeed * Time.deltaTime);
                Vector3 verticalMovement = Vector3.up * (leftAxis.y * movementSpeed * Time.deltaTime);

                Vector3 totalMovement = horizontalMovement + forwardMovement + verticalMovement;
                playerRig.position += totalMovement;
            }

            prevPrimaryButtonPressed = primaryButtonPressed;
            prevSecondaryButtonPressed = secondaryButtonPressed;
            prevLeftPrimaryButtonPressed = leftPrimaryButtonPressed;
            prevLeftSecondaryButtonPressed = leftSecondaryButtonPressed;

            Ray ray = new Ray(rightControllerTransform.position, rightControllerTransform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                if (primaryButtonDown || secondaryButtonDown)
                {
                    HandleObjectTag(hit.collider.tag);
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                primaryButtonPressed = true;
                Ray ray = new Ray(rightControllerTransform.position, rightControllerTransform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    HandleObjectTag(hit.collider.tag);
                }
            }
            else
            {
                primaryButtonPressed = false;
            }
        }
    }

    private void HandleObjectTag(string objectTag)
    {
        TagInfo tagInfo = tagInfoList.Find(info => info.tag == objectTag);
        if (tagInfo != null)
        {
            UpdateUIAndPlayAudio(tagInfo.description, tagInfo.audioClip);
        }
        else
        {
            if (infoText != null)
                infoText.text = "";
        }
    }

    private void UpdateUIAndPlayAudio(string description, AudioClip clip)
    {
        if (infoText != null)
        {
            infoText.text = description;
        }

        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
