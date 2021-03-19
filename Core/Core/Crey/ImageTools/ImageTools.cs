using Crey.Utils;
using ImageMagick;
using ImageMagick.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Crey.ImageTools
{
    public class ImageTool
    {
        public DataSpan Recompress(DataSpan imageData, MagickFormat format = MagickFormat.Jpeg)
        {
            using (var img = new MagickImage(imageData.FullBufferRef))
            {
                img.Format = format;
                return new DataSpan(img.ToByteArray());
            }
        }

        public DataSpan Resize(DataSpan imageData, int width, int height, MagickFormat format = MagickFormat.Jpeg)
        {
            using (var img = new MagickImage(imageData.FullBufferRef))
            {
                img.Resize(width, height);
                img.Format = format;
                return new DataSpan(img.ToByteArray());
            }
        }
    }

    public static class ThumbnailToolsExtensions
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
