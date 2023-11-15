namespace SpaceEngineers.Core.GenericHost
{
    /// <summary>
    /// IHostedServiceRegistry
    /// </summary>
    public interface IHostedServiceRegistry
    {
        /// <summary>
        /// Enrolls specified hosted service object
        /// </summary>
        /// <param name="obj">Hosted service object</param>
        void Enroll(object obj);

        /// <summary>
        /// Returns true if hosted service has previously enrolled object
        /// </summary>
        /// <param name="obj">Hosted service object</param>
        /// <returns>True if hosted service has previously enrolled object</returns>
        bool Contains(object obj);
    }
}