namespace SpaceEngineers.Core.Basics.Test
{
    using System.IO;
    using System.Text;
    using System.Threading;
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

            var token = CancellationToken.None;
            var encoding = new UTF8Encoding();

            var longBytes = encoding.GetBytes(@long);
            var shortBytes = encoding.GetBytes(@short);

            using (var stream = new MemoryStream(longBytes))
            {
                var readed = stream.ReadAllAsync(encoding, token).Result;
                Assert.Equal(@long, readed);

                stream.OverWriteAllAsync(shortBytes, token).Wait(token);

                readed = stream.ReadAllAsync(encoding, token).Result;
                Assert.Equal(@short, readed);
            }
        }
    }
}