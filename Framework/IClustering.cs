/*
 * General interface for clustering algorithms
 * 
 * Ondrej Kaas
 */
using Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework
{
    public interface ISelector
    {
        /// <summary>
        /// General mechanism for setting algorithms 
        /// </summary>
        /// <param name="properties">setup for algorithm</param>
        void SetProperties(Dictionary<string, Object> properties);

        /// <summary>
        /// Main entry point to clustering
        /// </summary>
        /// <param name="points"></param>
        /// <returns>time for debug purpose</returns>
        int ComputeClustering(Candidate[] points, RegistrationConfig config);

        DQuat FindDualQuatCandidate(Candidate[] candidate, RegistrationConfig config, int indexBestCandidate, out int clusterIndex);

        DQuat[] FindDualQuatCandidates(Candidate[] candidate, RegistrationConfig config, int indexBestCandidate, out int[] clusterIndexies);

        /// <summary>
		/// Gets indices of all facilities (cluster centres).
		/// </summary>
		/// <returns>Returns an array of facility indices.</returns>

        /// <summary>
        /// Get all information about clustering properties, times, input files, etc.
        /// Give more information after calling method ComputeClustering
        /// </summary>
        /// <returns></returns>
        string GetInfo();

    }
}
