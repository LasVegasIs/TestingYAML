using Core.Functional;
using Crey.Contracts;
using Crey.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Crey.Web
{
    public static class RestApiDefaults
    {
        public const int DeprecatedRouteOrder = 999;
    }

    public class CreyActionResult<TOk, TError> : IConvertToActionResult
        where TError : Error
    {
        public Result<TOk, TError> Result { get; set; }

        public IActionResult Convert()
        {
            return Result.IntoObjectResult();
        }

        public static implicit operator CreyActionResult<TOk, TError>(TOk ok) => new CreyActionResult<TOk, TError> { Result = ok };
        public static implicit operator CreyActionResult<TOk, TError>(TError err) => new CreyActionResult<TOk, TError> { Result = err };
        public static implicit operator CreyActionResult<TOk, TError>(Result<TOk, TError> result) => new CreyActionResult<TOk, TError> { Result = result };
    }


    public class CreyBinaryResult<TError> : IConvertToActionResult
    where TError : Error
    {
        public Result<BinaryContent, TError> Result { get; set; }

        public IActionResult Convert()
        {
            return Result.IntoBinaryResult();
        }

        public static implicit operator CreyBinaryResult<TError>(BinaryContent ok) => new CreyBinaryResult<TError> { Result = ok };
        public static implicit operator CreyBinaryResult<TError>(TError err) => new CreyBinaryResult<TError> { Result = err };
        public static implicit operator CreyBinaryResult<TError>(Result<BinaryContent, TError> result) => new CreyBinaryResult<TError> { Result = result };
    }

    public static class HttpExtensions
    {
        // moved to standard
        public static string GetETag(this HttpRequest httpRequest)
        {
            StringValues eTag;
            if (httpRequest.Headers.TryGetValue(HeaderNames.IfNoneMatch, out eTag))
            {
                return eTag.ToString();
            }
            return null;
        }


        // moved to standard
        public static int IntoStatusCode(this ErrorCodes code)
        {
            return (int)code.IntoHttpStatusCode();
        }

        // moved to standard
        public static HttpStatusCode IntoHttpStatusCode(this ErrorCodes code)
        {
            switch (code)
            {
                case ErrorCodes.NoError: return HttpStatusCode.OK;
                case ErrorCodes.TimeOut: return HttpStatusCode.RequestTimeout;
                case ErrorCodes.ServerError: return HttpStatusCode.InternalServerError;
                case ErrorCodes.AccountNotFound: return HttpStatusCode.UnprocessableEntity;
                case ErrorCodes.ItemNotFound: return HttpStatusCode.UnprocessableEntity;
                case ErrorCodes.AccessDenied: return HttpStatusCode.Unauthorized;
                case ErrorCodes.InvalidArgument: return HttpStatusCode.PreconditionFailed;
                case ErrorCodes.CommandError: return HttpStatusCode.BadRequest;
                default: return HttpStatusCode.InternalServerError;
            }
        }

        public static ObjectResult IntoOkResult<TOk, TError>(this Result<TOk, TError> result) where TError : Error
        {
            return new OkObjectResult(result.Ok);
        }

        public static ObjectResult IntoErrorResult<TOk, TError>(this Result<TOk, TError> result) where TError : Error
        {
            return new ObjectResult(result.Error)
            {
                StatusCode = result.Error.ErrorCode.IntoStatusCode()
            };
        }

        public static ObjectResult IntoObjectResult<TOk, TError>(this Result<TOk, TError> result) where TError : Error
        {
            return result.Match(
                    ok =>
                    {
                        var res = new OkObjectResult(ok);
                        return res;
                    },
                    err =>
                    {
                        var res = new ObjectResult(err);
                        res.StatusCode = err.ErrorCode.IntoStatusCode();
                        return res;
                    });
        }

        public static ObjectResult Result<TOk>(this ControllerBase self, Result<TOk, ProblemDetails> result) =>
            result.Value switch
            {
                TOk ok => self.Ok(ok),
                ProblemDetails problem => self.Problem(problem.Detail, problem.Instance, problem.Status, problem.Title, problem.Type)
            };

        public static ActionResult IntoBinaryResult<TError>(this Result<BinaryContent, TError> result) where TError : Error
        {
            if (result == null)
                return new StatusCodeResult(StatusCodes.Status422UnprocessableEntity);

            return result.Match<ActionResult>(
               ok =>
               {
                   if (ok == null)
                       return new StatusCodeResult(StatusCodes.Status422UnprocessableEntity);

                   if (ok.Data == null)
                       return new StatusCodeResult(StatusCodes.Status304NotModified);

                   var mime = ok.MimeType ?? "application/octet-stream";
                   var res = new FileContentResult(ok.Data, mime);
                   if (!string.IsNullOrEmpty(ok.ContentHash))
                   {
                       EntityTagHeaderValue entityTagHeaderValue;
                       bool isValid = EntityTagHeaderValue.TryParse($"\"{ok.ContentHash}\"", out entityTagHeaderValue);
                       if (!isValid)
                       {
                           isValid = EntityTagHeaderValue.TryParse(ok.ContentHash, out entityTagHeaderValue);
                       }

                       if (!isValid)
                       {
                           throw new ServerErrorException($"Unprocessable ETag: {ok.ContentHash}");
                       }

                       res.EntityTag = entityTagHeaderValue;
                   }
                   return res;
               },
               err =>
               {
                   var res = new ObjectResult(err);
                   res.StatusCode = err.ErrorCode.IntoStatusCode();
                   return res;
               });
        }

        public static ActionResult IntoActionResult(this BinaryContent content)
        {
            if (content == null)
                return new StatusCodeResult(StatusCodes.Status422UnprocessableEntity);

            if (content.Data == null)
                return new StatusCodeResult(StatusCodes.Status304NotModified);

            var mime = content.MimeType ?? "application/octet-stream";
            var res = new FileContentResult(content.Data, mime);
            if (!string.IsNullOrEmpty(content.ContentHash))
                res.EntityTag = new EntityTagHeaderValue($"\"{content.ContentHash}\"");

            return res;
        }

        [System.Obsolete("return CreyActionResult instead or use IntoObjectResult if Ok or Error is not known/required")]
        public static JsonResult CreateResult<TOk, TError>(this Controller controller, Result<TOk, TError> result) where TError : Error
        {
            return result.Match(
                ok =>
                {
                    JsonResult jsonResult = controller.Json(ok);
                    jsonResult.StatusCode = StatusCodes.Status200OK;
                    return jsonResult;
                },
                error =>
                {
                    JsonResult jsonResult = controller.Json(error);
                    jsonResult.StatusCode = error.ErrorCode.IntoStatusCode();
                    return jsonResult;
                }
            );
        }

        public static string ExtractEndpointName(this IHttpContextAccessor context)
        {
            try
            {
                if (context == null || context.HttpContext == null)
                {
                    return "";
                }

                var ep = context.HttpContext.GetEndpoint();
                if (ep != null)
                    return ep.DisplayName;
                else
                    return "";
            }
            catch (Exception)
            {
                return "";
            }
        }


        [Obsolete]
        public static IEnumerable<IPAddress> GetRemoteIPAddresses(this HttpContext httpContext)
        {
            var result = new List<IPAddress>();
            if (httpContext.Connection.RemoteIpAddress != null)
                result.Add(httpContext.Connection.RemoteIpAddress);

            string failed = null;
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var ipList) && ipList.Any())
            {
                var ips = ipList.Select(x => x.Split(",")).SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x));
                foreach (var ip in ips)
                {
                    if (CreyIPAddress.TryParse(ip, out var parsed))
                        result.Add(parsed);
                    else if (failed == null)
                        failed = string.Join(",", ips);
                }
            }

            if (!result.Any() && failed != null)
                throw new InvalidArgumentException($"Failed to parse IP from one of:'{failed}'");
            return result;
        }
    }
}
