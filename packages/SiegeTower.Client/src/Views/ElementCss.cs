public static class ElementCss
{
	public static string Classes(Element element, params string[] roles)
	{
		var classes = new List<string>(roles.Where(role => !string.IsNullOrWhiteSpace(role)));
		if (element.Color != ElementColor.None)
		{
			classes.Add($"color-{element.Color.ToString().ToLowerInvariant()}");
		}
		AddState(classes, element.State, ElementState.Hoverable, "is-hoverable");
		AddState(classes, element.State, ElementState.Selected, "is-selected");
		AddState(classes, element.State, ElementState.Inactive, "is-inactive");
		AddState(classes, element.State, ElementState.Disabled, "is-disabled");
		AddState(classes, element.State, ElementState.Open, "is-open");
		AddState(classes, element.State, ElementState.Expanded, "is-expanded");
		AddState(classes, element.State, ElementState.Draggable, "is-draggable");
		AddState(classes, element.State, ElementState.Dragging, "is-dragging");
		AddState(classes, element.State, ElementState.DropTarget, "is-drop-target");
		AddState(classes, element.State, ElementState.Resizing, "is-resizing");
		if (element.Alignment != GridAlignment.Default)
		{
			classes.Add($"grid-align-{element.Alignment.ToString().ToLowerInvariant()}");
		}
		return string.Join(" ", classes);
	}

	static void AddState(List<string> classes, ElementState state, ElementState flag, string name)
	{
		if (state.HasFlag(flag))
		{
			classes.Add(name);
		}
	}
}