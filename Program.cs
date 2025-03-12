using System.Diagnostics;

namespace Shader_Compile_Script
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string? glslcPath = null;
                string? slangcPath = null;
                string? searchPath = null;

                if (args.Length > 0)
                {
                    if (args.Length != 2 && args.Length != 4 && args.Length != 6)
                    {
                        throw new ArgumentException("Invalid amount of arguments provided");
                    }
                    for (int i = 0; i < args.Length; i += 2)
                    {
                        switch (args[i])
                        {
                            case "-dir":
                                searchPath = args[i + 1];
                                break;
                            case "-glslc":
                                glslcPath = args[i + 1];
                                break;
                            case "-slangc":
                                slangcPath = args[i + 1];
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
                    Console.WriteLine($@"Using ""{slangcPath}""");
                    List<string> slangcFiles = GetSlangShaders(searchPath);
                    ProcessSlangShaders(slangcFiles, slangcPath);
                }
                else
                {
                    Console.WriteLine("Skipping slangc | Argument not provided or file not found");
                }
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
            foreach (string file in files)
            {
                Console.WriteLine($@"Compiling ""{file}""");
                Process.Start(glslcPath, $@"""{file}"" -o ""{file}"".spv");
            }
        }
        static List<string> GetSlangShaders(string searchPath)
        {
            List<string> files =
            [
                .. Directory.GetFiles(searchPath, "*.slang", SearchOption.AllDirectories),
            ];
            return files;
        }
        static void ProcessSlangShaders(List<string> files, string slangcPath)
        {
            foreach (string file in files)
            {
                Console.WriteLine($@"Compiling ""{file}""");
                Process.Start(slangcPath, $@"""{file}"" -profile glsl_460 -target spirv -entry vertMain -o ""{file}"".vert.spv -matrix-layout-row-major");
                Process.Start(slangcPath, $@"""{file}"" -profile glsl_460 -target spirv -entry fragMain -o ""{file}"".frag.spv -matrix-layout-row-major");
            }
        }
    }
}