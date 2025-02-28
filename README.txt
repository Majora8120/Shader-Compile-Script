A script that scans a folder and its subfolders for shaders and compiles them to spir-v with glslc.exe.

Usage:
-d [directory path] Sets a custom search path.
-g [file path] Sets a custom glslc.exe path.

The search path defaults to "./".
The glslc path defaults to "./glslc.exe" or "C:/VulkanSDK/[latest version installed]/Bin/glslc.exe".
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
2. Build it like any basic C# console app.