using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Original script taken from https://dineshpunni.notion.site/Drawing-in-XR-2a2b46869e6f46c589092045a86e8a0a
// Video followed: https://www.youtube.com/watch?v=JZFfKTYSt7k
public class WandDrawer : XRGrabInteractable
{
    public static InteractorHandedness HeldHand = InteractorHandedness.None;
    
    [Space, Header("Drawing")]
    private InputDevice trackingHand;
    private bool ableToDraw = false;

    [SerializeField] private float minDistanceBeforeNewPoint = 0.008f;

    [SerializeField] private float tubeDefaultWidth = 0.010f;
    [SerializeField] private int tubeSides = 8;
    
    [SerializeField] private Material defaultLineMaterial;

    private Vector3 prevPointDistance = Vector3.zero;

    private List<Vector3> points = new List<Vector3>();
    private List<TubeRenderer> tubeRenderers = new List<TubeRenderer>();

    private TubeRenderer currentTubeRenderer;

    [SerializeField]
    private Transform drawPoint = null;
    private Transform debugDrawPoint = null;
    
    [SerializeField]
    private float hoverSpeed = 1F;
    [SerializeField]
    private float hoverDist = 1F;
    Coroutine hoverRoutine = null;
    
    [SerializeField]
    private bool enableMouseDrawing = false;

    bool isDrawing = false;
    bool wasDrawing = false;
    
    int particleCounter = 0;

    private void Start()
    {
#if UNITY_EDITOR
        debugDrawPoint = new GameObject("Debug_MouseDrawer").transform;
        debugDrawPoint.position = Vector3.zero;
#else
        enableMouseDrawing = false;
#endif
        
        AddNewTubeRenderer();
        Destroy(currentTubeRenderer.gameObject);
        
        if (hoverRoutine == null)
        {
            hoverRoutine = StartCoroutine(Hover());
        }
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
            hoverRoutine = null;
        }
        
        base.OnSelectEntering(args);

        // select current hand:
        WandDrawer.HeldHand = args.interactorObject.handedness;
        XRNode hand = HeldHand == InteractorHandedness.Left ? XRNode.LeftHand : XRNode.RightHand;
        trackingHand = InputDevices.GetDeviceAtXRNode(hand);
        ableToDraw = trackingHand.isValid;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);

        // stop ability to draw
        ableToDraw = false;

        // stop haptics
        XRNode hand = HeldHand == InteractorHandedness.Left ? XRNode.LeftHand : XRNode.RightHand;
        HeldHand = InteractorHandedness.None;
        InputDevices.GetDeviceAtXRNode(hand).StopHaptics();

        if (hoverRoutine == null)
        {
            hoverRoutine = StartCoroutine(Hover());
        }
    }

    private void Update()
    {
        CheckDrawState();
    }

    private void AddNewTubeRenderer()
    {
        points.Clear();
        GameObject go = new GameObject($"TubeRenderer__{tubeRenderers.Count}");
        go.transform.position = Vector3.zero;

        TubeRenderer goTubeRenderer = go.AddComponent<TubeRenderer>();
        tubeRenderers.Add(goTubeRenderer);

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.material = defaultLineMaterial;

        goTubeRenderer.SetPositions(points.ToArray());
        goTubeRenderer.radiusOne = tubeDefaultWidth;
        goTubeRenderer.radiusTwo = tubeDefaultWidth;
        goTubeRenderer.sides = tubeSides;

        currentTubeRenderer = goTubeRenderer;
    }

    private void CheckDrawState()
    {
        if (!DrawingManager.CanDraw)
            return;
        
        if (enableMouseDrawing)    // editor only
        {
            isDrawing = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
            if (isDrawing)
            {
                var pos = UnityEngine.InputSystem.Mouse.current.position;
                debugDrawPoint.transform.position = Camera.main.ScreenToWorldPoint((Vector3)pos.value + Vector3.forward);
            }
            else
            {
                debugDrawPoint.transform.localPosition = Vector3.zero;
            }
        }
        else if (ableToDraw) trackingHand.TryGetFeatureValue(CommonUsages.triggerButton, out isDrawing);

        if (isDrawing)
        {
            if (!wasDrawing)    // drawing started!
            {
                AddNewTubeRenderer();
            }

            UpdateTube(enableMouseDrawing ? debugDrawPoint : drawPoint);
        }
        if (!isDrawing && wasDrawing)   // drawing ended!
        {
            DrawingManager.CanDraw = false;
            currentTubeRenderer.OnDrawingFinished();
        }

        // update previous drawing state:
        wasDrawing = isDrawing;
    }

    private void UpdateTube(Transform refPoint)
    {
        if (prevPointDistance == Vector3.zero)
        {
            prevPointDistance = refPoint.transform.position;
        }

        if (Vector3.Distance(prevPointDistance, refPoint.transform.position) >= minDistanceBeforeNewPoint)
        {
            prevPointDistance = refPoint.transform.position;
            AddPoint(prevPointDistance);
        }
    }

    private void AddPoint(Vector3 position)
    {
        points.Add(position);
        currentTubeRenderer.SetPositions(points.ToArray());
        currentTubeRenderer.GenerateMesh();
        
        // particles for fun, but don't do them on every single vertex
        particleCounter = (particleCounter + 1) % 5;
        if (particleCounter == 0) MagicParticleManager.Instance?.GetParticle(MagicParticleManager.Particle.DrawingStars, position);
    }

    IEnumerator Hover()
    {
        float startY = transform.position.y;
        float time = 0F;
        while (true)
        {
            time = (time + Time.deltaTime) % hoverSpeed;
            float current = time / hoverSpeed;  // %
            float sinOffset = Mathf.Sin(current * 2 * Mathf.PI);
            
            transform.position = new Vector3(transform.position.x, startY + (sinOffset * hoverDist), transform.position.z);
            yield return null;
        }
    }
}