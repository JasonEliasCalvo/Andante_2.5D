using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;


[System.Serializable]
public class Route
{
    public List<int> pointIndices;
}

public class Ship : MovableObject
{
    public InteractableOptions interactableOptions;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Rutas del barco (ordenadas en secuencia)")]
    [SerializeField] private List<Route> routes;

    private int currentRoute = 0;
    private int routeStep = 0;
    private bool isSkipping = false;

    [Header("Sounds")]
    public AudioClip shipBell, waterSound;
    private bool hasPlayedBellSound = false;

    private void OnEnable()
    {
        UIInputHandler.OnBoatSkipPressed += TrySkipRoute;
    }

    private void OnDisable()
    {
        UIInputHandler.OnBoatSkipPressed -= TrySkipRoute;
    }

    private void TrySkipRoute()
    {
        if (canMove && !isSkipping)
        {
            StartCoroutine(SkipRoute());
        }
    }


    protected override void Move()
    {
        if (!hasPlayedBellSound)
        {
            AudioManager.Instance.PlaySound(shipBell);
            hasPlayedBellSound = true;
        }

        interactableOptions.possibleInteract = false;
        GameManager.instance.GameEnd();
        List<int> currentPath = routes[currentRoute].pointIndices;
        Transform target = movePoints[currentPath[routeStep]];

        if (!UIManager.instance.skipPanel.activeSelf && !isSkipping)
        {
            UIManager.instance.ShowSkipPanel(true);
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 baseEuler = transform.rotation.eulerAngles;
            float targetY = lookRotation.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(baseEuler.x, targetY, baseEuler.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.001f)
        {
            routeStep++;
            if (routeStep >= currentPath.Count)
            {
                EndMove();
            }
        }
    }

    public void EndMove()
    {
        canMove = false;
        hasPlayedBellSound = false;
        interactableOptions.possibleInteract = true;
        UIManager.instance.ShowSkipPanel(false);
        GameManager.instance.GameStart();
        currentRoute = (currentRoute + 1) % routes.Count;
        routeStep = 0;
    }

    private IEnumerator SkipRoute()
    {
        isSkipping = true;
        UIManager.instance.ShowSkipPanel(false);

        GameManager.instance.StartFadeOut(() =>
        {
            List<int> currentPath = routes[currentRoute].pointIndices;
            Transform finalTarget = movePoints[currentPath[currentPath.Count - 1]];
            Vector3 oldPos = transform.position;

            transform.position = finalTarget.position;

            if (currentPath.Count >= 2)
            {
                Transform beforeLast = movePoints[currentPath[currentPath.Count - 2]];
                Vector3 lookDir = finalTarget.position - beforeLast.position;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(lookDir);
                    Vector3 baseEuler = transform.rotation.eulerAngles;
                    float targetY = lookRotation.eulerAngles.y;
                    Quaternion targetRotation = Quaternion.Euler(baseEuler.x, targetY, baseEuler.z);
                    transform.rotation = targetRotation;
                }

            }

            EndMove();

            Vector3 delta = transform.position - oldPos;
            var cam = GameManager.instance.DefaultCam.GetComponent<CinemachineCamera>();
            if (cam != null && cam.Follow != null)
            {
                cam.OnTargetObjectWarped(cam.Follow, delta);
            }
        });

        yield return new WaitForSeconds(GameManager.instance.fadeDuration + 0.1f);

        GameManager.instance.StartFadeIn(() =>
        {
            isSkipping = false;
        });
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
            StartSeaSound();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            other.transform.SetParent(null);
            StopSeaSound();
        }
    }
    public void StartSeaSound()
    {
        AudioManager.Instance.PlayLoopSound(waterSound);
    }
    public void StopSeaSound()
    {
        AudioManager.Instance.StopLoopSound();
    }
}


