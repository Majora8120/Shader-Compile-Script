A script that scans a folder and its subfolders for shaders and compiles them to spir-v with glslc.
Supports Windows and Linux (only tested on Ubuntu).

Usage:
-d [directory path] Sets a custom search path.
-g [file path] Sets a custom glslc path.

The search path defaults to "./".
The glslc file path defaults to "./glslc(.exe)". 
The Windows build also looks in "C:/VulkanSDK/x.x.x.x/Bin/glslc.exe" if glslc.exe is not found in "./".
Outputs .spv file in the same directory as the inputted shader file.

Supported input file extensions:
.vert
.frag
.tesc
.tese
.geom
.comp

Building:
1. Install .Net SDK 8.0 or higher.
2. Download source code.
3. Run: dotnet build "[sourceDir]/Shader Compile Script.csproj" -r [platform]