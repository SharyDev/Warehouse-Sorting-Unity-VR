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

    [Header("Triggered Audio")]
    public AudioSource audioSource;

    [Header("Background Music")]
    public AudioSource backgroundAudioSource;
    public List<AudioClip> backgroundMusicClips;

    [Header("Raycast Settings")]
    public Transform rightControllerTransform;
    //From how far away can the controller's "view" see objects.
    public float raycastDistance = 25f;

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

    private int currentTrackIndex = 0;

    void Start()
    {
        //Intializes the left and right VR controller (tested on Meta Quest 2).
        rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        //Starts playing any background music if set in the editor.
        if (backgroundAudioSource != null && backgroundMusicClips != null && backgroundMusicClips.Count > 0)
        {
            PlayBackgroundTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        //Will try to find if the device is valid, fallsback to XR Device Simulator
        //Note the XR Device Simulator should be disabled when testing/using a real
        //VR headset.
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
                infoText.text = "Right Joystick to move, Point to any object and click B or A to get a description. " +
                                "Left joystick to jump. The right joystick will move in the direction you're moving in. " +
                                "Moving the right joystick backwards will make the camera flip to look backwards. " +
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
            //This is the fallback part the if VR devices are not found.
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

        //Checks if background track has finished in order to play the next one (if there is a next one).
        if (backgroundAudioSource != null && !backgroundAudioSource.isPlaying && backgroundMusicClips.Count > 0)
        {
            //Moves to the next track.
            currentTrackIndex = (currentTrackIndex + 1) % backgroundMusicClips.Count;
            PlayBackgroundTrack(currentTrackIndex);
        }
    }

    /*
     *Manages the UI when an Object's Tag is detected in the right controller's
     *view.
     */
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
    /*
     *Used to update the UI and play a quick audio sound.
     */
    private void UpdateUIAndPlayAudio(string description, AudioClip clip)
    {
        if (infoText != null)
        {
            infoText.text = description;
        }

        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();
            AudioClip currentlyPlayingClip = clip;
            StartCoroutine(StopAudioAfterSeconds(currentlyPlayingClip, 8f)); 
        }
    }

    private System.Collections.IEnumerator StopAudioAfterSeconds(AudioClip originalClip, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (audioSource.isPlaying && audioSource.clip == originalClip)
        {
            audioSource.Stop();
        }
    }



    private void PlayBackgroundTrack(int index)
    {
        if (backgroundMusicClips.Count == 0) return;

        backgroundAudioSource.loop = false; 
        backgroundAudioSource.clip = backgroundMusicClips[index];
        backgroundAudioSource.Play();
    }
}
