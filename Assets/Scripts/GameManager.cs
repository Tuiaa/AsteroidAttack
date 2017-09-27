using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private GameObject _asteroid;
    [SerializeField]
    private GameObject _turret;
    [SerializeField]
    private GameObject _missile;
    [SerializeField]
    private GameObject _safetyZone;
    [SerializeField]
    private float _missileSpeed = 2f;
    [SerializeField]
    private float _safetyZoneCircleRadius = 2f;

    private Plane _planeForRaycast;

    // Line renderers for mouse drag and safetyzone area
    private LineRenderer _lineRend;
    private LineRenderer _lineRendCircle;
    private int segments = 50;

    private Vector3 _startPosition;
    private Vector3 _currentPosition;

    GameObject _spawnedAsteroid;
    private Vector3 _asteroidTrajectory;

    private void Start()
    {
        _lineRend = GetComponent<LineRenderer>();
        _lineRendCircle = _safetyZone.GetComponent<LineRenderer>();
        _lineRend.enabled = false;

        _safetyZone.transform.localScale = new Vector3(_safetyZoneCircleRadius, _safetyZoneCircleRadius, _safetyZoneCircleRadius);
        SphereCollider safetyZoneCollider = _safetyZone.GetComponent<SphereCollider>();
        safetyZoneCollider.radius = _safetyZoneCircleRadius;

        _lineRendCircle.positionCount = segments + 1;
        _lineRendCircle.useWorldSpace = false;
        CircleLineRenderer();
    }

    public void CircleLineRenderer()
    {
        _lineRendCircle.positionCount = (segments + 1);
        _lineRendCircle.useWorldSpace = false;

        float deltaTheta = (float)(2.0 * Mathf.PI) / segments;
        float theta = 0f;

        for(int i = 0; i < segments + 1; i++)
        {
            float x = _safetyZoneCircleRadius * Mathf.Cos(theta);
            float z = _safetyZoneCircleRadius * Mathf.Sin(theta);
            Vector3 pos = new Vector3(x, 0, z);
            _lineRendCircle.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _startPosition = GetMousePosition();
            if(_startPosition != Vector3.zero)
            {
                _lineRend.SetPosition(0, _startPosition);
                _lineRend.positionCount = 1;
                _lineRend.enabled = true;
            }
        }
        else if(Input.GetMouseButton(0))
        {
            _currentPosition = GetMousePosition();
            if(_currentPosition != Vector3.zero)
            {
                _lineRend.positionCount = 2;
                _lineRend.SetPosition(1, _currentPosition);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            _lineRend.enabled = false;
            _asteroidTrajectory = _currentPosition - _startPosition;

            _spawnedAsteroid = Instantiate(_asteroid, _currentPosition, Quaternion.identity);
            _spawnedAsteroid.GetComponent<Rigidbody2D>().velocity = _asteroidTrajectory;

            if(TrajectoryWithinSafetyZone())
            {
                float time = CalculateTime();
                Vector2 asteroidPos = CalculatePosition(time);

                GameObject missile = Instantiate(_missile, _turret.transform);
                missile.GetComponent<Rigidbody2D>().velocity = asteroidPos.normalized * _missileSpeed;
            }
        }
    }

    private Vector2 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _planeForRaycast = new Plane(Vector3.forward, Vector3.zero);
        float rayDistance;
        if(_planeForRaycast.Raycast(ray, out rayDistance))
        {
            return ray.GetPoint(rayDistance);
        }
        return Vector2.zero;
    }

    private bool TrajectoryWithinSafetyZone()
    {
        bool hitSafetyZone = false;
        RaycastHit objectHit;
        if(Physics.Raycast(_currentPosition, _asteroidTrajectory.normalized, out objectHit, 50))
        {
            hitSafetyZone = objectHit.transform.tag == "SafetyZone" ? true : false;
        }
        return hitSafetyZone;
    }

    private float CalculateTime()
    {
        Vector2 asteroidVelocity = _spawnedAsteroid.GetComponent<Rigidbody2D>().velocity;
        float missileVelocity = _missileSpeed;
        Vector2 asteroidStartPos = _spawnedAsteroid.transform.position;
        Vector2 missileStartPosition = Vector2.zero;

        // a, b and c of quadratic equation
        // I got these using the equation for calculating position
        // x = x0 + v0*t --> x_asteroidStartPos + v_asteroidStartVelocity * time = x_missileStartPos + v_missileStartSpeed * time
        // Time is the only unknown variable so that's what I calculated
        // After simplifying I got a quadratic equation
        float aa = (missileVelocity * missileVelocity);
        float ab = (asteroidVelocity.x * asteroidVelocity.x);
        float ac = (asteroidVelocity.y * asteroidVelocity.y);

        float a = (aa - ab - ac);

        float ba = (2 * asteroidVelocity.x * asteroidStartPos.x);
        float bb = (2 * asteroidVelocity.x * missileStartPosition.x);
        float bc = (2 * asteroidVelocity.y * asteroidStartPos.y);
        float bd = (2 * asteroidVelocity.y * missileStartPosition.y);

        float b = (-ba + bb - bc + bd);

        float ca = (asteroidStartPos.x - missileStartPosition.x) * (asteroidStartPos.x - missileStartPosition.x);
        float cb = (asteroidStartPos.y - missileStartPosition.y) * (asteroidStartPos.y - missileStartPosition.y);

        float c = (-ca - cb);

        float toMathf = ((b * b) - (4.0f * a * c));

        // Quadratic equation with two options
        float firstOption = ((-b) + (Mathf.Sqrt(toMathf))) / (2 * a);
        float secondOption = ((-b) - (Mathf.Sqrt(toMathf))) / (2 * a);

        if(firstOption > 0)
        {
            return firstOption;
        }
        else if(secondOption > 0)
        {
            return secondOption;
        }
        else
        {
            Debug.LogError("Asteroid not reachable!");
            return 0;
        }
    }

    // Calculates the position of asteroid at the time calculated earlier
    private Vector2 CalculatePosition(float time)
    {
        float posX = (_spawnedAsteroid.transform.position.x + _spawnedAsteroid.GetComponent<Rigidbody2D>().velocity.x * time);
        float posY = (_spawnedAsteroid.transform.position.y + _spawnedAsteroid.GetComponent<Rigidbody2D>().velocity.y * time);

        Vector2 asteroidPos = new Vector2(posX, posY);
        return asteroidPos;
    }
}
