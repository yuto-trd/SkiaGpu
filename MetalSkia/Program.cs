using SkiaSharp;
using Veldrid;
using Veldrid.MetalBindings;

var options = new GraphicsDeviceOptions
{
    PreferStandardClipSpaceYDirection = true,
    PreferDepthRangeZeroToOne = true
};

var graphicsDevice = GraphicsDevice.CreateMetal(options);
var gdType = graphicsDevice.GetType();
var mtlDevice = (MTLDevice?)gdType.GetProperty("Device")?.GetValue(graphicsDevice);
var mtlCommandQueue = (MTLCommandQueue?)gdType.GetProperty("CommandQueue")?.GetValue(graphicsDevice);

using (var grContext = GRContext.CreateMetal(new GRMtlBackendContext
       {
           DeviceHandle = mtlDevice!.Value.NativePtr,
           QueueHandle = mtlCommandQueue!.Value.NativePtr,
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