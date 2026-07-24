using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;

internal class Program
{
    private static void Main(string[] args)
    {
        Intrinsics();
        Numerics();
    }

    private static void Numerics()
    {
        int n = 18;
        int[] array1 = new int[n];
        int[] array2 = new int[n];
        int[] array11 = new int[n];
        int[] array21 = new int[n];
        int[] result = new int[n];
        int[] result2 = new int[n];


        for (int i = 0; i < n; i++)
        {
            array1[i] = i;
            array2[i] = i * 2;

            array11[i] = i;
            array21[i] = i * 2;
        }

        // ArrayAddWithVector(array1, array2, result);
        // ArrayAddWithVector(array1, array2, result);
        // AddArraysWithoutSIMD(array11, array21, result2);
        // AddArraysWithoutSIMD(array11, array21, result2);
        // AddArraysWithoutSIMD(array11, array21, result2);
        // ArrayAddWithVector(array1, array2, result);
        // ArrayAddWithVector(array1, array2, result);
        // ArrayAddWithVector(array1, array2, result);
        // AddArraysWithoutSIMD(array11, array21, result2);
        // AddArraysWithoutSIMD(array11, array21, result2);


        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        ArrayAddWithVector(array1, array2, result);
        stopwatch.Stop();
        var t1 = stopwatch.ElapsedTicks;

        Console.WriteLine("with: {0}", t1);


        Console.WriteLine("***");
        foreach (var val in result)
        {
            Console.WriteLine(val);
        }
        return;

        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        AddArraysWithoutSIMD(array11, array21, result2);
        stopwatch2.Stop();
        var t2 = stopwatch2.ElapsedTicks;

        Console.WriteLine("without: {0}", t2);

        var d = t1 / (float)t2;
        Console.WriteLine($"increase: {d}");
    }

    static void Intrinsics()
    {
        float[] arrayA = new float[4] { 1f, 2f, 3f, 4f };
        float[] arrayB = new float[4] { 10f, 20f, 30f, 40f };
        float[] result = new float[4];

        AddArray(arrayA, arrayB, result);

        foreach (var val in result)
        {
            Console.WriteLine(val);
        }


        int n = 19;

        float[] arrayA2 = new float[n];
        float[] arrayB2 = new float[n];

        for (int i = 0; i < n; i++)
        {
            arrayA2[i] = i;
            arrayB2[i] = i * 2;
        }

        float[] result2 = new float[n];

        AddArray(arrayA2, arrayB2, result2);

        Console.WriteLine("***");
        foreach (var val in result2)
        {
            Console.WriteLine(val);
        }
    }

    static unsafe void AddArray(float[] arrayA, float[] arrayB, float[] result)
    {
        fixed (float* ptrA = arrayA, ptrB = arrayB, ptrResult = result)
        {
            for (int i = 0; i < arrayA.Length; i += 4)
            {
                var vecA = Sse.LoadVector128(ptrA + i);
                var vecB = Sse.LoadVector128(ptrB + i);

                var vecR = Sse.Add(vecA, vecB);
                Sse.Store(ptrResult + i, vecR);
            }
        }
    }

    static void ArrayAddWithVector<T>(T[] array1, T[] array2, T[] result) where T : INumber<T>
    {
        int i = 0;
        Vector<T> v1, v2, vResult;

        Console.WriteLine("count {0}", Vector<T>.Count);

        for (; i <= array1.Length - Vector<T>.Count; i += Vector<T>.Count)
        {
            v1 = new Vector<T>(array1, i);
            v2 = new Vector<T>(array2, i);
            vResult = v1 + v2; // SIMD operation
            vResult.CopyTo(result, i);
        }

        for (; i < array1.Length; i++)
        {
            result[i] = array1[i] + array2[i];
        }
    }

    static void AddArraysWithoutSIMD(int[] array1, int[] array2, int[] result)
    {
        for (int i = 0; i < array1.Length; i++)
        {
            result[i] = array1[i] + array2[i];
        }
    }
}