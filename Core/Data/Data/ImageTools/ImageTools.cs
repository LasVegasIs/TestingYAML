#nullable enable
using ImageMagick;
using ImageMagick.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Crey.Data.ImageTools
{
    public class ImageTool
    {
        public ReadOnlyMemory<byte> Recompress(ReadOnlyMemory<byte> imageData, MagickFormat format = MagickFormat.Jpeg)
        {
            using (var img = new MagickImage(imageData.ToArray()))
            {
                img.Format = format;
                var data = img.ToByteArray();
                return new ReadOnlyMemory<byte>(data, 0, data.Length);
            }
        }

        public ReadOnlyMemory<byte> Resize(ReadOnlyMemory<byte> imageData, int width, int height, MagickFormat format = MagickFormat.Jpeg)
        {
            using (var img = new MagickImage(imageData.ToArray()))
            {
                img.Resize(width, height);
                img.Format = format;
                var data = img.ToByteArray();
                return new ReadOnlyMemory<byte>(data, 0, data.Length);
            }
        }
    }

    public static class ImageToolExtensions
    {
        public static IServiceCollection AddImageTools(this IServiceCollection serviceCollection)
        {
            if (serviceCollection.Any(x => x.ServiceType == typeof(ImageTool)))
                return serviceCollection;

            ConfigurationFiles configFiles = ConfigurationFiles.Default;
            string temporaryDirectory = MagickNET.Initialize(configFiles);

            serviceCollection.AddSingleton<ImageTool>();

            return serviceCollection;
        }
    }
}
