#nullable enable
using Crey.Data.Azure;
using Crey.Instrumentation.Web;
using Crey.Utils;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crey.Data.ImageTools
{
    public class AutoScaledImageHelper
    {
        private readonly ImageTool imageTools_;
        private readonly string imageBase_;
        private readonly BlobContainer blob_;
        private readonly string rawExt_;
        private readonly Dictionary<string, (int, int)> sizeMap_;

        ///  The storage for the source image
        public string RawName => $"{imageBase_}.{rawExt_}";
        public string SizedPrefix => $"{imageBase_}-";
        public string SizedName(string size, string ext) => $"{SizedPrefix}{size}.{ext}";


        public AutoScaledImageHelper(
            BlobContainer blob,
            ImageTool imageTools,
            string imagePrefix,
            string rawExt,
            Dictionary<string, (int, int)> sizeMap
            )
        {
            imageTools_ = imageTools;
            imageBase_ = imagePrefix;
            blob_ = blob;
            rawExt_ = rawExt;
            sizeMap_ = sizeMap;
        }

        public ReadOnlyMemory<byte> PrepareImage(ReadOnlyMemory<byte> rawData)
        {
            try
            {
                return imageTools_.Recompress(rawData, GetFormatFor(rawExt_));
            }
            catch
            {
                throw new InvalidArgumentException($"Unsupported source image");
            }
        }

        public async Task<string> SaveImage(ReadOnlyMemory<byte> imageData)
        {
            var pictureBlob = await blob_.UploadAsync(RawName, imageData);
            await blob_.DeleteByPrefixAsync(SizedPrefix);
            return pictureBlob.GetETagString();
        }

        public async Task<BinaryContent> GetRawImage(string etag)
        {
            return (await blob_.DownloadBinaryAsync(RawName, etag, GetMimeFor(rawExt_)))
                ?? throw new ItemNotFoundException($"Image not found");
        }

        public async Task<BinaryContent> GetImage(string size, string ext, string etag)
        {
            if (size.Any(c => !(char.IsLetterOrDigit(c) || c == '_')))
                throw new ItemNotFoundException($"Invalid image size: {size}");

            // try to load sized
            var sizedName = SizedName(size, ext);
            var result = await blob_.DownloadBinaryAsync(sizedName, etag, GetMimeFor(ext));
            if (result != null)
                return result;

            // create cached sized images from raw
            var rawImage = await blob_.DownloadBinaryAsync(RawName, etag, GetMimeFor(rawExt_))
                ?? throw new ItemNotFoundException($"Image not found");

            if (size == "raw" && ext == rawExt_)
            {
                // return just what we have
                return rawImage;
            }

            var format = GetFormatFor(ext);
            var inputImage = new ReadOnlyMemory<byte>(rawImage.Data);
            ReadOnlyMemory<byte> imageResult = null;
            if (size == "raw")
            {
                //only (re)compress, preserve original size
                imageResult = imageTools_.Recompress(inputImage, format);
            }
            else if (sizeMap_.TryGetValue(size, out var imageSize))
            {
                // resize and change compression
                imageResult = imageTools_.Resize(inputImage, imageSize.Item1, imageSize.Item2, format);
            }
            else
                throw new ItemNotFoundException($"Invalid thumbnail size: {size}");

            var blob = await blob_.UploadAsync(sizedName, imageResult);
            return new BinaryContent { Data = imageResult.ToArray(), ContentHash = blob.GetContentHash(), MimeType = GetMimeFor(ext) };
        }


        private string GetMimeFor(string ext)
        {
            if (ext == "jpg")
            {
                return "image/jpeg";
            }
            else if (ext == "webp")
            {
                return "image/webp";
            }
            else if (ext == "jp2")
            {
                return "image/jp2";
            }
            else if (ext == "png")
            {
                return "image/png";
            }
            else
            {
                throw new InvalidArgumentException("Unsupported extension");
            }
        }

        private MagickFormat GetFormatFor(string ext)
        {
            if (ext == "jpg")
            {
                return MagickFormat.Jpeg;
            }
            else if (ext == "webp")
            {
                return MagickFormat.WebP;
            }
            else if (ext == "jp2")
            {
                return MagickFormat.Jp2;
            }
            else if (ext == "png")
            {
                return MagickFormat.Png;
            }
            else
            {
                throw new InvalidArgumentException("Unsupported extension");
            }
        }
    }
}
