using System.Diagnostics;

namespace Shader_Compile_Script
{
    internal class Program
    {
        const uint DEFAULT_MAX_BATCH_SIZE = 10U;
        static uint MaxBatchSize = DEFAULT_MAX_BATCH_SIZE;
        static void Main(string[] args)
        {
            string? inputDir = null;
            string? outputDir = null;
            string? slangcPath = null;
            string? slangcTarget = null;
            string? slangcInputExtensions = null;
            string? slangcOutputExtension = null;
            string slangcArgs = "";
            bool waitWhenFinished = true;

            try
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                
                if (args.Length > 0)
                {
                    for (int i = 0; i < args.Length;)
                    {
                        switch (args[i])
                        {
                            // Main args
                            case "-in_dir":
                                inputDir = args.ElementAt(i + 1);
                                i += 2;
                                break;
                            case "-out_dir":
                                outputDir = args.ElementAt(i + 1);
                                i += 2;
                                break;
                            case "-max_batch_size":
                                MaxBatchSize = uint.Parse(args.ElementAt(i + 1));
                                i += 2;
                                break;
                            case "-no_wait":
                                waitWhenFinished = false;
                                i += 1;
                                break;
                            // Slangc args
                            case "-slangc":
                                slangcPath = args.ElementAt(i + 1);
                                i += 2;
                                break;
                            case "-slangc_target":
                                slangcTarget = args.ElementAt(i + 1);
                                i += 2;
                                break;
                            case "-slangc_in_ext":
                                slangcInputExtensions = args.ElementAt(i + 1);
                                i += 2;
                                break;
                            case "-slangc_out_ext":
                                slangcOutputExtension = args.ElementAt(i + 1);
                                i += 2;
                                break;
                            case "-slangc_args":
                                slangcArgs = args.ElementAt(i + 1);
                                i += 2;
                                break;
                            
                            default:
                                throw new ArgumentException("ERROR::Invalid argument(s) provided");
                        }
                    }
                }

                if (inputDir is null)
                {
                    throw new ArgumentException("ERROR::No input directory provided");
                }
                if (!Directory.Exists(inputDir))
                {
                    throw new DirectoryNotFoundException($"ERROR::{inputDir} does not exist.");
                }
                if (outputDir is not null && !Directory.Exists(outputDir))
                {
                    throw new DirectoryNotFoundException($"ERROR::{outputDir} does not exist.");
                }
                if (MaxBatchSize == 0)
                {
                    throw new ArgumentException("ERROR::Max batch size cannot be 0");
                }

                if (slangcPath is not null && File.Exists(slangcPath))
                {
                    slangcArgs = slangcArgs.Trim('"');
                    if (slangcTarget is null)
                        throw new ArgumentException("ERROR::No target was provided");
                    if (slangcInputExtensions is null)
                        throw new ArgumentException("ERROR::No input extension(s) provided");
                    if (slangcOutputExtension is null)
                        throw new ArgumentException("ERROR::No output extension provided");

                    List<string> slangShaders = GetSlangShaders(inputDir, slangcInputExtensions);
                    ProcessSlangShaders(slangShaders, slangcPath, slangcTarget, outputDir, slangcOutputExtension, slangcArgs);
                }
                else
                {
                    Console.WriteLine("ERROR::No valid slangc path was provided");
                }

                stopwatch.Stop();
                TimeSpan timeSpan = stopwatch.Elapsed;
                string time = $"{timeSpan.Minutes} minutes and {timeSpan.Seconds} seconds";
                Console.WriteLine($"INFO::Finished in {time}!");
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }
            if (waitWhenFinished)
            {
                Console.ReadLine();
            }
        }

        static List<string> GetSlangShaders(string dir, string exts)
        {
            string extensions = exts.Trim('"');
            extensions = extensions.Replace(".", "");
            char[] splitChars = [' '];
            string[] extList = extensions.Split(splitChars);
            List<string> files = new();
            foreach (string ext in extList)
            {
                files.AddRange(Directory.GetFiles(dir, $"*.{ext}", SearchOption.AllDirectories));
            }
            return files;
        }
        static void ProcessSlangShaders(List<string> shaders, string slangc, string target, string? outDir, string outExt, string args)
        {
            Console.WriteLine($"INFO::{shaders.Count} shader(s) found");
            List<Process> processes = new();
            string extension = outExt.Replace(".", "");
            foreach (string shader in shaders)
            {
                if (processes.Count >= MaxBatchSize)
                {
                    foreach (Process pro in processes)
                    {
                        pro.WaitForExit();
                    }
                    processes.Clear();
                }

                string outputPath = "";
                if (outDir is not null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(shader);
                    string fullOutputDir = Path.GetFullPath(outDir);
                    outputPath = Path.Combine(fullOutputDir, fileName) + $".{extension}";
                }
                else
                {
                    // Will place the output shader in the same dir as the input shader
                    string fileName = Path.GetFileNameWithoutExtension(shader);
                    // Could not get GetDirectoryName() to return null in my testing
                    string fullOutputDir = Path.GetFullPath(Path.GetDirectoryName(shader) ?? throw new ArgumentNullException("ERROR::Unexpected null argument"));
                    outputPath = Path.Combine(fullOutputDir, fileName) + $".{extension}";
                }

                Process process = new();
                process.StartInfo.FileName = slangc;
                process.StartInfo.Arguments = $@"""{shader}"" -target {target} -o ""{outputPath}"" {args}";
                process.Start();
                processes.Add(process);
                Console.WriteLine($@"INFO::Compiling ""{shader}""");
            }
            foreach (Process process in processes)
            {
                process.WaitForExit();
            }
        }
    }
}