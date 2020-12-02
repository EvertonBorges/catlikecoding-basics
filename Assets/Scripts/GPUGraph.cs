using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{

    const int maxResolution = 1000;

    public enum TransitionMode { Cycle, Random }

    [SerializeField]
    ComputeShader computeShader = default;
    [SerializeField]
    Material material = default;
    [SerializeField]
    Mesh mesh = default;

    [SerializeField, Range(10, maxResolution)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionEnum function = default;
    [SerializeField]
    TransitionMode transitionMode = TransitionMode.Cycle;
    [SerializeField, Min(0f)]
    private float functionDuration = 1f, transitionDuration = 1f;

    float duration;

    bool transitioning = false;

    FunctionLibrary.FunctionEnum transitionFunction;

    ComputeBuffer positionsBuffer;

    static readonly int positionId = Shader.PropertyToID("_Positions");
    static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    static readonly int stepId = Shader.PropertyToID("_Step");
    static readonly int timeId = Shader.PropertyToID("_Time");
    static readonly int transitionProgressId = Shader.PropertyToID("_TransitionProgress");
    static readonly int scaleId = Shader.PropertyToID("_Scale");

    void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
        }

        UpdateFunctionOnGPU();
    }

    private void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        computeShader.SetFloat(transitionProgressId, Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        var kernelIndex = (int) function + (int) (transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;
        computeShader.SetBuffer(kernelIndex, positionId, positionsBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);
        material.SetBuffer(positionId, positionsBuffer);
        material.SetVector(scaleId, new Vector4(step, 1f / step));
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    private void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionEnum(function) :
            FunctionLibrary.GetRandomFunctionEnumOtherThan(function);
    }

    void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
    }

    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

}
