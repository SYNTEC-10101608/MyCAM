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
		void Show();

		/// <summary>
		/// Remove all rendered objects
		/// </summary>
		void Remove();

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
