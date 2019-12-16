namespace SpaceEngineers.Core.Basics.Test
{
    using System.IO;
    using System.Text;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// StreamExtensions class test
    /// </summary>
    public class StreamExtensionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public StreamExtensionsTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void OwerWriteAllTest()
        {
            const string @long = "Hello world!";
            const string @short = "Hello!";

            var encoding = new UTF8Encoding();

            var longBytes = encoding.GetBytes(@long);
            var shortBytes = encoding.GetBytes(@short);

            using (var stream = new MemoryStream(longBytes))
            {
                var readed = stream.ReadAllAsync(encoding).Result;
                Assert.Equal(@long, readed);

                stream.OverWriteAllAsync(shortBytes).Wait();

                readed = stream.ReadAllAsync(encoding).Result;
                Assert.Equal(@short, readed);
            }
        }
    }
}