using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SkiaSharp;
using Veldrid;
using Veldrid.MetalBindings;
using Vulkan;

var options = new GraphicsDeviceOptions
{
    PreferStandardClipSpaceYDirection = true,
    PreferDepthRangeZeroToOne = true
};

var graphicsDevice = GraphicsDevice.CreateVulkan(options);
var gdType = graphicsDevice.GetType();
var vkInstance = (VkInstance?)gdType.GetProperty("Instance")?.GetValue(graphicsDevice);
var vkPhysicalDevice = (VkPhysicalDevice?)gdType.GetProperty("PhysicalDevice")?.GetValue(graphicsDevice);
var vkDevice = (VkDevice?)gdType.GetProperty("Device")?.GetValue(graphicsDevice);
var vkQueue = (VkQueue?)gdType.GetProperty("GraphicsQueue")?.GetValue(graphicsDevice);
var graphicsQueueIndex = (uint?)gdType.GetProperty("GraphicsQueueIndex")?.GetValue(graphicsDevice);
IntPtr vkPhysicalDeviceFeatures;
unsafe
{
    vkPhysicalDeviceFeatures = (IntPtr)NativeMemory.Alloc((nuint)sizeof(VkPhysicalDeviceFeatures));
    VulkanNative.vkGetPhysicalDeviceFeatures(vkPhysicalDevice!.Value,
        (VkPhysicalDeviceFeatures*)vkPhysicalDeviceFeatures);
}

using (var grContext = GRContext.CreateVulkan(new GRVkBackendContext
       {
           VkInstance = vkInstance!.Value.Handle,
           VkPhysicalDevice = vkPhysicalDevice!.Value.Handle,
           VkDevice = vkDevice!.Value.Handle,
           VkQueue = vkQueue!.Value.Handle,
           GraphicsQueueIndex = graphicsQueueIndex!.Value,
           VkPhysicalDeviceFeatures = vkPhysicalDeviceFeatures,
           GetProcedureAddress = ((name, instance, device) =>
           {
               unsafe
               {
                   using var nameAddr = new FixedUtf8String(name);
                   if (instance != IntPtr.Zero)
                   {
                       return VulkanNative.vkGetInstanceProcAddr(instance, nameAddr.StringPtr);
                   }

                   if (device != IntPtr.Zero)
                   {
                       return VulkanNative.vkGetDeviceProcAddr(device, nameAddr.StringPtr);
                   }

                   return VulkanNative.vkGetInstanceProcAddr(vkInstance!.Value.Handle, nameAddr.StringPtr);
               }
           })
       }))
using (var skSurface = SKSurface.Create(grContext, false, new SKImageInfo(200, 200, SKColorType.Bgra8888)))
using (var paint = new SKPaint())
{
    paint.Color = SKColors.White;
    skSurface.Canvas.DrawCircle(100, 100, 30, paint);

    using var skImage = skSurface.Snapshot();
    using var skBitmap = SKBitmap.FromImage(skImage);
    using var stream = File.Create("output.png");
    skBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
}