using System.Diagnostics;
using System.Numerics;

internal class Program
{
    static Stopwatch sw1 = new();
    static Stopwatch sw2 = new();

    private static void Main(string[] args)
    {
        string path = @"./";
        string inputDir = Path.Combine(path, "images");
        string outDir = Path.Combine(path, "out");

        // Получаем все BMP файлы в inputDir
        string[] bmpFiles = Directory.GetFiles(inputDir, "*.bmp", SearchOption.TopDirectoryOnly);

        foreach (string inputFile in bmpFiles)
        {
            // Формируем имя выходного файла с тем же именем
            string fileName = Path.GetFileName(inputFile);
            string output = Path.Combine(outDir, fileName);

            ProcessBmp(inputFile, output);
        }

        Console.WriteLine("Готово!");
    }

    // ---------- Обработка BMP ----------
    public static void ProcessBmp(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
            return;

        byte[] fileData = File.ReadAllBytes(inputPath);
        int pixelOffset = BitConverter.ToInt32(fileData, 10);
        int pixelDataLength = fileData.Length - pixelOffset;

        byte[] pixels = new byte[pixelDataLength];
        Array.Copy(fileData, pixelOffset, pixels, 0, pixelDataLength);

        Sw_SIMD(pixels);

        Array.Copy(pixels, 0, fileData, pixelOffset, pixelDataLength);
        File.WriteAllBytes(outputPath, fileData);
    }

    static void Sw_SIMD(byte[] pixels)
    {
        sw1.Start();

        InvertColorsSimd(pixels);

        sw1.Stop();
        Console.WriteLine($"SIMD: {sw1.ElapsedTicks}");
    }

    static void Sw_Scalar(byte[] pixels)
    {
        sw1.Start();

        InvertColorsScalar(pixels);

        sw1.Stop();
        Console.WriteLine($"Scalar: {sw1.ElapsedTicks}");
    }

    public static void InvertColorsSimd(byte[] pixels)
    {
        int vectorSize = Vector<byte>.Count; // 16 байт (SSE) или 32 байта (AVX2)
        int i = 0;

        // Создаём вектор, заполненный 255
        Vector<byte> maxBytes = new Vector<byte>(255);

        // Обрабатываем блоками по vectorSize
        for (; i <= pixels.Length - vectorSize; i += vectorSize)
        {
            // Загружаем 16 (или 32) байт в вектор
            var vec = new Vector<byte>(pixels, i);

            // Вычитаем из 255 (255 - value) — насыщение не требуется, т.к. результат в [0,255]
            var result = maxBytes - vec;

            // Сохраняем обратно
            result.CopyTo(pixels, i);
        }

        // Остаток (если длина не кратна vectorSize) — добиваем скалярно
        for (; i < pixels.Length; i++)
        {
            pixels[i] = (byte)(255 - pixels[i]);
        }
    }

    public static void InvertColorsScalar(byte[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = (byte)(255 - pixels[i]);
        }
    }
}
