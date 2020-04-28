﻿using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable 0219

namespace ImmixKit
{
    namespace Weapons
    {
        public class GrenadeControllerRuntimeData
        {
            public Kit_GrenadeRenderer grenadeRenderer;
            public Kit_ThirdPersonGrenadeRenderer tpGrenadeRenderer;
            public List<GameObject> instantiatedObjects = new List<GameObject>(); //Objects that were instantiated by this weapon. When it is replaced, they have to be cleaned up first.

            public bool isSelectedAndReady = false;

            public Animator genericAnimator;

            #region Weapon Delay
    
            public Transform weaponDelayTransform;
   
            public Vector3 weaponDelayCur;
 
            public float weaponDelayCurrentX;
   
            public float weaponDelayCurrentY;
            #endregion

            #region Weapon Fall
            public Transform weaponFallTransform;
            #endregion

            #region Run Animation
   
            public bool startedRunAnimation;
            #endregion

            #region Sound

            public AudioSource sounds;
            #endregion

            public float lastRun;

    
            public int lastWeaponAnimationID;

            public int amountOfGrenadesLeft;
                  public bool isThrowingGrenade;
    
            public float beganThrowingGrenade;
      
            public bool hasThrownGrenadeAndIsWaitingForReturn;
      
            public float grenadeThrownAt;
  
            public bool isRedrawInProgress;

            #region Input
            public bool lastLmb;
            public bool lastRmb;
            #endregion
        }

        public class GrenadeControllerOthersRuntimeData
        {
            public Kit_ThirdPersonGrenadeRenderer tpGrenadeRenderer;
            public List<GameObject> instantiatedObjects = new List<GameObject>(); 
        }

        public enum GrenadeMode { QuickUse, IndividualWeapon, Both }

        public class Kit_ModernGrenadeScript : Kit_WeaponBase
        {
            public GrenadeMode grenadeMode = GrenadeMode.Both;
    
            public int amountOfGrenadesAtStart = 3;
            [Header("Normal use")]
        
            public float pullPinTime = 0.5f;
      
            public float throwTime = 0.5f;
         
            public float redrawTime = 0.5f;
      
            [Header("Quick Use")]
            public float quickUsePullPinTime = 0.5f;
     
            public float quickUseThrowTime = 0.5f;

            [Header("Throw")]
       
            public Vector3 throwForce;
        
            public Vector3 throwTorque;
        
            public GameObject grenadePrefab;

            #region Sounds
            [Header("Sounds")]
      
            public AudioClip drawSound;
   
            public AudioClip putawaySound;
      
            public AudioClip pullPinSound;
        
            public AudioClip throwSound;
     
            public AudioClip pullPinQuickSound;
      
            public AudioClip throwQuickSound;
         
            public float thirdPersonRange = 30f;
     
            [HideInInspector]
            public AnimationCurve thirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 30f, 0f);
        
            public int voiceGrenadeSoundID;
            #endregion

            #region Weapon Delay
            [Header("Weapon Delay")]
      
            public float weaponDelayBaseAmount = 1f;
        
            public float weaponDelayMaxAmount = 0.02f;
       
            public float weaponDelayAimingMultiplier = 0.3f;
          
            public float weaponDelaySmooth = 3f;
            #endregion

            #region Weapon Tilt
         
            public bool weaponTiltEnabled = true;
        
            public float weaponTiltIntensity = 5f;
     
            public float weaponTiltReturnSpeed = 3f;
            #endregion

            #region Weapon Fall
            [Header("Fall Down effect")]
            public float fallDownAmount = 10.0f;
            public float fallDownMinOffset = -6.0f;
            public float fallDownMaxoffset = 6.0f;
            public float fallDownTime = 0.1f;
            public float fallDownReturnSpeed = 1f;
            #endregion

            #region Generic Animations
            [Header("Generic Animations")]
     
            public GameObject genericGunAnimatorControllerPrefab;

     
            public bool useGenericWalkAnim = true;

        
            public bool useGenericRunAnim = true;
            #endregion

            [HideInInspector]
            public int gameGunID;

            public override WeaponDisplayData GetWeaponDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    if (data.amountOfGrenadesLeft > 0 && (grenadeMode == GrenadeMode.IndividualWeapon || grenadeMode == GrenadeMode.Both))
                    {
                        WeaponDisplayData wdd = new WeaponDisplayData();
                        wdd.sprite = weaponHudPicture;
                        wdd.name = weaponName;
                        return wdd;
                    }
                }
                return null;
            }

            public override WeaponQuickUseDisplayData GetWeaponQuickUseDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    if (data.amountOfGrenadesLeft > 0 && (grenadeMode == GrenadeMode.QuickUse || grenadeMode == GrenadeMode.Both))
                    {
                        WeaponQuickUseDisplayData wdd = new WeaponQuickUseDisplayData();
                        wdd.sprite = weaponQuickUsePicture;
                        wdd.name = weaponName;
                        wdd.amount = data.amountOfGrenadesLeft;
                        return wdd;
                    }
                }
                return null;
            }

            public override float AimInTime(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0.5f;
            }

            public override void AnimateWeapon(Kit_PlayerBehaviour pb, object runtimeData, int id, float speed)
            {
                if (pb.isBot) return;
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;

                    //Camera animation
                    if (data.grenadeRenderer.cameraAnimationEnabled)
                    {
                        if (data.grenadeRenderer.cameraAnimationType == CameraAnimationType.Copy)
                        {
                            pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.grenadeRenderer.cameraAnimationReferenceRotation) * data.grenadeRenderer.cameraAnimationBone.localRotation;
                        }
                        else if (data.grenadeRenderer.cameraAnimationType == CameraAnimationType.LookAt)
                        {
                            pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.grenadeRenderer.cameraAnimationReferenceRotation) * Quaternion.LookRotation(data.grenadeRenderer.cameraAnimationTarget.localPosition - data.grenadeRenderer.cameraAnimationBone.localPosition);
                        }
                    }
                    else
                    {
                        //Go back to 0,0,0
                        pb.playerCameraAnimationTransform.localRotation = Quaternion.Slerp(pb.playerCameraAnimationTransform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                    }

                    //Weapon delay calculation
                    if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        //Get input from the mouse
                        data.weaponDelayCurrentX = -pb.input.mouseX * weaponDelayBaseAmount * Time.deltaTime;
                        if (!pb.looking.ReachedYMax(pb)) //Check if we should have delay on y looking
                            data.weaponDelayCurrentY = -pb.input.mouseY * weaponDelayBaseAmount * Time.deltaTime;
                        else //If not, just set it to zero
                            data.weaponDelayCurrentY = 0f;
                    }
                    else
                    {
                        //Cursor is not locked, set values to zero
                        data.weaponDelayCurrentX = 0f;
                        data.weaponDelayCurrentY = 0f;
                    }

                    //Clamp
                    data.weaponDelayCurrentX = Mathf.Clamp(data.weaponDelayCurrentX, -weaponDelayMaxAmount, weaponDelayMaxAmount);
                    data.weaponDelayCurrentY = Mathf.Clamp(data.weaponDelayCurrentY, -weaponDelayMaxAmount, weaponDelayMaxAmount);

                    //Update Vector
                    data.weaponDelayCur.x = data.weaponDelayCurrentX;
                    data.weaponDelayCur.y = data.weaponDelayCurrentY;
                    data.weaponDelayCur.z = 0f;

                    //Smooth move towards the target
                    data.weaponDelayTransform.localPosition = Vector3.Lerp(data.weaponDelayTransform.localPosition, data.weaponDelayCur, Time.deltaTime * weaponDelaySmooth);

                    //Weapon tilt
                    if (weaponTiltEnabled)
                    {
                        data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.Euler(0, 0, -pb.movement.GetMovementDirection(pb).x * weaponTiltIntensity), Time.deltaTime * weaponTiltReturnSpeed);
                    }
                    else
                    {
                        data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.identity, Time.deltaTime * weaponTiltReturnSpeed);
                    }

                    //Weapon Fall
                    data.weaponFallTransform.localRotation = Quaternion.Slerp(data.weaponFallTransform.localRotation, Quaternion.identity, Time.deltaTime * fallDownReturnSpeed);

                    //Set speed
                    if (id != 0)
                    {
                        data.genericAnimator.SetFloat("speed", speed);
                    }
                    //If idle, set speed to 1
                    else
                    {
                        data.genericAnimator.SetFloat("speed", 1f);
                    }

                    //Run position and rotation
                    //Check state and if we can move
                    if (id == 2 && data.isSelectedAndReady)
                    {
                        //Move to run pos
                        data.grenadeRenderer.transform.localPosition = Vector3.Lerp(data.grenadeRenderer.transform.localPosition, data.grenadeRenderer.runPos, Time.deltaTime * data.grenadeRenderer.runSmooth);
                        //Move to run rot
                        data.grenadeRenderer.transform.localRotation = Quaternion.Slerp(data.grenadeRenderer.transform.localRotation, Quaternion.Euler(data.grenadeRenderer.runRot), Time.deltaTime * data.grenadeRenderer.runSmooth);
                        //Set time
                        data.lastRun = Time.time;
                    }
                    else
                    {
                        //Move back to idle pos
                        data.grenadeRenderer.transform.localPosition = Vector3.Lerp(data.grenadeRenderer.transform.localPosition, Vector3.zero, Time.deltaTime * data.grenadeRenderer.runSmooth * 2f);
                        //Move back to idle rot
                        data.grenadeRenderer.transform.localRotation = Quaternion.Slerp(data.grenadeRenderer.transform.localRotation, Quaternion.identity, Time.deltaTime * data.grenadeRenderer.runSmooth * 2f);
                    }


                    //Check if state changed
                    if (id != data.lastWeaponAnimationID)
                    {
                        //Idle
                        if (id == 0)
                        {
                            //Play idle animation
                            data.genericAnimator.CrossFade("Idle", 0.3f);

                            if (!useGenericRunAnim)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.grenadeRenderer.anim)
                                    {
                                        data.grenadeRenderer.anim.ResetTrigger("Start Run");
                                        data.grenadeRenderer.anim.SetTrigger("End Run");
                                    }
           
                                }
                            }
                        }
                        //Walk
                        else if (id == 1)
                        {
                            //Check if we should use generic anim
                            if (useGenericWalkAnim)
                            {
                                //Play run animation
                                data.genericAnimator.CrossFade("Walk", 0.2f);
                            }
                            //If not continue to play Idle
                            else
                            {
                                //Play idle animation
                                data.genericAnimator.CrossFade("Idle", 0.3f);
                            }

                            if (!useGenericRunAnim)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.grenadeRenderer.anim)
                                    {
                                        data.grenadeRenderer.anim.ResetTrigger("Start Run");
                                        data.grenadeRenderer.anim.SetTrigger("End Run");
                                    }
               
                                }
                            }
                        }
                        //Run
                        else if (id == 2)
                        {
                            //Check if we should use generic anim
                            if (useGenericRunAnim)
                            {
                                //Play run animation
                                data.genericAnimator.CrossFade("Run", 0.2f);
                            }
                            //If not continue to play Idle
                            else
                            {
                                //Play idle animation
                                data.genericAnimator.CrossFade("Idle", 0.3f);
                                //Start run animation on weapon animator
                                if (!data.startedRunAnimation && data.isSelectedAndReady)
                                {
                                    data.startedRunAnimation = true;
                                    if (data.grenadeRenderer.anim)
                                    {
                                        data.grenadeRenderer.anim.ResetTrigger("End Run");
                                        data.grenadeRenderer.anim.SetTrigger("Start Run");
                                    }
                            
                                }
                            }
                        }
                        //Update last state
                        data.lastWeaponAnimationID = id;
                    }
                    else
                    {
                        if (!useGenericRunAnim)
                        {
                            //Idle
                            if (id == 0)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.grenadeRenderer.anim)
                                    {
                                        data.grenadeRenderer.anim.ResetTrigger("Start Run");
                                        data.grenadeRenderer.anim.SetTrigger("End Run");
                                    }
                         
                                }
                            }
                            //Walk
                            else if (id == 1)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.grenadeRenderer.anim)
                                    {
                                        data.grenadeRenderer.anim.ResetTrigger("Start Run");
                                        data.grenadeRenderer.anim.SetTrigger("End Run");
                                    }
                          
                                }
                            }
                            //Run
                            else if (id == 2)
                            {
                                //Start run animation on weapon animator
                                if (!data.startedRunAnimation && data.isSelectedAndReady)
                                {
                                    data.startedRunAnimation = true;
                                    if (data.grenadeRenderer.anim)
                                    {
                                        data.grenadeRenderer.anim.ResetTrigger("End Run");
                                        data.grenadeRenderer.anim.SetTrigger("Start Run");
                                    }
                       
                                }
                            }
                        }
                    }
                }
            }

            public override bool SupportsCustomization()
            {
                return false;
            }

            public override bool CanBeSelected(Kit_PlayerBehaviour pb, object runtimeData)
            {
                //Quick use only?
                if (grenadeMode == GrenadeMode.QuickUse) return false;
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    if (data.amountOfGrenadesLeft > 0) return true;
                    else return false;
                }
                //If not data is to be found, then it cant be selected
                return false;
            }

            public override bool SupportsQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                //Grenades DO support this, if it is enabled!
                if (grenadeMode == GrenadeMode.Both || grenadeMode == GrenadeMode.QuickUse)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                    {
                        GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                        if (data.amountOfGrenadesLeft > 0) return true;
                    }
                }
                return false;
            }

            public override float BeginQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                    if (!pb.isBot)
                    {
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.grenadeRenderer.visible = true;
                        }
                        else
                        {
                            data.grenadeRenderer.visible = false;
                        }
                        if (data.grenadeRenderer.anim)
                        {
                            //Enable anim
                            data.grenadeRenderer.anim.enabled = true;
                        }
    
                        //Play Animation
                        if (data.grenadeRenderer.anim)
                        {
                            data.grenadeRenderer.anim.Play("Quick PullPin", 0, 0f);
                        }
         
                        //Play Sound
                        data.sounds.clip = pullPinQuickSound;
                        data.sounds.Play();
                        //Play TP Aniamtion
                        pb.thirdPersonPlayerModel.PlayGrenadeAnimation(0);
                    }

                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon and hide
                        data.tpGrenadeRenderer.visible = true;
                        data.tpGrenadeRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        //Show tp weapon and show
                        data.tpGrenadeRenderer.visible = true;
                        data.tpGrenadeRenderer.shadowsOnly = false;
                    }

                    return quickUsePullPinTime;
                }

                //In case of failure...
                return 0f;
            }

            public override float EndQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;

                    if (grenadePrefab)
                    {
                        object[] instData = new object[2];
                        instData[0] = pb.isBot;
                        if (pb.isBot)
                        {
                            instData[1] = pb.botId;
                        }
                        else
                        {
                            instData[1] = pb.photonView.OwnerActorNr;
                        }

                        //Throw Grenade
                        GameObject go = PhotonNetwork.Instantiate(grenadePrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);

                        //Get script
                        Kit_Grenade grenade = go.GetComponent<Kit_Grenade>();

                        if (grenade)
                        {
                            grenade.gunId = gameGunID;
                        }

                        //Get Rigidbody
                        Rigidbody rb = go.GetComponent<Rigidbody>();

                        if (rb)
                        {
                            rb.AddRelativeForce(throwForce);
                            rb.AddRelativeTorque(throwTorque);
                        }
                        else
                        {
                            Debug.LogError("Thrown Grenade has no rigidbody attached!");
                        }

                        //Get Collider
                        Collider col = go.GetComponentInChildren<Collider>();
                        if (col)
                        {
                            Physics.IgnoreCollision(col, pb.cc);
                            Physics.IgnoreCollision(pb.cc, col);
                        }
                    }
                    else
                    {
                        Debug.LogError("No Grenade Prefab!");
                    }

                    //Subtract
                    data.amountOfGrenadesLeft--;

                    if (!pb.isBot)
                    {
                        //Play Animation
                        if (data.grenadeRenderer.anim)
                        {
                            data.grenadeRenderer.anim.Play("Quick Throw", 0, 0f);
                        }

                        //Play Sound
                        data.sounds.clip = throwQuickSound;
                        data.sounds.Play();
                        //Play TP Aniamtion
                        pb.thirdPersonPlayerModel.PlayGrenadeAnimation(1);
                    }

                    return quickUseThrowTime;
                }
                //In case of failure...
                return 0f;
            }

            public override void EndQuickUseAfter(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    if (!pb.isBot)
                    {
                        if (data.grenadeRenderer.anim)
                        {
                            //Disable anim
                            data.grenadeRenderer.anim.enabled = false;
                        }
     
                        data.grenadeRenderer.visible = false;
                    }
                    data.tpGrenadeRenderer.visible = false;
                }
            }

            public override float BeginQuickUseOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                    //Play TP Aniamtion
                    pb.thirdPersonPlayerModel.PlayGrenadeAnimation(0);
                    //Update range
                    pb.thirdPersonPlayerModel.soundFire.maxDistance = thirdPersonRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                    pb.thirdPersonPlayerModel.soundFire.clip = pullPinQuickSound;
                    pb.thirdPersonPlayerModel.soundFire.Play();

                    //Show tp weapon and show
                    data.tpGrenadeRenderer.visible = true;
                    data.tpGrenadeRenderer.shadowsOnly = false;

                    return quickUsePullPinTime;
                }

                //In case of failure...
                return 0f;
            }

            public override float EndQuickUseOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    //Play TP Aniamtion
                    pb.thirdPersonPlayerModel.PlayGrenadeAnimation(1);
                    //Update range
                    pb.thirdPersonPlayerModel.soundFire.maxDistance = thirdPersonRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                    pb.thirdPersonPlayerModel.soundFire.clip = throwQuickSound;
                    pb.thirdPersonPlayerModel.soundFire.Play();

                    return quickUseThrowTime;
                }
                //In case of failure...
                return 0f;
            }

            public override void EndQuickUseAfterOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    data.tpGrenadeRenderer.visible = false;
                }
            }

            public override bool WaitForQuickUseButtonRelease()
            {
                return true;
            }

            public override void CalculateWeaponUpdate(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;

                    //Set this weapon to selected and ready (for other things)
                    data.isSelectedAndReady = true;

                    if (pb.canControlPlayer && (LockCursor.lockCursor || pb.isBot))
                    {
                        if (data.isThrowingGrenade)
                        {
                            if (Time.time - pullPinTime > data.beganThrowingGrenade && !pb.input.lmb)
                            {
                                if (grenadePrefab)
                                {
                                    object[] instData = new object[2];
                                    instData[0] = pb.isBot;
                                    if (pb.isBot)
                                    {
                                        instData[1] = pb.botId;
                                    }
                                    else
                                    {
                                        instData[1] = pb.photonView.OwnerActorNr;
                                    }

                                    //Throw Grenade
                                    GameObject go = PhotonNetwork.Instantiate(grenadePrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);

                                    //Get script
                                    Kit_Grenade grenade = go.GetComponent<Kit_Grenade>();

                                    if (grenade)
                                    {
                                        grenade.gunId = gameGunID;
                                    }

                                    //Get Rigidbody
                                    Rigidbody rb = go.GetComponent<Rigidbody>();

                                    if (rb)
                                    {
                                        rb.AddRelativeForce(throwForce);
                                        rb.AddRelativeTorque(throwTorque);
                                    }
                                    else
                                    {
                                        Debug.LogError("Thrown Grenade has no rigidbody attached!");
                                    }

                                    //Get Collider
                                    Collider col = go.GetComponentInChildren<Collider>();
                                    if (col)
                                    {
                                        Physics.IgnoreCollision(col, pb.cc);
                                        Physics.IgnoreCollision(pb.cc, col);
                                    }
                                }
                                else
                                {
                                    Debug.LogError("No Grenade Prefab!");
                                }

                                //Subtract
                                data.amountOfGrenadesLeft--;
                                //Wait for return
                                data.hasThrownGrenadeAndIsWaitingForReturn = true;
                                //Set Time
                                data.grenadeThrownAt = Time.time;
                                //Don't throw anymore
                                data.isThrowingGrenade = false;

                                if (!pb.isBot)
                                {
                                    //Play Animation
                                    if (data.grenadeRenderer.anim)
                                    {
                                        data.grenadeRenderer.anim.Play("Throw", 0, 0f);
                                    }
             
                                    //Play Sound
                                    data.sounds.clip = throwSound;
                                    data.sounds.Play();
                                    //Play TP Aniamtion
                                    pb.thirdPersonPlayerModel.PlayGrenadeAnimation(1);
                                }
                                else
                                {
                                    //Play TP anim
                                    pb.thirdPersonPlayerModel.PlayGrenadeAnimation(1);
                                    //Play Sound
                                    //Update range
                                    pb.thirdPersonPlayerModel.soundFire.maxDistance = thirdPersonRange;
                                    //Update sound rolloff
                                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                                    pb.thirdPersonPlayerModel.soundFire.clip = throwSound;
                                    pb.thirdPersonPlayerModel.soundFire.Play();
                                }

                                //Send RPC
                                pb.photonView.RPC("GrenadeThrowNetwork", RpcTarget.Others);
                            }
                        }
                        else
                        {
                            if (!data.hasThrownGrenadeAndIsWaitingForReturn)
                            {
                                if (data.amountOfGrenadesLeft > 0)
                                {
                                    if (pb.input.lmb && !data.isThrowingGrenade)
                                    {
                                        //Initiate throw
                                        data.isThrowingGrenade = true;
                                        //Set time
                                        data.beganThrowingGrenade = Time.time;

                                        if (!pb.isBot)
                                        {
                                            //Play Animation
                                            if (data.grenadeRenderer.anim)
                                            {
                                                data.grenadeRenderer.anim.Play("PullPin", 0, 0f);
                                            }
              
                                            //Play Sound
                                            data.sounds.clip = pullPinSound;
                                            data.sounds.Play();
                                            //Play TP Aniamtion
                                            pb.thirdPersonPlayerModel.PlayGrenadeAnimation(0);
                                        }
                                        else
                                        {
                                            //Play TP anim
                                            pb.thirdPersonPlayerModel.PlayGrenadeAnimation(0);
                                            //Update range
                                            pb.thirdPersonPlayerModel.soundFire.maxDistance = thirdPersonRange;
                                            //Update sound rolloff
                                            pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                                            //Play Sound
                                            pb.thirdPersonPlayerModel.soundFire.clip = pullPinSound;
                                            pb.thirdPersonPlayerModel.soundFire.Play();
                                        }

                                        //Send RPC
                                        pb.photonView.RPC("GrenadePullPinNetwork", RpcTarget.Others);
                                    }
                                }
                            }
                        }
                    }

                    if (data.hasThrownGrenadeAndIsWaitingForReturn)
                    {
                        if (!data.isRedrawInProgress)
                        {
                            if (Time.time - throwTime > data.grenadeThrownAt)
                            {
                                if (data.amountOfGrenadesLeft > 0)
                                {
                                    //Set bool
                                    data.isRedrawInProgress = true;
                                    //Set Time
                                    data.grenadeThrownAt = Time.time;
                                }
                                else
                                {
                                    data.isRedrawInProgress = false;
                                    data.hasThrownGrenadeAndIsWaitingForReturn = false;
                                    if (!pb.isBot)
                                    {
                                        //Reset fov
                                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                                        if (data.grenadeRenderer.anim)
                                        {
                                            //Disable anim
                                            data.grenadeRenderer.anim.enabled = false;
                                        }
              
                                        //Hide
                                        data.grenadeRenderer.visible = false;
                                    }
                                    //Hide tp weapon
                                    data.tpGrenadeRenderer.visible = false;

                                    //Make sure it is not ready yet
                                    data.isSelectedAndReady = false;
                                    //Stop third person anims
                                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                                    //Force unselect
                                    pb.weaponManager.ForceUnselectCurrentWeapon(pb);
                                }
                            }
                        }
                        else
                        {
                            if (Time.time - redrawTime > data.grenadeThrownAt)
                            {
                                //Done throwing! :)
                                data.hasThrownGrenadeAndIsWaitingForReturn = false;
                                data.isRedrawInProgress = false;
                            }
                        }
                    }

                    //Update HUD
                    if (!pb.isBot)
                    {
                        pb.main.hud.DisplayAmmo(-1, data.amountOfGrenadesLeft, true);
                    }
                }
            }

            public override void CalculateWeaponUpdateOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public override void NetworkGrenadePullPinRPCReceived(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    //Play TP anim
                    pb.thirdPersonPlayerModel.PlayGrenadeAnimation(0);
                    //Update range
                    pb.thirdPersonPlayerModel.soundFire.maxDistance = thirdPersonRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                    //Play Sound
                    pb.thirdPersonPlayerModel.soundFire.clip = pullPinSound;
                    pb.thirdPersonPlayerModel.soundFire.Play();
                }
            }

            public override void NetworkGrenadeThrowRPCReceived(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    //Play TP anim
                    pb.thirdPersonPlayerModel.PlayGrenadeAnimation(1);
                    //Update range
                    pb.thirdPersonPlayerModel.soundFire.maxDistance = thirdPersonRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                    //Play Sound
                    pb.thirdPersonPlayerModel.soundFire.clip = throwSound;
                    pb.thirdPersonPlayerModel.soundFire.Play();
                }
            }

            public override void DrawWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    if (!pb.isBot)
                    {
                        //Reset pos & rot of the renderer
                        data.grenadeRenderer.transform.localPosition = Vector3.zero;
                        data.grenadeRenderer.transform.localRotation = Quaternion.identity;
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                        if (data.grenadeRenderer.anim)
                        {
                            //Enable anim
                            data.grenadeRenderer.anim.enabled = true;
                        }

                        //Play animation
                        if (data.grenadeRenderer.anim)
                        {
                            data.grenadeRenderer.anim.Play("Draw", 0, 0f);
                        }
          
                        //Play sound if it is assigned
                        if (drawSound) data.sounds.PlayOneShot(drawSound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.grenadeRenderer.visible = true;
                        }
                        else
                        {
                            data.grenadeRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon and hide
                        data.tpGrenadeRenderer.visible = true;
                        data.tpGrenadeRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        //Show tp weapon and show
                        data.tpGrenadeRenderer.visible = true;
                        data.tpGrenadeRenderer.shadowsOnly = false;
                    }
                    //Make sure it is not ready yet
                    data.isSelectedAndReady = false;
                    //Reset run animation
                    data.startedRunAnimation = false;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void DrawWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    //Show tp weapon
                    data.tpGrenadeRenderer.visible = true;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                    //Update range
                    pb.thirdPersonPlayerModel.soundOther.maxDistance = thirdPersonRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundOther.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                    //Play Sound
                    pb.thirdPersonPlayerModel.soundOther.clip = drawSound;
                    pb.thirdPersonPlayerModel.soundOther.Play();
                }
            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, object runtimeData, bool wasFallDamageApplied)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    if (wasFallDamageApplied)
                    {
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(data.weaponFallTransform, new Vector3(fallDownAmount, Random.Range(fallDownMinOffset, fallDownMaxoffset), 0), fallDownTime));
                    }
                    else
                    {
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(data.weaponFallTransform, new Vector3(fallDownAmount / 3, Random.Range(fallDownMinOffset, fallDownMaxoffset) / 2, 0), fallDownTime));
                    }
                }
            }

            public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    //Get runtime data
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    //Activate or deactivate based on bool
                    if (perspective == Kit_GameInformation.Perspective.ThirdPerson)
                    {
                        if (data.grenadeRenderer)
                        {
                            data.grenadeRenderer.visible = false;
                        }
                        if (data.tpGrenadeRenderer)
                        {
                            data.tpGrenadeRenderer.visible = true;
                            data.tpGrenadeRenderer.shadowsOnly = false;
                        }
                    }
                    else
                    {
                        if (data.grenadeRenderer)
                        {
                            data.grenadeRenderer.visible = true;
                        }
                        if (data.tpGrenadeRenderer)
                        {
                            data.tpGrenadeRenderer.visible = true;
                            data.tpGrenadeRenderer.shadowsOnly = true;
                        }
                    }
                }
            }

            public override bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

            public override WeaponIKValues GetIK(Kit_PlayerBehaviour pb, Animator anim, object runtimeData)
            {
                return new WeaponIKValues();
            }

            public override WeaponStats GetStats()
            {
                return new WeaponStats();
            }

            public override bool SupportsStats()
            {
                return false;
            }

            public override bool IsWeaponAiming(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info, object runtimeData)
            {
                if (stream.IsWriting)
                {

                }
                else
                {

                }
            }

            public override void PutawayWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    if (!pb.isBot)
                    {
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        if (data.grenadeRenderer.anim)
                        {
                            //Enable anim
                            data.grenadeRenderer.anim.enabled = true;
                        }
 
                        //Play animation
                        if (data.grenadeRenderer.anim)
                        {
                            data.grenadeRenderer.anim.Play("Putaway", 0, 0f);
                        }
     
                        //Play sound if it is assigned
                        if (putawaySound) data.sounds.PlayOneShot(putawaySound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.grenadeRenderer.visible = true;
                        }
                        else
                        {
                            //Hide
                            data.grenadeRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon
                        data.tpGrenadeRenderer.visible = true;
                        data.tpGrenadeRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        data.tpGrenadeRenderer.visible = true;
                        data.tpGrenadeRenderer.shadowsOnly = false;
                    }
                    //Make sure it is not ready yet
                    data.isSelectedAndReady = false;
                    //Abort any throws!
                    data.hasThrownGrenadeAndIsWaitingForReturn = false;
                    data.isThrowingGrenade = false;
                    data.isRedrawInProgress = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponHide(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;
                    //Reset run animation
                    data.startedRunAnimation = false;
                    if (!pb.isBot)
                    {
                        //Hide weapon
                        data.grenadeRenderer.visible = false;
                        if (data.grenadeRenderer.anim)
                        {
                            //Disable anim
                            data.grenadeRenderer.anim.enabled = false;
                        }
       
                        //Reset pos & rot of the renderer
                        data.grenadeRenderer.transform.localPosition = Vector3.zero;
                        data.grenadeRenderer.transform.localRotation = Quaternion.identity;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                    }
                    //Hide tp weapon
                    data.tpGrenadeRenderer.visible = false;
                    //Make sure it is not ready
                    data.isSelectedAndReady = false;
                    //Abort any throws!
                    data.hasThrownGrenadeAndIsWaitingForReturn = false;
                    data.isThrowingGrenade = false;
                    data.isRedrawInProgress = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponHideOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    //Hide tp weapon
                    data.tpGrenadeRenderer.visible = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(GrenadeControllerOthersRuntimeData))
                {
                    GrenadeControllerOthersRuntimeData data = runtimeData as GrenadeControllerOthersRuntimeData;
                    //Show tp weapon
                    data.tpGrenadeRenderer.visible = true;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                    //Update range
                    pb.thirdPersonPlayerModel.soundOther.maxDistance = thirdPersonRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundOther.SetCustomCurve(AudioSourceCurveType.CustomRolloff, thirdPersonRolloff);
                    //Play Sound
                    pb.thirdPersonPlayerModel.soundOther.clip = putawaySound;
                    pb.thirdPersonPlayerModel.soundOther.Play();
                }
            }

            public override float Sensitivity(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 1f;
            }

            public override object SetupFirstPerson(Kit_PlayerBehaviour pb, int[] attachments)
            {
                GrenadeControllerRuntimeData data = new GrenadeControllerRuntimeData();

                if (!pb.isBot)
                {
                    //Setup root for this weapon
                    GameObject root = new GameObject("Weapon root");
                    root.transform.parent = pb.weaponsGo; //Set root
                    root.transform.localPosition = Vector3.zero; //Reset position
                    root.transform.localRotation = Quaternion.identity; //Reset rotation
                    root.transform.localScale = Vector3.one; //Reset scale

                    //Setup generic animations
                    GameObject genericAnimations = Instantiate(genericGunAnimatorControllerPrefab);
                    genericAnimations.transform.parent = root.transform;
                    genericAnimations.transform.localPosition = Vector3.zero; //Reset position
                    genericAnimations.transform.localRotation = Quaternion.identity; //Reset rotation
                    genericAnimations.transform.localScale = Vector3.one; //Reset scale

                    //Get animator
                    Animator anim = genericAnimations.GetComponent<Animator>(); ;
                    anim.Play("Idle");
                    data.genericAnimator = anim;

                    //Delay transform
                    GameObject delayTrans = new GameObject("Weapon delay");
                    delayTrans.transform.parent = genericAnimations.transform; //Set root
                    delayTrans.transform.localPosition = Vector3.zero; //Reset position
                    delayTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    delayTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.weaponDelayTransform = delayTrans.transform;

                    //Delay transform
                    GameObject fallTrans = new GameObject("Weapon fall");
                    fallTrans.transform.parent = delayTrans.transform; //Set root
                    fallTrans.transform.localPosition = Vector3.zero; //Reset position
                    fallTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    fallTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.weaponFallTransform = fallTrans.transform;

                    //Get Fire Audio (Needs to be consistent)
                    if (pb.weaponsGo.GetComponent<AudioSource>()) data.sounds = pb.weaponsGo.GetComponent<AudioSource>();
                    else data.sounds = pb.weaponsGo.gameObject.AddComponent<AudioSource>();

                    //Setup the first person prefab
                    GameObject fpRuntime = Instantiate(firstPersonPrefab, fallTrans.transform, false);
                    fpRuntime.transform.localScale = Vector3.one; //Reset scale

                    //Setup renderer
                    data.grenadeRenderer = fpRuntime.GetComponent<Kit_GrenadeRenderer>();
                    data.grenadeRenderer.visible = false;

                    //Add to the list
                    data.instantiatedObjects.Add(root);
                }

                //Set data
                data.amountOfGrenadesLeft = amountOfGrenadesAtStart;

                //Return runtime data
                return data;
            }

            public override void SetupThirdPerson(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, object runtimeData, int[] attachments)
            {
                GrenadeControllerRuntimeData data = runtimeData as GrenadeControllerRuntimeData;

                //Setup the third person prefab
                GameObject tpRuntime = Instantiate(thirdPersonPrefab, pb.thirdPersonPlayerModel.weaponsInHandsGo, false);
                //Set Scale
                tpRuntime.transform.localScale = Vector3.one;

                //Setup renderer
                data.tpGrenadeRenderer = tpRuntime.GetComponent<Kit_ThirdPersonGrenadeRenderer>();
                data.tpGrenadeRenderer.visible = false;
                if (!pb.isBot)
                {
                    //Make it shadows only
                    data.tpGrenadeRenderer.shadowsOnly = true;
                }

                //Add to the list
                data.instantiatedObjects.Add(tpRuntime);
            }

            public override object SetupThirdPersonOthers(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, int[] attachments)
            {
                //Create runtime data (for replication)
                GrenadeControllerOthersRuntimeData data = new GrenadeControllerOthersRuntimeData();

                //Setup the third person prefab
                GameObject tpRuntime = Instantiate(thirdPersonPrefab, pb.thirdPersonPlayerModel.weaponsInHandsGo, false);
                //Set Scale
                tpRuntime.transform.localScale = Vector3.one;

                data.instantiatedObjects.Add(tpRuntime);

                //Setup renderer
                data.tpGrenadeRenderer = tpRuntime.GetComponent<Kit_ThirdPersonGrenadeRenderer>();
                data.tpGrenadeRenderer.visible = false;

                //Return the data
                return data;
            }

            public override void SetupValues(int id)
            {
                //Get our ID
                gameGunID = id;
            }

            public override float SpeedMultiplier(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 1f;
            }

            public override int WeaponState(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0;
            }

            public override int GetWeaponType(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 3;
            }
        }
    }
}