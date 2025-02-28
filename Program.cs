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
                string glslcPath;
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
                    throw new Exception("glslc.exe couldn't be found");
                }
                Console.WriteLine($@"Using ""{glslcPath}""");

                string searchPath = @"./";
                if (args.Length > 0)
                {
                    if (Directory.Exists(args[0]))
                    {
                        searchPath = args[0];
                    }
                    else
                    {
                        throw new Exception($@"Invalid path provided in argument ""{args[0]}""");
                    }
                }

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