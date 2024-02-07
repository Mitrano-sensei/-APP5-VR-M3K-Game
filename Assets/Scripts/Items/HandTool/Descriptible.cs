using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hoverable))]
public class Descriptible : MonoBehaviour
{
    private Hoverable _hoverable;

    [SerializeField, TextArea] private string _description;

    void Start()
    {
        _hoverable = GetComponent<Hoverable>();

        _hoverable.OnHoverEnter.AddListener(OnHoverEnter);
        _hoverable.OnHoverExit.AddListener(OnHoverExit);
    }

    public void OnHoverEnter(OnHoverEnterEvent e)
    {
        var handtool = e.HoveredWith?.GetComponent<HandTool>();
        if (handtool == null)
            return;

        handtool.OpenDescription(_description);
    }

    public void OnHoverExit(OnHoverExitEvent e)
    {
        var handtool = e.HoveredWith?.GetComponent<HandTool>();
        if (handtool == null)
            return;

        handtool.CloseDescription();
    }
}
