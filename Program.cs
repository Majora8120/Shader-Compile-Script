using System.Diagnostics;

namespace Shader_Compile_Script
{
    internal class Program
    {
        public const uint DEFAULT_MAX_BATCH_SIZE = 10; 
        static void Main(string[] args)
        {
            string? searchPath = null;
            string? glslcPath = null;
            string? slangcPath = null;
            string? slangcTarget = null;
            string? slangcInputExtensions = null;
            string? slangcOutputExtension = null;
            string? slangcArgs = null;
            uint maxBatchSize = DEFAULT_MAX_BATCH_SIZE;
            bool waitWhenFinished = false;

            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                if (args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i += 2)
                    {
                        switch (args[i])
                        {
                            case "-dir":
                                searchPath = args.ElementAt(i + 1);
                                break;
                            case "-glslc":
                                glslcPath = args.ElementAt(i + 1);
                                break;
                            case "-slangc":
                                slangcPath = args.ElementAt(i + 1);
                                break;
                            case "-slangc_target":
                                slangcTarget = args.ElementAt(i + 1);
                                break;
                            case "-slangc_in_ext":
                                slangcInputExtensions = args.ElementAt(i + 1);
                                break;
                            case "-slangc_out_ext":
                                slangcOutputExtension = args.ElementAt(i + 1);
                                break;
                            case "-slangc_args":
                                slangcArgs = args.ElementAt(i + 1);
                                break;
                            case "-max_batch_size":
                                maxBatchSize = UInt32.Parse(args.ElementAt(i + 1));
                                break;
                            case "-wait":
                                waitWhenFinished = true;
                                break;
                            default:
                                throw new ArgumentException("Error: Invalid argument(s) provided");
                        }
                    }
                }

                if (searchPath is null)
                {
                    throw new ArgumentException("Error: No searchPath provided");
                }
                if (!Directory.Exists(searchPath))
                {
                    throw new DirectoryNotFoundException($"Error: {searchPath} does not exist.");
                }
                if (maxBatchSize == 0)
                {
                    throw new ArgumentException("Error: Max batch size cannot be 0");
                }

                if (glslcPath is not null && File.Exists(glslcPath))
                {
                    List<string> glslcFiles = GetGlslcShaders(searchPath);
                    ProcessGlslShaders(glslcFiles, glslcPath, maxBatchSize);
                }
                else
                {
                    Console.WriteLine("Glslc: Skipping | Argument not provided or file not found");
                }

                if (slangcPath is not null && File.Exists(slangcPath))
                {
                    if (slangcArgs is not null)
                        slangcArgs = slangcArgs.Trim('"');
                    if (slangcArgs is null)
                        slangcArgs = "";
                    if (slangcTarget is null)
                        throw new ArgumentException("Slangc: Error: No target was provided");
                    if (slangcInputExtensions is null)
                        throw new ArgumentException("Slangc: Error: No input extension(s) provided");
                    if (slangcOutputExtension is null)
                        throw new ArgumentException("Slangc: Error: No output extension provided");

                    List<string> slangShaders = GetSlangShaders(searchPath, slangcInputExtensions);
                    ProcessSlangShaders(slangShaders, slangcPath, slangcTarget, slangcOutputExtension, slangcArgs, maxBatchSize);
                }
                else
                {
                    Console.WriteLine("Slangc: Skipping | Argument not provided or file not found");
                }

                stopwatch.Stop();
                TimeSpan timeSpan = stopwatch.Elapsed;
                string time = String.Format("{0:00} minutes and {1:00} seconds", timeSpan.Minutes, timeSpan.Seconds);
                Console.WriteLine($"Finished in {time}!");
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

        static List<string> GetGlslcShaders(string searchPath)
        {
            List<string> files =
            [
                .. Directory.GetFiles(searchPath, "*.vert", SearchOption.AllDirectories),
                .. Directory.GetFiles(searchPath, "*.frag", SearchOption.AllDirectories),
                .. Directory.GetFiles(searchPath, "*.tesc", SearchOption.AllDirectories),
                .. Directory.GetFiles(searchPath, "*.tese", SearchOption.AllDirectories),
                .. Directory.GetFiles(searchPath, "*.geom", SearchOption.AllDirectories),
                .. Directory.GetFiles(searchPath, "*.comp", SearchOption.AllDirectories),
            ];
            return files;
        }
        static void ProcessGlslShaders(List<string> files, string glslcPath, uint maxBatchSize)
        {
            Console.WriteLine($"Glslc: {files.Count} shaders found");
            List<Process> processes = new List<Process>();
            foreach (string file in files)
            {
                if (processes.Count >= maxBatchSize)
                {
                    foreach (Process pro in processes)
                    {
                        pro.WaitForExit();
                    }
                    processes.Clear();
                }

                Console.WriteLine($@"Glslc: Compiling ""{file}""");
                Process process = new Process();
                process.StartInfo.FileName = glslcPath;
                process.StartInfo.Arguments = $@"""{file}"" -o ""{file}"".spv";
                process.Start();
                processes.Add(process);
            }
            foreach (Process process in processes)
            {
                process.WaitForExit();
            }
            Console.WriteLine("Glslc: Finished!");
        }

        static List<string> GetSlangShaders(string searchPath, string extensions)
        {
            extensions = extensions.Trim('"');
            extensions = extensions.Replace(".", "");
            char[] splitChars = [' '];
            string[] extList = extensions.Split(splitChars);
            List<string> files = new List<string>();
            foreach (string ext in extList)
            {
                files.AddRange(Directory.GetFiles(searchPath, $"*.{ext}", SearchOption.AllDirectories));
            }
            return files;
        }
        static void ProcessSlangShaders(List<string> shaders, string slangcPath, string target, string extension, string otherArgs, uint maxBatchSize)
        {
            Console.WriteLine($"Slangc: {shaders.Count} shaders found");
            List<Process> processes = new List<Process>();
            extension = extension.Replace(".", "");
            foreach (string shader in shaders)
            {
                if (processes.Count >= maxBatchSize)
                {
                    foreach (Process pro in processes)
                    {
                        pro.WaitForExit();
                    }
                    processes.Clear();
                }

                Console.WriteLine($@"Slangc: Compiling ""{shader}""");
                Process process = new Process();
                process.StartInfo.FileName = slangcPath;
                process.StartInfo.Arguments = $@"""{shader}"" -target {target} -o ""{shader.Replace(".slang", "")}"".{extension} {otherArgs}";
                process.Start();
                processes.Add(process);
            }
            foreach (Process process in processes)
            {
                process.WaitForExit();
            }
            Console.WriteLine("Slangc: Finished!");
        }
    }
}