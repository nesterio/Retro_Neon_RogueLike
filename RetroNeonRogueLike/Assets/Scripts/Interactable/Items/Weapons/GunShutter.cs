using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GunShutter : MonoBehaviour
{
    [SerializeField] Transform shutterTrans;
    [Space(5)]
    float shutPos;
    [SerializeField] float openPos;
    [Space(5)]
    public float shutterSpeed = 0.2f;
    [Space(5)]
    [SerializeField] bool xShutterMove;

    void Awake() 
    {
        if (xShutterMove)
            shutPos = shutterTrans.localPosition.x;
        else
            shutPos = shutterTrans.localPosition.z;
    }

    public void PlayShutter() 
    {
        if(xShutterMove)
        shutterTrans.DOLocalMoveX(openPos, shutterSpeed /2, false)
        .OnComplete(() => shutterTrans.DOLocalMoveX(shutPos, shutterSpeed /2, false));

        else
        shutterTrans.DOLocalMoveZ(openPos, shutterSpeed / 2, false)
        .OnComplete(() => shutterTrans.DOLocalMoveZ(shutPos, shutterSpeed / 2, false));
    }
}
