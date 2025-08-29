using System;
using System.Runtime.InteropServices;

namespace MyCAM.Post
{
	internal static class NativeFCL
	{
		private const string DllName = "FCLTool.dll";

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		public static extern IntPtr CollisionSolver_Create();

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		public static extern void CollisionSolver_Destroy( IntPtr obj );

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		public static extern void CollisionSolver_AddModel(
			IntPtr obj,
			string szID,
			int triCount, int[] indexList,
			int vertexCount, double[] vertexList );

		[DllImport( DllName, CallingConvention = CallingConvention.StdCall )]
		[return: MarshalAs( UnmanagedType.I1 )] // C++ bool -> C# bool
		public static extern bool CollisionSolver_CheckCollision(
			IntPtr obj,
			string szID1,
			string szID2,
			double[] trsf1,
			double[] trsf2 );
	}

	public class CollisionSolver : IDisposable
	{
		private IntPtr nativeHandle;

		public CollisionSolver()
		{
			nativeHandle = NativeFCL.CollisionSolver_Create();
			if( nativeHandle == IntPtr.Zero ) {
				throw new Exception( "Failed to create native FCLTest instance." );
			}
		}

		public void AddModel( string id, int[] indices, double[] vertices )
		{
			int triCount = indices.Length / 3;
			int vertexCount = vertices.Length / 3;
			NativeFCL.CollisionSolver_AddModel( nativeHandle, id, triCount, indices, vertexCount, vertices );
		}

		public bool CheckCollision( string id1, string id2, double[] trsf1, double[] trsf2 )
		{
			return NativeFCL.CollisionSolver_CheckCollision( nativeHandle, id1, id2, trsf1, trsf2 );
		}

		public void Dispose()
		{
			if( nativeHandle != IntPtr.Zero ) {
				NativeFCL.CollisionSolver_Destroy( nativeHandle );
				nativeHandle = IntPtr.Zero;
			}
			GC.SuppressFinalize( this );
		}

		~CollisionSolver()
		{
			Dispose();
		}
	}
}
