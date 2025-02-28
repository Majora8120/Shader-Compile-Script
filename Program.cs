using System.Diagnostics;

namespace Shader_Compile_Script
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string SDKpath = "C:/VulkanSDK/";
                string? glslcPath = null;
                string? searchPath = null;

                if (args.Length > 0)
                {
                    if (args.Length != 2 && args.Length != 4)
                    {
                        throw new ArgumentException("Invalid amount of arguments provided");
                    }
                    for (int i = 0; i < args.Length; i += 2)
                    {
                        switch (args[i])
                        {
                            case "-d":
                                searchPath = args[i + 1];
                                break;
                            case "-g":
                                glslcPath = args[i + 1];
                                break;
                            default:
                                throw new ArgumentException("Invalid argument(s) provided");
                        }
                    }
                }

                if (searchPath == null)
                {
                    searchPath = "./";
                }

                if (glslcPath == null)
                {
                    if (File.Exists("./glslc.exe"))
                    {
                        glslcPath = "./glslc.exe";
                    }
                    else if (Directory.Exists(SDKpath) && Directory.GetDirectories(SDKpath).Length > 0)
                    {
                        string[] SDKpaths = Directory.GetDirectories(SDKpath);
                        glslcPath = $"{SDKpaths.Last()}/Bin/glslc.exe"; // Highest SDK version will be last in list
                    }
                    else
                    {
                        throw new FileNotFoundException("glslc.exe couldn't be found");
                    }
                }

                if (!Directory.Exists(searchPath))
                {
                    throw new DirectoryNotFoundException($"{searchPath} does not exist.");
                }
                if (!File.Exists(glslcPath))
                {
                    throw new FileNotFoundException($"{glslcPath} does not exist.");
                }

                Console.WriteLine($@"Using ""{glslcPath}""");

                List<string> files =
                [
                    .. Directory.GetFiles(searchPath, "*.vert", SearchOption.AllDirectories),
                    .. Directory.GetFiles(searchPath, "*.frag", SearchOption.AllDirectories),
                    .. Directory.GetFiles(searchPath, "*.tesc", SearchOption.AllDirectories),
                    .. Directory.GetFiles(searchPath, "*.tese", SearchOption.AllDirectories),
                    .. Directory.GetFiles(searchPath, "*.geom", SearchOption.AllDirectories),
                    .. Directory.GetFiles(searchPath, "*.comp", SearchOption.AllDirectories),
                ];
                Console.WriteLine($@"{files.Count} shaders found in ""{searchPath}""");
                foreach (string file in files)
                {
                    Console.WriteLine($@"Compiling ""{file}""");
                    Process.Start(glslcPath, $@"""{file}"" -o ""{file}"".spv");
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}