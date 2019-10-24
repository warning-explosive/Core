namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public static class StreamExtensions
    {
        public static async Task<string> ReadAllAsync(this Stream stream, Encoding encoding)
        {
            return encoding.GetString(await stream.ReadAllAsync());
        }
        
        public static async Task OverWriteAllAsync(this Stream stream, string serialized, Encoding encoding)
        {
            await stream.OverWriteAllAsync(encoding.GetBytes(serialized));
        }

        public static async Task<byte[]> ReadAllAsync(this Stream stream)
        {
            var bytes = (int)stream.Length;
            var buffer = new byte[bytes];
            
            var offset = 0;
            stream.Position = offset;
            
            await stream.ReadAsync(buffer, offset, bytes);

            return buffer;
        }

        public static async Task OverWriteAllAsync(this Stream stream, byte[] bytes)
        {
            await stream.FlushAsync();
            
            var offset = 0;
            stream.Position = offset;
            
            await stream.WriteAsync(bytes, offset, bytes.Length);
        }
    }
}