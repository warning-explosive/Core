namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// JoinExpression
    /// </summary>
    public class JoinExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="leftSource">Left source expression</param>
        /// <param name="rightSource">Right source expression</param>
        /// <param name="on">On expression</param>
        public JoinExpression(
            ISqlExpression leftSource,
            ISqlExpression rightSource,
            BinaryExpression on)
        {
            if (leftSource is not JoinExpression
                && leftSource is not NamedSourceExpression)
            {
                throw new ArgumentException($"{nameof(JoinExpression)} doesn't support {leftSource.GetType().Name} as {nameof(leftSource)} argument");
            }

            if (rightSource is not JoinExpression
                && rightSource is not NamedSourceExpression)
            {
                throw new ArgumentException($"{nameof(JoinExpression)} doesn't support {rightSource.GetType().Name} as {nameof(rightSource)} argument");
            }

            LeftSource = leftSource;
            RightSource = rightSource;
            On = on;
        }

        /// <summary>
        /// Left source expression
        /// </summary>
        public ISqlExpression LeftSource { get; }

        /// <summary>
        /// Right source expression
        /// </summary>
        public ISqlExpression RightSource { get; }

        /// <summary>
        /// On expression
        /// </summary>
        public ISqlExpression On { get; }
    }
}