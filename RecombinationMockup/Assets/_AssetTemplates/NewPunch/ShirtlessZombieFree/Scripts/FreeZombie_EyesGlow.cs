using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeZombie_EyesGlow : MonoBehaviour
{



    private int eyesTyp;
    public Material[] BodyMaterials = new Material[1];

    public enum EyesGlow
    {
        No,
        Yes
    }


    public EyesGlow eyesGlow;

    void OnValidate()
    {
        if (eyesGlow == 0)
        {



            BodyMaterials[0].DisableKeyword("_EMISSION");
            BodyMaterials[0].SetFloat("_EmissiveExposureWeight", 1);
        }
        else
        {


            BodyMaterials[0].EnableKeyword("_EMISSION");
            BodyMaterials[0].SetFloat("_EmissiveExposureWeight", 0);

        }

    }
}
