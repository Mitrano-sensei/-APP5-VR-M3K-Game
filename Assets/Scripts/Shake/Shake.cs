using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Shake : MonoBehaviour
{
    [SerializeField] private float _shakeIntensity = 1f;
    [SerializeField] private float _shakeDuration = 1f;

    private Sequence _shakeSequence;
    void Start()
    {
        var cockpit = gameObject.transform;

        _shakeSequence = DOTween.Sequence();
        _shakeSequence
                .SetAutoKill(false)
                .Append(cockpit.DOLocalMoveX(cockpit.localPosition.x + _shakeIntensity, _shakeDuration / 3))
                .Append(cockpit.DOLocalMoveX(cockpit.localPosition.x - _shakeIntensity, _shakeDuration / 3))
                .Append(cockpit.DOLocalMoveX(cockpit.localPosition.x + (_shakeIntensity/2), _shakeDuration / 4))
                .SetEase(Ease.InOutBack)
                .Pause();
    }

    public void ShakeEverything()
    {
        Debug.Log("Shake dat bouty");
        ShakeCockpit();
        ShakeObjects();
    }

    private void ShakeCockpit()
    {
        _shakeSequence.Restart();
        _shakeSequence.Play();

    }

    private void ShakeObjects()
    {
    }
}
