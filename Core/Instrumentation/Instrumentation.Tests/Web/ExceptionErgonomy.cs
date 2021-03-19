using Xunit;

namespace Crey.Instrumentation.Web
{
    public class HttpStatusExceptionTest
    {

        [Fact]
        public void TestErgonomy()
        {
            try
            {
                try
                {
                    throw new ItemNotFoundException("ex").WithDetail(new { Hello = "Hello" });
                }
                catch (HttpStatusErrorException ex)
                {
                    Assert.True(ex.BodyJson == @"{""Message"":""ex"",""Detail"":{""Hello"":""Hello""}}");
                    throw ex.WithDetail(new { Hello = "World" });
                }
            }
            catch (ItemNotFoundException ex)
            {
                Assert.True(ex.BodyJson == @"{""Message"":""ex"",""Detail"":{""Hello"":""World""}}");
            }
        }

    }
}
