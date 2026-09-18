public sealed class FilePreviewComponent : Component, IRequires<DockWindow>
{
	public string FilePath { get; set; }
	public string Contents { get; set; }
	public bool IsImage { get; set; }
	public string MimeType { get; set; }

	public FilePreviewComponent(Entity entity, string filePath, string contents, bool isImage, string mimeType) : base(entity)
	{
		FilePath = filePath;
		Contents = contents;
		IsImage = isImage;
		MimeType = mimeType;
	}
}