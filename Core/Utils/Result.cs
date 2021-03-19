using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Crey.Utils
{
    /// <remarks>
    /// Handled xternal API (match, correctness, serialization, equality, dictionary hash, view in debug, to string).
    /// Internal plumbing is complicated.
    /// Half of the code can be deleted from this class with C# 9 and .NET 5 and drop of legacy Newtonsoft and new System.Text.Json (with improved performance)
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay}")]
    [Newtonsoft.Json.JsonConverter(typeof(ResultConverter))]
    public sealed class Result<TOk, TError> : IEquatable<TOk>, IEquatable<Result<TOk, TError>> // issue: does not employs C# 8 feature for nullability checking to model null safe result vs optional
    {
        // issue: does not employ possibility to use with C# pattern matching over type (and then over structure)
        private readonly TOk ok_;
        private readonly TError error_;

        public Result(TOk ok)
        {
            ok_ = ok;// ok can be null, so we get Result<TOk?, TError> === Result<Optional<TOk>, TError>>
            IsOk = true;
        }

        public Result(TError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));// no reason to create error which is null
            error_ = error;
            IsOk = false;
        }

        [DebuggerStepThrough]
        public static implicit operator Result<TOk, TError>(TOk ok) => new Result<TOk, TError>(ok);

        [DebuggerStepThrough]
        public static implicit operator Result<TOk, TError>(TError error) => new Result<TOk, TError>(error);

        public bool IsOk { get; }

        public bool IsError => !IsOk;

        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => IsOk ? $"Ok({Value})" : $"Error({Value})";

        /// <summary>
        /// Must not be called directly. Either via Unwraps/Map or via switch Value
        /// </summary>
        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [IgnoreDataMember]
        [JsonIgnore]
        public TOk Ok
        {
            [DebuggerStepThrough]
            get
            {
                ThrowOk(IsOk);
                return ok_;
            }
        }

        private void ThrowOk(bool shouldBeTrue)
        {
            if (!shouldBeTrue) throw new InvalidProgramException($"Must check result kind before accessing property. Asked for Ok, but that had Error= {Error}");
        }

        private void ThrowError(bool shouldBeTrue)
        {
            if (!shouldBeTrue) throw new InvalidProgramException($"Must check result kind before accessing property. Asked for Error, but that had Ok={Ok}");
        }

        /// <summary>
        /// Must not be called directly. Either via Unwraps/Map or via switch Value
        /// </summary>
        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [IgnoreDataMember]
        [JsonIgnore]
        public TError Error
        {
            [DebuggerStepThrough]
            get
            {
                ThrowError(IsError);
                return error_;
            }
        }

        /// <summary>
        /// Is either <see typeref="TOk"/> or <see typeref="TError"/>.
        ///</summary>       
        [IgnoreDataMember]
        [JsonIgnore]
        public object Value
        {
            [DebuggerStepThrough]
            get => IsOk ? (object)ok_ : error_;
        }

        public T Match<T>(Func<TOk, T> okFunc, Func<TError, T> errorFunc)
        {
            return IsOk ? okFunc(ok_) : errorFunc(error_);
        }

        public void MatchAction(Action<TOk> okAction, Action<TError> errorAction)
        {
            if (IsOk)
            {
                okAction(ok_);
            }
            else
            {
                errorAction(error_);
            }
        }

        public Result<T2, TError> AndThen<T2>(Func<TOk, Result<T2, TError>> mapFunc)
        {
            return IsOk ? mapFunc(ok_) : error_;
        }

        public Result<T, TError> Map<T>(Func<TOk, T> mapFunc)
        {
            return IsOk ? mapFunc(ok_) : (Result<T, TError>)error_;
        }

        public Result<TOk, T> MapError<T>(Func<TError, T> mapFunc)
        {
            return !IsOk ? mapFunc(error_) : (Result<TOk, T>)ok_;
        }

        [Obsolete("use extensions version")]
        public TOk UnwrapOr(Func<TError, TOk> mapFunc)
        {
            return !IsOk ? mapFunc(error_) : ok_;
        }

        public async Task<TOk> UnwrapOrAsync(Func<TError, Task<TOk>> mapFunc)
        {
            return !IsOk ? await mapFunc(error_) : ok_;
        }

        // TODO: move to extesions

        public Result<TOk, TError> OnOk(Action<TOk> action)
        {
            if (IsOk)
            {
                action(ok_);
            }

            return this;
        }

        public Result<TOk, TError> OnError(Action<TError> action)
        {
            if (!IsOk)
            {
                action(error_);
            }

            return this;
        }

        /// <summary>
        /// If error, return <paramref name="defaultValue"/>, else <see cref="Ok"/>
        /// </summary>
        public TOk OrElse(TOk defaultValue) => Match(ok => ok, err => defaultValue);

        /// <summary>
        /// If error, return value created by <paramref name="okFactory"/>, else <see cref="Ok"/>
        /// </summary>
        public TOk OrElse(Func<TOk> okFactory) => Match(ok => ok, err => okFactory());

        /// <summary>
        /// If error, return default value, else <see cref="Ok"/>
        /// </summary>
        public TOk OrDefault() => Match(ok => ok, err => default(TOk));

        public bool Equals(TOk other) => other != null && IsOk && EqualityComparer<TOk>.Default.Equals(Ok, other);

        public bool Equals(TError other) => other != null && !IsOk && EqualityComparer<TError>.Default.Equals(Error, other);

        public bool Equals(Result<TOk, TError> other) => other != null && (other.IsOk ? Equals(other.Ok) : Equals(other.Error));

        /// <summary>
        /// Gets hash code of underlying value.
        /// </summary>
        public override int GetHashCode() => IsOk ? Ok.GetHashCode() : Error.GetHashCode();

        public override bool Equals(object obj) => obj is Result<TOk, TError> other && Equals(other);

        public override string ToString() => IsOk ? Ok.ToString() : Error.ToString();
    }

    internal class ResultConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.Name.Contains("Result");
        public override object ReadJson(JsonReader reader, Type typeToConvert, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var okType = typeToConvert.GetGenericArguments()[0];
            var errorType = typeToConvert.GetGenericArguments()[1];
            var isOk = false;
            var isError = false;
            object val = null;
            while (reader.Read())
            {
                if (reader.Value.ToString() == "IsOk")
                {
                    reader.Read();
                    isOk = bool.Parse(reader.Value.ToString());
                }
                if (reader.Value.ToString() == "IsError")
                {
                    reader.Read();
                    isError = bool.Parse(reader.Value.ToString());
                }

                if (reader.Value.ToString() == "Ok")
                {
                    reader.Read();
                    if (isOk)
                        val = serializer.Deserialize(reader, okType);
                    reader.Read();
                    if (reader.TokenType == JsonToken.EndObject)
                        break;
                }

                if (reader.Value.ToString() == "Error")
                {
                    reader.Read();
                    if (!isOk)
                        val = serializer.Deserialize(reader, errorType);
                    reader.Read();
                    if (reader.TokenType == JsonToken.EndObject)
                        break;
                }
            }

            return Activator.CreateInstance(typeToConvert, val);
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteStartObject();
            var isOk = (bool)value.GetType().GetProperty("IsOk", BindingFlags.Public | BindingFlags.Instance).GetValue(value);
            var val = value.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetValue(value);
            writer.WritePropertyName("IsOk");
            writer.WriteValue(isOk);
            writer.WritePropertyName("IsError");
            writer.WriteValue(!isOk);
            if (isOk)
            {
                writer.WritePropertyName("Ok");
            }
            else
            {
                writer.WritePropertyName("Error");
            }

            serializer.Serialize(writer, val);

            writer.WriteEndObject();
        }
    }

    /// static methods allow to use extensions in fluent chain
    public static class ResultExtensions
    {
        /// <summary>
        /// Returns the contained Ok value or computes it from a closure.
        /// </summary>
        public static TOk UnwrapOrElse<TOk, TError>(this Result<TOk, TError> self, Func<TError, TOk> op)
        {
            return self.IsOk ? self.Ok : op(self.Error);
        }

        /// <summary>
        /// Returns the contained Ok value.
        /// </summary>
        /// <exception cref="InvalidProgramException">Generally you should check for IsOk.</exception>
        public static TOk Unwrap<TOk, TError>(this Result<TOk, TError> self)
        {
            return self.Ok;
        }

        public async static Task<TOk> OkAsync<TOk, TError>(this Task<Result<TOk, TError>> self)
        {
            return (await self).Ok;
        }

        public static IEnumerable<Result<TResult, Exception>> SelectResult<TIn, TResult>(this IEnumerable<TIn> self, Func<TIn, TResult> selector)
        {
            foreach (var item in self)
            {
                Result<TResult, Exception> result;
                try
                {
                    result = selector(item);
                }
                catch (Exception ex)
                {
                    result = ExceptionDispatchInfo.Capture(ex).SourceException;
                }

                yield return result;
            }
        }

        public static IEnumerable<TResult> WhereOk<TResult, Exception>(this IEnumerable<Result<TResult, Exception>> self)
        {
            return self.Where(x => x.IsOk).Select(x => x.Ok);
        }

        /// <exception cref="AggregateException">All error results.</exception>
        public static void SelectThrow<TResult>(this IEnumerable<Result<TResult, Exception>> self)
        {
            var errors = self.Where(x => !x.IsOk).Cast<Result<TResult, Exception>>().Select(x => x.Error);
            if (errors.Any())
                throw new AggregateException(errors);
        }
    }
}
