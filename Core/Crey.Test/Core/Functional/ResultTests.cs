using System;
using Xunit;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;
using Crey.Contracts;

namespace Core.Functional
{
    public class ResultTests
    {
        [Fact]
        public void SerializeOk()
        {
            Result<int, HttpStatusCode> r = 42;
            var x = JObject.FromObject(r);
            Assert.Equal(true, x["IsOk"]);
            Assert.Equal(42, x["Ok"]);
            Assert.False(x.ContainsKey("Value"));
            Assert.False(x.ContainsKey("Error"));
        }

        public class X
        {
            public string Y { get; set; }
            public int[] Z { get; set; }

            public override bool Equals(object obj) => obj is X x && x.Y == Y && x.Z.FirstOrDefault() == Z.FirstOrDefault();

            public override int GetHashCode() => HashCode.Combine(Y, Z);
        }

        [Fact]
        public void DeSerializeOk()
        {
            Result<X, HttpStatusCode> r = new X { Y = "42", Z = new[] { 1, 2, 3 } };
            var s = JsonConvert.SerializeObject(r);
            var x = JsonConvert.DeserializeObject<Result<X, HttpStatusCode>>(s);
            Assert.Equal(r, x);
        }

        [Fact]
        public void DeSerializeOkPartial()
        {
            var partial = "{\"IsOk\":true, \"Ok\":42}";
            var x = JsonConvert.DeserializeObject<Result<int, Error>>(partial);
            Assert.Equal(42, x);
            Assert.True(x.IsOk);
        }

        [Fact]
        public void DeSerializeErrorPartial()
        {
            var partial = "{\"IsOk\":false, \"Error\":42}";
            var x = JsonConvert.DeserializeObject<Result<string, int>>(partial);
            Assert.Equal(42, x);
            Assert.True(x.IsError);
        }

        [Fact]
        public void DeSerializeErrorPartial2()
        {
            var partial = "{\"IsOk\":false, \"Error\":42}";
            var x = JsonConvert.DeserializeObject<Result<string, int>>(partial);
            Assert.Equal(42, x);
            Assert.True(x.IsError);
        }

        [Fact]
        public void DeSerializeErrorTextPartial2()
        {
            var partial = "{\"IsOk\":false, \"Error\":42}";
            var x = System.Text.Json.JsonSerializer.Deserialize<Result<string, int>>(partial);
            Assert.Equal(42, x);
            Assert.True(x.IsError);
        }

        [Fact]
        public void DeSerializeOkResultNull()
        {
            var partial = "{\"IsOk\":true, \"Ok\":42, \"Error\": null}";
            var x = JsonConvert.DeserializeObject<Result<int, string>>(partial);
            Assert.Equal(42, x);
            Assert.True(x.IsOk);
        }

        [Fact]
        public void DeSerializeOkTextResultNull()
        {
            var partial = "{\"IsOk\":true, \"Ok\":42, \"Error\": null}";
            var x = System.Text.Json.JsonSerializer.Deserialize<Result<int, string>>(partial);
            Assert.Equal(42, x);
            Assert.True(x.IsOk);
        }


        [Fact]
        public void DeSerializeErrorComplexObject()
        {
            Result<X, Error> r = new Error { ErrorCode = ErrorCodes.CommandError, Message = "Hey" };
            var s = System.Text.Json.JsonSerializer.Serialize(r);
            var x = System.Text.Json.JsonSerializer.Deserialize<Result<X, Error>>(s);
            switch (x.Value)
            {
                case Error err:
                    Assert.Equal(ErrorCodes.CommandError, err.ErrorCode);
                    Assert.Equal("Hey", err.Message);
                    break;
                case X v:
                    Assert.False(true);
                    break;
            }
        }

        public class Job
        {
            public Exception Clr { get; set; }
            public Result<X, Error> Result { get; set; }
            public string Name { get; set; }
        }


        [Fact]
        public void DeSerializeErrorTextComplexObjectInside()
        {
            Result<X, Error> r = new Error { ErrorCode = ErrorCodes.CommandError, Message = "Hey" };
            var s = System.Text.Json.JsonSerializer.Serialize(new Job { Result = r, Name = "Foo" });
            var x = System.Text.Json.JsonSerializer.Deserialize<Job>(s);
            switch (x.Result.Value)
            {
                case Error err:
                    Assert.Equal(ErrorCodes.CommandError, err.ErrorCode);
                    Assert.Equal("Hey", err.Message);
                    break;
                case X v:
                    Assert.False(true);
                    break;
            }
            Assert.Equal("Foo", x.Name);
            Assert.Null(x.Clr);
        }

        [Fact]
        public void DeSerializeErrorComplexObjectInside()
        {
            Result<X, Error> r = new Error { ErrorCode = ErrorCodes.CommandError, Message = "Hey" };
            var s = JsonConvert.SerializeObject(new Job { Result = r, Name = "Foo" });
            var x = JsonConvert.DeserializeObject<Job>(s);
            switch (x.Result.Value)
            {
                case Error err:
                    Assert.Equal(ErrorCodes.CommandError, err.ErrorCode);
                    Assert.Equal("Hey", err.Message);
                    break;
                case X v:
                    Assert.False(true);
                    break;
            }
            Assert.Equal("Foo", x.Name);
            Assert.Null(x.Clr);
        }


        [Fact]
        public void DeSerializeOkText()
        {
            Result<X, HttpStatusCode> r = new X { Y = "42", Z = new[] { 1, 2, 3 } };
            var s = System.Text.Json.JsonSerializer.Serialize(r);
            var x = System.Text.Json.JsonSerializer.Deserialize<Result<X, HttpStatusCode>>(s);
            Assert.Equal(r, x);
        }

        [Fact]
        public void DeSerializeOkBool()
        {
            Result<bool, NoData> r = true;
            var s = JsonConvert.SerializeObject(r);
            var x = JsonConvert.DeserializeObject<Result<bool, NoData>>(s);
            Assert.Equal((object)true, x.Ok);
        }


        [Fact]
        public void DeSerializeErrorNoData()
        {
            Result<bool, NoData> r = new NoData();
            var s = JsonConvert.SerializeObject(r);
            var x = JsonConvert.DeserializeObject<Result<bool, NoData>>(s);
            Assert.NotNull(x.Error);
        }

        [Fact]
        public void DeSerializeErrorTextNoData()
        {
            Result<bool, NoData> r = new NoData();
            var s = System.Text.Json.JsonSerializer.Serialize(r);
            var x = System.Text.Json.JsonSerializer.Deserialize<Result<bool, NoData>>(s);
            Assert.NotNull(x.Error);
        }

        [Fact]
        public void DeSerializeOkBoolText()
        {
            Result<bool, NoData> r = true;
            var s = System.Text.Json.JsonSerializer.Serialize(r);
            var x = System.Text.Json.JsonSerializer.Deserialize<Result<bool, NoData>>(s);
            Assert.Equal((object)true, x.Ok);
        }

        [Fact]
        public void DeSerializeError()
        {
            Result<X, HttpStatusCode> r = HttpStatusCode.Ambiguous;
            var s = JsonConvert.SerializeObject(r);
            var x = JsonConvert.DeserializeObject<Result<X, HttpStatusCode>>(s);
            Assert.Equal(r, x);
        }

        [Fact]
        public void DeSerializeErrorText()
        {
            Result<X, HttpStatusCode> r = HttpStatusCode.Ambiguous;
            var s = System.Text.Json.JsonSerializer.Serialize(r);
            var x = System.Text.Json.JsonSerializer.Deserialize<Result<X, HttpStatusCode>>(s);
            Assert.Equal(r, x);
        }

        [Fact]
        public void SerializeError()
        {
            Result<int, HttpStatusCode> r = HttpStatusCode.Ambiguous;
            var x = JObject.FromObject(r);
            Assert.Equal(false, x["IsOk"]);
            Assert.Equal(HttpStatusCode.Ambiguous, Enum.Parse<HttpStatusCode>((x["Error"] as JValue).Value.ToString()));
            Assert.False(x.ContainsKey("Value"));
            Assert.False(x.ContainsKey("Ok"));
        }

        [Fact]
        public void SerializeErrorText()
        {
            Result<int, HttpStatusCode> r = HttpStatusCode.Ambiguous;
            var c = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(r));
            var x = c.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);
            Assert.Equal(false.ToString(), x["IsOk"].ToString());
            Assert.Equal(HttpStatusCode.Ambiguous.ToString(), Enum.Parse<HttpStatusCode>(x["Error"].ToString()).ToString());
            Assert.False(x.ContainsKey("Value"));
            Assert.False(x.ContainsKey("Ok"));
        }

        [Fact]
        public void SerializeOkText()
        {
            Result<int, HttpStatusCode> r = 42;
            var c = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(r));
            var x = c.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);
            Assert.Equal(true.ToString(), x["IsOk"].ToString());
            Assert.Equal(42.ToString(), x["Ok"].ToString());
            Assert.False(x.ContainsKey("Value"));
            Assert.False(x.ContainsKey("Error"));
        }
    }
}
