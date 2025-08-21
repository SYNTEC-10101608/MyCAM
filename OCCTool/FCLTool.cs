using System;
using System.Runtime.InteropServices;

namespace OCCTool
{
	internal static class NativeFCL
	{
		private const string DllName = "FCLTool.dll";

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		public static extern IntPtr FCLTest_Create();

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		public static extern void FCLTest_Destroy( IntPtr obj );

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		public static extern void FCLTest_AddModel(
			IntPtr obj,
			string szID,
			int triCount, int[] indexList,
			int vertexCount, double[] vertexList );

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		[return: MarshalAs( UnmanagedType.I1 )] // C++ bool -> C# bool
		public static extern bool FCLTest_CheckCollision(
			IntPtr obj,
			string szID1,
			string szID2,
			double[] trsf1,
			double[] trsf2 );
	}

	public class FCLTest : IDisposable
	{
		private IntPtr nativeHandle;

		public FCLTest()
		{
			nativeHandle = NativeFCL.FCLTest_Create();
			if( nativeHandle == IntPtr.Zero ) {
				throw new Exception( "Failed to create native FCLTest instance." );
			}
		}

		public void AddModel( string id, int[] indices, double[] vertices )
		{
			int triCount = indices.Length / 3;
			int vertexCount = vertices.Length / 3;
			NativeFCL.FCLTest_AddModel( nativeHandle, id, triCount, indices, vertexCount, vertices );
		}

		public bool CheckCollision( string id1, string id2, double[] trsf1, double[] trsf2 )
		{
			return NativeFCL.FCLTest_CheckCollision( nativeHandle, id1, id2, trsf1, trsf2 );
		}

		public void Dispose()
		{
			if( nativeHandle != IntPtr.Zero ) {
				NativeFCL.FCLTest_Destroy( nativeHandle );
				nativeHandle = IntPtr.Zero;
			}
			GC.SuppressFinalize( this );
		}

		~FCLTest()
		{
			Dispose();
		}
	}
}
