using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera thirdPersonCam;
    public CinemachineCamera firstPersonCam;

    private bool isFirstPerson = false;

    private void Start()
    {
        SetThirdPerson();
    }


    public void ToggleCamera()
    {
        isFirstPerson = !isFirstPerson;

        if (isFirstPerson)
            SetFirstPerson();
        else
            SetThirdPerson();
    }

    private void SetFirstPerson()
    {
        firstPersonCam.Priority = 20;
        thirdPersonCam.Priority = 10;
    }

    private void SetThirdPerson()
    {
        thirdPersonCam.Priority = 20;
        firstPersonCam.Priority = 10;
    }
}
