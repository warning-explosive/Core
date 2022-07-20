namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    /// <summary>
    /// BinaryOperator
    /// </summary>
    public enum BinaryOperator
    {
        /// <summary>
        /// Equal (==)
        /// </summary>
        Equal,

        /// <summary>
        /// NotEqual (!=)
        /// </summary>
        NotEqual,

        /// <summary>
        /// GreaterThanOrEqual
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// GreaterThan
        /// </summary>
        GreaterThan,

        /// <summary>
        /// LessThan
        /// </summary>
        LessThan,

        /// <summary>
        /// LessThanOrEqual
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// AndAlso (and)
        /// </summary>
        AndAlso,

        /// <summary>
        /// OrElse (or)
        /// </summary>
        OrElse,

        /// <summary>
        /// ExclusiveOr (^)
        /// </summary>
        ExclusiveOr,

        /// <summary>
        /// Coalesce (??)
        /// </summary>
        Coalesce,

        /// <summary>
        /// Contains (sql in)
        /// </summary>
        Contains,

        /// <summary>
        /// Like (sql like)
        /// </summary>
        Like,

        /// <summary>
        /// Add (a + b)
        /// </summary>
        Add,

        /// <summary>
        /// Subtract (a - b)
        /// </summary>
        Subtract,

        /// <summary>
        /// Divide (a / b)
        /// </summary>
        Divide,

        /// <summary>
        /// Multiply (a * b)
        /// </summary>
        Multiply,

        /// <summary>
        /// Modulo (a % b)
        /// </summary>
        Modulo
    }
}