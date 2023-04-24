using System;
using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;


namespace PoissonBlending
{

    public class PoissonBlending : MonoBehaviour
    {
        public Texture2D source = null;
        public Texture2D mask = null;
        public Texture2D target = null;
        [SerializeField] ComputeShader compute = null;
        [SerializeField, Range(0, 15000)] int iterations = 800;
        private bool initDone = false;
        
        
        public RenderTexture tex;
        int kernelInit;
        int kernelPoisson;
        uint3 threads;

        static int _Source = Shader.PropertyToID("Source");
        static int _Target = Shader.PropertyToID("Target");
        static int _Result = Shader.PropertyToID("Result");
        static int _Mask = Shader.PropertyToID("Mask");

        void InitPoisson()
        {
            Debug.Assert(source.width == mask.width && source.width == target.width);
            Debug.Assert(source.height == mask.height && source.height == target.height);

            tex = new RenderTexture(source.width, source.height, 0);
            tex.enableRandomWrite = true;
            tex.Create();

            kernelPoisson = compute.FindKernel("PoissonBlending");
            kernelInit = compute.FindKernel("Init");

            threads = compute.GetThreadGroupSize(kernelPoisson);
            Debug.Log(source.width +" " + threads.x + "   " + source.width%threads.x);
            Debug.Assert(source.width % threads.x == 0);
            Debug.Assert(source.height % threads.y == 0);
            initDone = true; 
        }

    

       public void DoPoissonBlending()
        {
            if(!initDone) InitPoisson();
          //  var sw = Stopwatch.StartNew();

            // Init
            compute.SetTexture(kernelInit, _Target, target);
            compute.SetTexture(kernelInit, _Result, tex);
            compute.Dispatch(kernelInit, source.width / (int)threads.x, source.height / (int)threads.y, 1);

            // Poisson Blending
            compute.SetTexture(kernelPoisson, _Source, source);
            compute.SetTexture(kernelPoisson, _Mask, mask);
            compute.SetTexture(kernelPoisson, _Target, target);
            compute.SetTexture(kernelPoisson, _Result, tex);

            for (int i = 0; i < iterations; i++)
            {
                compute.Dispatch(kernelPoisson, source.width / (int)threads.x, source.height / (int)threads.y, 1);
            }

            //sw.Stop();
           // double d = (double)sw.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
          //  Debug.Log($"GPU: {d} ms");
        }

        void OnDestroy()
        {
            if(tex != null) tex.Release();
        }


    }
}
