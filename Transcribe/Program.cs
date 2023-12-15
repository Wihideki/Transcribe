﻿// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.IO;
using NAudio.MediaFoundation;
using Whisper.net;
using Whisper.net.Ggml;

public class Program
{
    // This example shows how to use the NAudio library to convert an mp3 file to a wav file with 16Khz sample rate and then use the Whisper library to process the wav file.
    public static async Task Main(string[] args)
    {

        //Setar variaveis
        string modelo = "tiny"; //tiny, base, small, medium, largev1, largev2, largev3
        var mp3FileName = "C:\\Whisper\\video2.mkv"; //path do arquivo
        string pathDestino = "C:\\Whisper"; //pasta destino
        string nomeArquivo = "teste.txt"; //nome da transcricao


        GgmlType ggmlType = GgmlType.LargeV1;
        string modelFileName = "";
        switch (modelo)
        {
            case "tiny":
                ggmlType = GgmlType.Tiny;
                modelFileName = "ggml-tiny.bin";
                break;
            case "base":
                ggmlType = GgmlType.Base;
                modelFileName = "ggml-base.bin";
                break;
            case "small":
                ggmlType = GgmlType.Small;
                modelFileName = "ggml-small.bin";
                break;
            case "medium":
                ggmlType = GgmlType.Medium;
                modelFileName = "ggml-medium.bin";
                break;
            case "largev1":
                ggmlType = GgmlType.LargeV1;
                modelFileName = "ggml-largev1.bin";
                break;
            case "largev2":
                ggmlType = GgmlType.LargeV2;
                modelFileName = "ggml-largev2.bin";
                break;
            case "largev3":
                ggmlType = GgmlType.LargeV3;
                modelFileName = "ggml-largev3.bin";
                break;

        }
        // We declare three variables which we will use later, ggmlType, modelFileName and mp3FileName
        //var ggmlType = GgmlType.LargeV2;
        //var modelFileName = "ggml-largev2.bin";
        //var mp3FileName = "C:\\Whisper\\video.mp4";

        // This section detects whether the "ggml-base.bin" file exists in our project disk. If it doesn't, it downloads it from the internet
        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        // This section creates the whisperFactory object which is used to create the processor object.
        using var whisperFactory = WhisperFactory.FromPath(modelFileName);

        // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        // This section opens the mp3 file and converts it to a wav file with 16Khz sample rate.
        using var fileStream = File.OpenRead(mp3FileName);

        using var wavStream = new MemoryStream();

        if (mp3FileName.Contains(".mp3"))
        {
            using var reader = new Mp3FileReader(fileStream);
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
            WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());

            // This section sets the wavStream to the beginning of the stream. (This is required because the wavStream was written to in the previous section)
            wavStream.Seek(0, SeekOrigin.Begin);
        }
        if (mp3FileName.Contains(".wav"))
        {
            using var reader = new WaveFileReader(fileStream);
            
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
            WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());

            // This section sets the wavStream to the beginning of the stream. (This is required because the wavStream was written to in the previous section)
            wavStream.Seek(0, SeekOrigin.Begin);
        }
        if (mp3FileName.Contains(".mp4") || mp3FileName.Contains(".mkv")) 
        {
            
            using var reader = new MediaFoundationReader(mp3FileName);
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
            WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());

            wavStream.Seek(0, SeekOrigin.Begin);


        }
        /*
        // This section processes the audio file and prints the results (start time, end time and text) to the console.
        await foreach (var result in processor.ProcessAsync(wavStream))
        {
            Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
        }
        */
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(pathDestino, nomeArquivo)))
        {
            await foreach (var result in processor.ProcessAsync(wavStream))
            {
                outputFile.WriteLine($"{result.Start}->{result.End}: {result.Text}");
                Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            }
        }

    }
    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}

