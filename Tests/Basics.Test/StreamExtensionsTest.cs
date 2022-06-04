namespace SpaceEngineers.Core.Basics.Test
{
    using System.Globalization;
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
        internal void OverWriteAllTest()
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

        [Fact]
        internal void CompressionTest()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 42; i++)
            {
                sb.Append(nameof(CompressionTest));
            }

            var repeatedString = sb.ToString();
            var bytes = Encoding.UTF8.GetBytes(repeatedString);

            var compressedBytes = bytes.Compress();

            Output.WriteLine(bytes.Length.ToString(CultureInfo.InvariantCulture));
            Output.WriteLine(compressedBytes.Length.ToString(CultureInfo.InvariantCulture));
            Assert.True(bytes.Length > compressedBytes.Length);

            var decompressedBytes = compressedBytes.Decompress();

            Output.WriteLine(decompressedBytes.Length.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(bytes.Length, decompressedBytes.Length);

            var decompressedRepeatedString = Encoding.UTF8.GetString(decompressedBytes);

            Assert.Equal(repeatedString, decompressedRepeatedString);
        }
    }
}