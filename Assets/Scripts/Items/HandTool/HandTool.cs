using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Pickable))]
public class HandTool : MonoBehaviour
{
    [SerializeField] private GameObject _screenHolder;
    [SerializeField] private TextMeshProUGUI _text;

    public void Start()
    {
        _screenHolder.transform.localScale = _screenHolder.transform.localScale.With(y:0);
        _screenHolder.SetActive(false);

        var pickable = GetComponent<Pickable>();
        pickable.OnPick.AddListener(() => CloseDescription());
        pickable.OnUnPick.AddListener(() => CloseDescription());
    }

    public void OpenDescription(string description)
    {
        _screenHolder.SetActive(true);
        _screenHolder.transform.DOScaleY(1f, .1f).SetEase(Ease.InFlash);
        _text.SetText(description);
        LogManager.Instance.Log("OpenDescription: " + description);
    }

    public void CloseDescription()
    {
        if (_screenHolder.activeSelf == false)
            return;

        var seq = DOTween.Sequence();
        seq.Append(_screenHolder.transform.DOScaleY(0f, .05f).SetEase(Ease.InFlash)).OnComplete(() => _screenHolder.SetActive(false));
    }
}
