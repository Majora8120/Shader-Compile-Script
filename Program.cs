using System.Diagnostics;

namespace Shader_Compile_Script
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string[] SDKpaths = Directory.GetDirectories("C:/VulkanSDK/");
                string glslcPath = $"{SDKpaths.Last()}/Bin/glslc.exe"; ;
                Console.WriteLine($"Using SDK {SDKpaths.Last()}");
                if (!File.Exists(glslcPath))
                {
                    throw new Exception($"Couldn't find glslc.exe at {glslcPath}");
                }

                string searchPath = @"./";
                if (args.Length > 0)
                {
                    searchPath = args[0];
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
                Console.WriteLine($"{files.Count} shaders found");
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