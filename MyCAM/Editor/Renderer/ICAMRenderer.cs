using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Base interface for all CAM renderers
	/// </summary>
	internal interface ICAMRenderer
	{
		/// <summary>
		/// Show the rendered objects
		/// </summary>
		/// <param name="bUpdate">Whether to update view after showing, default is false</param>
		void Show( bool bUpdate = false );

		/// <summary>
		/// Remove all rendered objects
		/// </summary>
		/// <param name="bUpdate">Whether to update view after removing, default is false</param>
		void Remove( bool bUpdate = false );

		/// <summary>
		/// Set whether to show the rendered objects
		/// </summary>
		/// <param name="isShow">True to show, false to hide</param>
		void SetShow( bool isShow );

		/// <summary>
		/// Update the view after rendering
		/// </summary>
		void UpdateView();
	}
}
