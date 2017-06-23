using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorKit
{
    public enum HumanBodyBones
    {
        //
        // Summary:
        //     ///
        //     This is the Hips bone.
        //     ///
        Hips = 0,
        //
        // Summary:
        //     ///
        //     This is the Left Upper Leg bone.
        //     ///
        LeftUpperLeg = 1,
        //
        // Summary:
        //     ///
        //     This is the Right Upper Leg bone.
        //     ///
        RightUpperLeg = 2,
        //
        // Summary:
        //     ///
        //     This is the Left Knee bone.
        //     ///
        LeftLowerLeg = 3,
        //
        // Summary:
        //     ///
        //     This is the Right Knee bone.
        //     ///
        RightLowerLeg = 4,
        //
        // Summary:
        //     ///
        //     This is the Left Ankle bone.
        //     ///
        LeftFoot = 5,
        //
        // Summary:
        //     ///
        //     This is the Right Ankle bone.
        //     ///
        RightFoot = 6,
        //
        // Summary:
        //     ///
        //     This is the first Spine bone.
        //     ///
        Spine = 7,
        //
        // Summary:
        //     ///
        //     This is the Chest bone.
        //     ///
        Chest = 8,
        //
        // Summary:
        //     ///
        //     This is the Neck bone.
        //     ///
        Neck = 9,
        //
        // Summary:
        //     ///
        //     This is the Head bone.
        //     ///
        Head = 10,
        //
        // Summary:
        //     ///
        //     This is the Left Shoulder bone.
        //     ///
        LeftShoulder = 11,
        //
        // Summary:
        //     ///
        //     This is the Right Shoulder bone.
        //     ///
        RightShoulder = 12,
        //
        // Summary:
        //     ///
        //     This is the Left Upper Arm bone.
        //     ///
        LeftUpperArm = 13,
        //
        // Summary:
        //     ///
        //     This is the Right Upper Arm bone.
        //     ///
        RightUpperArm = 14,
        //
        // Summary:
        //     ///
        //     This is the Left Elbow bone.
        //     ///
        LeftLowerArm = 15,
        //
        // Summary:
        //     ///
        //     This is the Right Elbow bone.
        //     ///
        RightLowerArm = 16,
        //
        // Summary:
        //     ///
        //     This is the Left Wrist bone.
        //     ///
        LeftHand = 17,
        //
        // Summary:
        //     ///
        //     This is the Right Wrist bone.
        //     ///
        RightHand = 18,
        //
        // Summary:
        //     ///
        //     This is the Left Toes bone.
        //     ///
        LeftToes = 19,
        //
        // Summary:
        //     ///
        //     This is the Right Toes bone.
        //     ///
        RightToes = 20,
        //
        // Summary:
        //     ///
        //     This is the Left Eye bone.
        //     ///
        LeftEye = 21,
        //
        // Summary:
        //     ///
        //     This is the Right Eye bone.
        //     ///
        RightEye = 22,
        //
        // Summary:
        //     ///
        //     This is the Jaw bone.
        //     ///
        Jaw = 23,
        //
        // Summary:
        //     ///
        //     This is the left thumb 1st phalange.
        //     ///
        LeftThumbProximal = 24,
        //
        // Summary:
        //     ///
        //     This is the left thumb 2nd phalange.
        //     ///
        LeftThumbIntermediate = 25,
        //
        // Summary:
        //     ///
        //     This is the left thumb 3rd phalange.
        //     ///
        LeftThumbDistal = 26,
        //
        // Summary:
        //     ///
        //     This is the left index 1st phalange.
        //     ///
        LeftIndexProximal = 27,
        //
        // Summary:
        //     ///
        //     This is the left index 2nd phalange.
        //     ///
        LeftIndexIntermediate = 28,
        //
        // Summary:
        //     ///
        //     This is the left index 3rd phalange.
        //     ///
        LeftIndexDistal = 29,
        //
        // Summary:
        //     ///
        //     This is the left middle 1st phalange.
        //     ///
        LeftMiddleProximal = 30,
        //
        // Summary:
        //     ///
        //     This is the left middle 2nd phalange.
        //     ///
        LeftMiddleIntermediate = 31,
        //
        // Summary:
        //     ///
        //     This is the left middle 3rd phalange.
        //     ///
        LeftMiddleDistal = 32,
        //
        // Summary:
        //     ///
        //     This is the left ring 1st phalange.
        //     ///
        LeftRingProximal = 33,
        //
        // Summary:
        //     ///
        //     This is the left ring 2nd phalange.
        //     ///
        LeftRingIntermediate = 34,
        //
        // Summary:
        //     ///
        //     This is the left ring 3rd phalange.
        //     ///
        LeftRingDistal = 35,
        //
        // Summary:
        //     ///
        //     This is the left little 1st phalange.
        //     ///
        LeftLittleProximal = 36,
        //
        // Summary:
        //     ///
        //     This is the left little 2nd phalange.
        //     ///
        LeftLittleIntermediate = 37,
        //
        // Summary:
        //     ///
        //     This is the left little 3rd phalange.
        //     ///
        LeftLittleDistal = 38,
        //
        // Summary:
        //     ///
        //     This is the right thumb 1st phalange.
        //     ///
        RightThumbProximal = 39,
        //
        // Summary:
        //     ///
        //     This is the right thumb 2nd phalange.
        //     ///
        RightThumbIntermediate = 40,
        //
        // Summary:
        //     ///
        //     This is the right thumb 3rd phalange.
        //     ///
        RightThumbDistal = 41,
        //
        // Summary:
        //     ///
        //     This is the right index 1st phalange.
        //     ///
        RightIndexProximal = 42,
        //
        // Summary:
        //     ///
        //     This is the right index 2nd phalange.
        //     ///
        RightIndexIntermediate = 43,
        //
        // Summary:
        //     ///
        //     This is the right index 3rd phalange.
        //     ///
        RightIndexDistal = 44,
        //
        // Summary:
        //     ///
        //     This is the right middle 1st phalange.
        //     ///
        RightMiddleProximal = 45,
        //
        // Summary:
        //     ///
        //     This is the right middle 2nd phalange.
        //     ///
        RightMiddleIntermediate = 46,
        //
        // Summary:
        //     ///
        //     This is the right middle 3rd phalange.
        //     ///
        RightMiddleDistal = 47,
        //
        // Summary:
        //     ///
        //     This is the right ring 1st phalange.
        //     ///
        RightRingProximal = 48,
        //
        // Summary:
        //     ///
        //     This is the right ring 2nd phalange.
        //     ///
        RightRingIntermediate = 49,
        //
        // Summary:
        //     ///
        //     This is the right ring 3rd phalange.
        //     ///
        RightRingDistal = 50,
        //
        // Summary:
        //     ///
        //     This is the right little 1st phalange.
        //     ///
        RightLittleProximal = 51,
        //
        // Summary:
        //     ///
        //     This is the right little 2nd phalange.
        //     ///
        RightLittleIntermediate = 52,
        //
        // Summary:
        //     ///
        //     This is the right little 3rd phalange.
        //     ///
        RightLittleDistal = 53,
        //
        // Summary:
        //     ///
        //     This is the Upper Chest bone.
        //     ///
        UpperChest = 54,
        //
        // Summary:
        //     ///
        //     This is the Last bone index delimiter.
        //     ///
        LastBone = 55
    }
}
