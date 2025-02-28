A script that scans a folder and subfolders for shaders and compiles them to spir-v with glslc.exe.

Looks for glslc.exe in "./" and in Vulkan SDKs installed in "C:/VulkanSDK/".
Search path defaults to "./". A custom path can be provided with a command argument. Ex. "Shader Compile Script.exe" [directory]
Outputs .spv file in the same directory as the input file.

Supported input file extensions:
.vert
.frag
.tesc
.tese
.geom
.comp