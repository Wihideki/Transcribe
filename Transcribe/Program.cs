// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.IO;
using NAudio.MediaFoundation;
using Whisper.net;
using Whisper.net.Ggml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.CompilerServices;

public class Program
{
    private static string _modelo = "largev3"; //tiny, base, small, medium, largev1, largev2, largev3
    private static string _path = "C:\\Whisper"; //Caminho da pasta monitada

    public static string Modelo { get { return _modelo; } set { _modelo = value; } }
    public static string Monitorar { get { return _path; } private set { } }
    public static void Main(string[] args)
    {
        //string monitorar = "C:\\Whisper";
        using var watcher = new FileSystemWatcher(Monitorar);
        watcher.Filter = "*.wav";
        watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
        watcher.IncludeSubdirectories = true;
        watcher.Created += OnCreated;
        //watcher.Changed += OnCreated;
        watcher.EnableRaisingEvents = true;
        using var watcher2 = new FileSystemWatcher(Monitorar);
        watcher2.Filter = "*.mp3";
        watcher2.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
        watcher2.IncludeSubdirectories = true;
        watcher2.Created += OnCreated;
        //watcher2.Changed += OnCreated;
        watcher2.EnableRaisingEvents = true;
        using var watcher3 = new FileSystemWatcher(Monitorar);
        watcher3.Filter = "*.mp4";
        watcher3.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
        watcher3.IncludeSubdirectories = true;
        watcher3.Created += OnCreated;
        //watcher3.Changed += OnCreated;
        watcher3.EnableRaisingEvents = true;
        using var watcher4 = new FileSystemWatcher(Monitorar);
        watcher4.Filter = "*.mkv";
        watcher4.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
        watcher4.IncludeSubdirectories = true;
        watcher4.Created += OnCreated;
        //watcher4.Changed += OnCreated;
        watcher4.EnableRaisingEvents = true;

        while (true) 
        {
            Console.WriteLine($"Monitorando a pasta {Monitorar} com o modelo {Modelo}.");
            Console.WriteLine($"Digite 1 para mudar o modelo, digite 0 para parar e sair: ");
            string? digito = Console.ReadLine();
            if (digito == "0") break;
            else if (digito == "1")
            {
                Console.WriteLine("Digite o modelo (tiny, base, small, medium, largev1, largev2 ou largev3): ");
                string? altModel = Console.ReadLine();
                Modelo = altModel;
                Console.WriteLine("Modelo alterado\n");
            }
            else
                Console.WriteLine("Não entendi, tente novamente.");
        }
        
    }

    private static async void OnCreated(object sender, FileSystemEventArgs e)
    {
        await MainAsync(sender, e);
    }
    // This example shows how to use the NAudio library to convert an mp3 file to a wav file with 16Khz sample rate and then use the Whisper library to process the wav file.
    private static async Task MainAsync(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"Iniciando a transcrição do arquivo {e.FullPath}");
        //Setar variaveis
        string modelo = Modelo;
        var mp3FileName = e.FullPath; //path do arquivo
        string pathDestino = e.FullPath.Replace("\\"+e.Name,""); //pasta destino
        string nomeArquivo = e.Name+"_transc.txt"; //nome da transcricao


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
            .WithLanguage("pt").WithEntropyThreshold(3.2f).WithTemperature(0.5f)
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

