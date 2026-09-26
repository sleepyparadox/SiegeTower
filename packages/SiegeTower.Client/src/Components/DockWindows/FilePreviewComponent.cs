public sealed class FilePreviewComponent : Component, IRequires<DockWindow>
{
	public string FilePath { get; set; }
	public string Contents { get; set; }
	public string SavedContents { get; set; }
	public bool IsImage { get; set; }
	public bool IsEditable { get; set; }
	public string MimeType { get; set; }
	public bool IsDirty => !string.Equals(Contents, SavedContents, StringComparison.Ordinal);

	public FilePreviewComponent(Entity entity, string filePath, string contents, bool isImage, string mimeType) : base(entity)
	{
		FilePath = filePath;
		Contents = contents;
		SavedContents = contents;
		IsImage = isImage;
		MimeType = mimeType;
	}
}