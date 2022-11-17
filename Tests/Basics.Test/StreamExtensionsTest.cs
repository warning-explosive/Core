namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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
        internal void OverwriteTest()
        {
            const string str1 = "Hello world!";
            const string str2 = "Hello!";

            Assert.NotEqual(str1, str2);

            var encoding = Encoding.UTF8;

            ReadOnlySpan<byte> bytes = encoding.GetBytes(str1);

            using (var stream = bytes.AsMemoryStream())
            {
                var read = stream.AsString(encoding);
                Assert.Equal(str1, read);

                stream.Overwrite(encoding.GetBytes(str2));

                read = stream.AsString(encoding);
                Assert.Equal(str2, read);
            }
        }

        [Fact]
        internal async Task OverwriteAsyncTest()
        {
            const string str1 = "Hello world!";
            const string str2 = "Hello!";

            Assert.NotEqual(str1, str2);

            var encoding = Encoding.UTF8;

            ReadOnlyMemory<byte> bytes = encoding.GetBytes(str1);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
            using (var stream = bytes.Span.AsMemoryStream())
            {
                var read = await stream.AsString(encoding, cts.Token).ConfigureAwait(false);
                Assert.Equal(str1, read);

                await stream.Overwrite(encoding.GetBytes(str2), cts.Token).ConfigureAwait(false);

                read = await stream.AsString(encoding, cts.Token).ConfigureAwait(false);
                Assert.Equal(str2, read);
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
            ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(repeatedString);

            ReadOnlySpan<byte> compressedBytes = bytes.Compress().Span;

            Output.WriteLine(bytes.Length.ToString(CultureInfo.InvariantCulture));
            Output.WriteLine(compressedBytes.Length.ToString(CultureInfo.InvariantCulture));
            Assert.True(bytes.Length > compressedBytes.Length);

            ReadOnlySpan<byte> decompressedBytes = compressedBytes.Decompress().Span;

            Output.WriteLine(decompressedBytes.Length.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(bytes.Length, decompressedBytes.Length);

            var decompressedRepeatedString = Encoding.UTF8.GetString(decompressedBytes);

            Assert.Equal(repeatedString, decompressedRepeatedString);
        }
    }
}