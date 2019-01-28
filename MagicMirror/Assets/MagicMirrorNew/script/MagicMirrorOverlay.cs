using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMirrorOverlay : MonoBehaviour {

    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GUITexture backgroundImage;

    [Tooltip("Camera that will be used to overlay the 3D-objects over the background.")]
    public Camera foregroundCamera;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Game object used to overlay the joint.")]
    public Transform overlayObject;

    public Transform UpperArmObject;
    public Transform LowerArmObject;

    [Tooltip("Kinect joint that is going to be overlayed.")]
    public KinectInterop.JointType trackedJointHandRight = KinectInterop.JointType.HandRight;

    [Tooltip("Kinect joint that is going to be overlayed.")]
    public KinectInterop.JointType trackedJointWristRight = KinectInterop.JointType.WristRight;

    [Tooltip("Kinect joint that is going to be overlayed.")]
    public KinectInterop.JointType trackedJointElbowRight = KinectInterop.JointType.ElbowRight;

    [Tooltip("Kinect joint that is going to be overlayed.")]
    public KinectInterop.JointType trackedJointShoulderRight = KinectInterop.JointType.ShoulderRight;

    private Quaternion initialRotation = Quaternion.identity;

    // Use this for initialization
    void Start () {

        // initial rotation
        initialRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));

        Debug.Log(UpperArmObject.forward);

    }

    // Update is called once per frame
    void Update()
    {

        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized() && foregroundCamera)
        {
            // set color background
            if (backgroundImage && (backgroundImage.texture == null))
            {
                backgroundImage.texture = manager.GetUsersClrTex();
            }

            // get the background rectangle (use the portrait background, if available)
            Rect backgroundRect = foregroundCamera.pixelRect;

            // overlay the joint
            int iHandRightJointIndex = (int)trackedJointHandRight;
            int iWristRightJointIndex = (int)trackedJointWristRight;
            int iElbowRightJointIndex = (int)trackedJointElbowRight;
            int iShoulderRightJointIndex = (int)trackedJointShoulderRight;

            if (manager.IsUserDetected())
            {
                // get body id
                long userId = manager.GetUserIdByIndex(playerIndex);

                // set bag position (hand right joint)
                if (manager.IsJointTracked(userId, iHandRightJointIndex))
                {
                    // joint position in 3d coordinate
                    Vector3 posHandRightJoint = manager.GetJointPosColorOverlay(userId, iHandRightJointIndex, foregroundCamera, backgroundRect);
                    //Debug.Log(posHandRightJoint);
                    if (posHandRightJoint != Vector3.zero)
                    {
                        if (overlayObject)
                        {
                            overlayObject.position = posHandRightJoint;
                        }
                    }
                }

                // set bag rotation
                if (manager.IsJointTracked(userId, iWristRightJointIndex) && manager.IsJointTracked(userId, iElbowRightJointIndex) && manager.IsJointTracked(userId, iShoulderRightJointIndex))
                {
                    Vector3 posWristRightJoint = manager.GetJointPosColorOverlay(userId, iWristRightJointIndex, foregroundCamera, backgroundRect);
                    Vector3 posElbowRightJoint = manager.GetJointPosColorOverlay(userId, iElbowRightJointIndex, foregroundCamera, backgroundRect);
                    Vector3 posShoulderRightJoint = manager.GetJointPosColorOverlay(userId, iShoulderRightJointIndex, foregroundCamera, backgroundRect);

                    Vector3 tar_dir = new Vector3(posWristRightJoint.x - posElbowRightJoint.x, 0, posWristRightJoint.z - posElbowRightJoint.z);
                    tar_dir = Vector3.Normalize(tar_dir);
                    //Debug.Log(tar_dir);

                    if (overlayObject)
                    {
                        Vector3 ori_dir = new Vector3(overlayObject.forward.x, 0, overlayObject.forward.z);
                        ori_dir = Vector3.Normalize(ori_dir);
                        float tar_angle = 360f;
                        Vector3 newDir = Vector3.RotateTowards(ori_dir, tar_dir, tar_angle * 3.14159F / 180F, 0.0F);
                        overlayObject.rotation = Quaternion.LookRotation(newDir);
                    }

                    // set lower arm rotation & position
                    Vector3 tar_arm_dir = new Vector3(posWristRightJoint.x - posElbowRightJoint.x, posWristRightJoint.y - posElbowRightJoint.y, posWristRightJoint.z - posElbowRightJoint.z);
                    tar_arm_dir = Vector3.Normalize(tar_arm_dir);

                    if (LowerArmObject)
                    {
                        LowerArmObject.position = posElbowRightJoint;
                        float tar_angle = 360f;
                        Vector3 newDir = Vector3.RotateTowards(LowerArmObject.forward, tar_arm_dir, tar_angle * 3.14159F / 180F, 0.0F);
                        LowerArmObject.rotation = Quaternion.LookRotation(newDir);
                    }

                    // set upper arm
                    Vector3 tar_upper_arm_dir = new Vector3(posShoulderRightJoint.x - posElbowRightJoint.x, posShoulderRightJoint.y - posElbowRightJoint.y, posShoulderRightJoint.z - posElbowRightJoint.z);
                    tar_upper_arm_dir = Vector3.Normalize(tar_upper_arm_dir);

                    if (UpperArmObject)
                    {
                        UpperArmObject.position = posElbowRightJoint;
                        float tar_angle = 360f;
                        Vector3 newDir = Vector3.RotateTowards(UpperArmObject.forward, tar_upper_arm_dir, tar_angle * 3.14159F / 180F, 0.0F);
                        UpperArmObject.rotation = Quaternion.LookRotation(newDir);
                    }

                }
            }
        }
    }
}
