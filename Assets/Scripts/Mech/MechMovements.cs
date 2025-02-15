using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechMovements : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _rotationSpeed = 10f;

    private bool _isMovingForward = false;
    private bool _isMovingBackward = false;
    private bool _isRotatingLeft = false;
    private bool _isRotatingRight = false;

    public float Speed { get => _speed; set => _speed = value; }
    public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }

    private void Update()
    {
        if (_isMovingForward)
        {
            // Raycasts 1m forward, if it hits something in obstacle layer, stop moving forward
            if (Physics.Raycast(transform.position, transform.forward, 1f, LayerMask.GetMask("Obstacle")))
                _isMovingForward = false;
            else
                transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        }
        else if (_isMovingBackward)
        {
            transform.Translate(Vector3.back * Speed * Time.deltaTime);
        }

        if (_isRotatingLeft)
        {
            transform.Rotate(Vector3.up * -RotationSpeed * Time.deltaTime);
        }
        else if (_isRotatingRight)
        {
            transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime);
        }
    }

    public void StartMovingForward()
    {
        _isMovingBackward = false;
        _isMovingForward = true;
    }

    public void StopMovingForward()
    {
        _isMovingForward = false;
    }

    public void StartMovingBackward()
    {
        _isMovingForward = false;
        _isMovingBackward = true;
    }

    public void StopMovingBackward()
    {
        _isMovingBackward = false;
    }

    public void StartRotatingLeft()
    {
        _isRotatingRight = false;
        _isRotatingLeft = true;
    }

    public void StopRotatingLeft()
    {
        _isRotatingLeft = false;
    }

    public void StartRotatingRight()
    {
        _isRotatingLeft = false;
        _isRotatingRight = true;
    }

    public void StopRotatingRight()
    {
        _isRotatingRight = false;
    }

    public void StopAllMovements()
    {
        _isMovingForward = false;
        _isMovingBackward = false;
        _isRotatingLeft = false;
        _isRotatingRight = false;
    }

    public void StopRotation()
    {
        _isRotatingLeft = false;
        _isRotatingRight = false;
    }

    public void StopMoving()
    {
        _isMovingForward = false;
        _isMovingBackward = false;
    }
}
