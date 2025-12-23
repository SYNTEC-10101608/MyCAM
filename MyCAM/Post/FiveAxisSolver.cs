using System;
using System.Runtime.InteropServices;

namespace PostTool.Interop
{
    /// <summary>
    /// Solution type for choosing between multiple five-axis solutions
    /// </summary>
    public enum SolutionType
    {
        /// <summary>Choose solution with shortest distance</summary>
        ShortestDist = 0,
        /// <summary>Choose solution with positive master rotation</summary>
        MasterPos = 1,
        /// <summary>Choose solution with negative master rotation</summary>
        MasterNeg = 2,
        /// <summary>Choose solution with shortest combined master and slave angle distance</summary>
        MSAngleShortestDist = 3
    }

    /// <summary>
    /// P/Invoke wrapper for five-axis solver functions
    /// </summary>
    public static class FiveAxisSolver
    {
        private const string DllName = "PostTool.dll";

        /// <summary>
        /// Calculate rotation angles from tool direction vectors
        /// </summary>
        /// <param name="toolDirection">Current tool direction vector (3 elements: X, Y, Z)</param>
        /// <param name="toolDirectionAtZero">Tool direction at zero position (3 elements: X, Y, Z)</param>
        /// <param name="directOfFirstRotAxis">Direction of first rotation axis (3 elements: X, Y, Z)</param>
        /// <param name="directOfSecondRotAxis">Direction of second rotation axis (3 elements: X, Y, Z)</param>
        /// <param name="lastMasterRotAngle">Last master rotation angle</param>
        /// <param name="lastSlaveRotAngle">Last slave rotation angle</param>
        /// <param name="mRotAngle1">Output: Master rotation angle solution 1</param>
        /// <param name="sRotAngle1">Output: Slave rotation angle solution 1</param>
        /// <param name="mRotAngle2">Output: Master rotation angle solution 2</param>
        /// <param name="sRotAngle2">Output: Slave rotation angle solution 2</param>
        /// <param name="iuToBluRotary">IU to BLU conversion factor for rotary axes</param>
        /// <returns>Error code (0 = no error)</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "FiveAxisSolver_IJKtoMS")]
        public static extern int IJKtoMS(
            [In] double[] toolDirection,
            [In] double[] toolDirectionAtZero,
            [In] double[] directOfFirstRotAxis,
            [In] double[] directOfSecondRotAxis,
            double lastMasterRotAngle,
            double lastSlaveRotAngle,
            out double mRotAngle1,
            out double sRotAngle1,
            out double mRotAngle2,
            out double sRotAngle2,
            double iuToBluRotary);

        /// <summary>
        /// Choose the best solution from two angle pairs
        /// </summary>
        /// <param name="mRotAngle1">Master rotation angle solution 1</param>
        /// <param name="sRotAngle1">Slave rotation angle solution 1</param>
        /// <param name="mRotAngle2">Master rotation angle solution 2</param>
        /// <param name="sRotAngle2">Slave rotation angle solution 2</param>
        /// <param name="lastMasterRotAngle">Last master rotation angle</param>
        /// <param name="lastSlaveRotAngle">Last slave rotation angle</param>
        /// <param name="masterRotAngle">Output: Selected master rotation angle</param>
        /// <param name="slaveRotAngle">Output: Selected slave rotation angle</param>
        /// <param name="type">Solution selection type</param>
        /// <param name="fStart">First axis start limit</param>
        /// <param name="fEnd">First axis end limit</param>
        /// <param name="sStart">Second axis start limit</param>
        /// <param name="sEnd">Second axis end limit</param>
        /// <param name="nRDOfFirst">Rotation direction of first axis (1 or -1)</param>
        /// <param name="nRDOfSecond">Rotation direction of second axis (1 or -1)</param>
        /// <param name="iuToBluRotary">IU to BLU conversion factor for rotary axes</param>
        /// <returns>Error code (0 = no error)</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "FiveAxisSolver_ChooseSolution")]
        public static extern int ChooseSolution(
            double mRotAngle1,
            double sRotAngle1,
            double mRotAngle2,
            double sRotAngle2,
            double lastMasterRotAngle,
            double lastSlaveRotAngle,
            out double masterRotAngle,
            out double slaveRotAngle,
            SolutionType type,
            double fStart,
            double fEnd,
            double sStart,
            double sEnd,
            int nRDOfFirst,
            int nRDOfSecond,
            double iuToBluRotary);
    }
}
