using System.Diagnostics;

namespace Shader_Compile_Script
{
    internal class Program
    {
        public const uint MAX_BATCH_SIZE = 10; 
        static void Main(string[] args)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                string? searchPath = null;
                string? glslcPath = null;
                string? slangcPath = null;
                string? slangcTarget = null;
                string? slangcArgs = null;

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
                            case "-target":
                                slangcTarget = args.ElementAt(i + 1);
                                break;
                            case "-slangc_args":
                                slangcArgs = args.ElementAt(i + 1);
                                break;
                            default:
                                throw new ArgumentException("Invalid argument(s) provided");
                        }
                    }
                }

                if (searchPath is null)
                {
                    throw new ArgumentException("No searchPath provided");
                }
                if (!Directory.Exists(searchPath))
                {
                    throw new DirectoryNotFoundException($"{searchPath} does not exist.");
                }

                if (glslcPath is not null && File.Exists(glslcPath))
                {
                    Console.WriteLine($@"Using ""{glslcPath}""");
                    List<string> glslcFiles = GetGlslcShaders(searchPath);
                    ProcessGlslShaders(glslcFiles, glslcPath);
                }
                else
                {
                    Console.WriteLine("Skipping glslc | Argument not provided or file not found");
                }

                if (slangcPath is not null && File.Exists(slangcPath))
                {
                    if (slangcArgs is not null)
                        slangcArgs = slangcArgs.Trim('"');
                    if (slangcArgs is null)
                        slangcArgs = "";
                    if (slangcTarget is null)
                        throw new ArgumentException("No Slangc target was provided");

                    Console.WriteLine($@"Using ""{slangcPath}""");
                    List<string> slangcFiles = GetSlangShaders(searchPath);
                    List<SlangShader> slangShaders = ScanSlangShaders(slangcFiles);
                    ProcessSlangShaders(slangShaders, slangcPath, slangcTarget, slangcArgs);
                }
                else
                {
                    Console.WriteLine("Skipping slangc | Argument not provided or file not found");
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
            Console.ReadLine();
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
        static void ProcessGlslShaders(List<string> files, string glslcPath)
        {
            List<Process> processes = new List<Process>();
            foreach (string file in files)
            {
                if (processes.Count >= MAX_BATCH_SIZE)
                {
                    foreach (Process pro in processes)
                    {
                        pro.WaitForExit();
                    }
                    processes.Clear();
                }

                Console.WriteLine($@"Compiling ""{file}""");
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
            Console.WriteLine($"Glslc finished. {files.Count} shaders compiled!");
        }
        
        struct SlangShader
        {
            public string filePath;
            public bool hasVertexStage;
            public bool hasFragmentStage;
            public bool hasComputeStage;
            public bool hasGeometryStage;
            public bool hasHullStage;
            public bool hasDomainStage;
            public bool hasMeshStage;
        }
        static List<string> GetSlangShaders(string searchPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(searchPath, $"*.slang", SearchOption.AllDirectories));
            return files;
        }
        static List<SlangShader> ScanSlangShaders(List<string> filePaths)
        {
            List<SlangShader> shaders = new List<SlangShader>();
            foreach (string path in filePaths)
            {
                SlangShader shader = new SlangShader();
                shader.filePath = path;
                StreamReader sr = new StreamReader(path);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    switch (line.ToString())
                    {
                        case @"[shader(""vertex"")]":
                            shader.hasVertexStage = true;
                            break;
                        case @"[shader(""fragment"")]":
                            shader.hasFragmentStage = true;
                            break;
                        case @"[shader(""compute"")]":
                            shader.hasComputeStage = true;
                            break;
                        case @"[shader(""geometry"")]":
                            shader.hasGeometryStage = true;
                            break;
                        case @"[shader(""hull"")]":
                            shader.hasHullStage = true;
                            break;
                        case @"[shader(""domain"")]":
                            shader.hasDomainStage = true;
                            break;
                        case @"[shader(""mesh"")]":
                            shader.hasMeshStage = true;
                            break;
                        default:
                            break;
                    }
                }
                sr.Close();
                shaders.Add(shader);
            }
            return shaders;
        }
        static void ProcessSlangShaders(List<SlangShader> shaders, string slangcPath, string target, string otherArgs)
        {
            List<Process> processes = new List<Process>();
            string extension = GetTargetExtension(target);
            foreach (SlangShader shader in shaders)
            {
                if (processes.Count >= MAX_BATCH_SIZE)
                {
                    foreach (Process process in processes)
                    {
                        process.WaitForExit();
                    }
                    processes.Clear();
                }

                Console.WriteLine($@"Compiling ""{shader.filePath}""");
                if (shader.hasVertexStage)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = slangcPath;
                    process.StartInfo.Arguments = $@"""{shader.filePath}"" -target {target} -o ""{shader.filePath.Replace(".slang", "")}"".vert.{extension} {otherArgs}";
                    process.Start();
                    processes.Add(process);
                }
                if (shader.hasFragmentStage)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = slangcPath;
                    process.StartInfo.Arguments = $@"""{shader.filePath}"" -target {target} -entry fragMain -o ""{shader.filePath.Replace(".slang", "")}"".frag.{extension} {otherArgs}";
                    process.Start();
                    processes.Add(process);
                }
                if (shader.hasComputeStage)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = slangcPath;
                    process.StartInfo.Arguments = $@"""{shader.filePath}"" -target {target} -entry compMain -o ""{shader.filePath.Replace(".slang", "")}"".comp.{extension} {otherArgs}";
                    process.Start();
                    processes.Add(process);
                }
                if (shader.hasGeometryStage)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = slangcPath;
                    process.StartInfo.Arguments = $@"""{shader.filePath}"" -target {target} -entry geomMain -o ""{shader.filePath.Replace(".slang", "")}"".geom.{extension} {otherArgs}";
                    process.Start();
                    processes.Add(process);
                }
                if (shader.hasHullStage)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = slangcPath;
                    process.StartInfo.Arguments = $@"""{shader.filePath}"" -target {target} -entry hullMain -o ""{shader.filePath.Replace(".slang", "")}"".hull.{extension} {otherArgs}";
                    process.Start();
                    processes.Add(process);
                }
                if (shader.hasDomainStage)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = slangcPath;
                    process.StartInfo.Arguments = $@"""{shader.filePath}"" -target {target} -entry domainMain -o ""{shader.filePath.Replace(".slang", "")}"".domain.{extension} {otherArgs}";
                    process.Start();
                    processes.Add(process);
                }
                if (shader.hasMeshStage)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = slangcPath;
                    process.StartInfo.Arguments = $@"""{shader.filePath}"" -target {target} -entry meshMain -o ""{shader.filePath.Replace(".slang", "")}"".mesh.{extension} {otherArgs}";
                    process.Start();
                    processes.Add(process);
                }
            }
            foreach (Process process in processes)
            {
                process.WaitForExit();
            }
            Console.WriteLine($"Slangc finished. {shaders.Count} shaders compiled!");
        }
        static string GetTargetExtension(string target)
        {
            switch (target)
            {
                case "spirv":
                    return "spv";
                case "glsl":
                    return "glsl";
                default:
                    return "shader";
            }
        }
    }
}