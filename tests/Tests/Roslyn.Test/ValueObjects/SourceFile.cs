namespace SpaceEngineers.Core.Roslyn.Test.ValueObjects
{
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Source file value object
    /// </summary>
    public class SourceFile
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Source file name (without extension)</param>
        /// <param name="text">Source file text</param>
        public SourceFile(string name, SourceText text)
        {
            Name = name;
            Text = text;
        }

        /// <summary>
        /// Source file name (without extension)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Source file text
        /// </summary>
        public SourceText Text { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}