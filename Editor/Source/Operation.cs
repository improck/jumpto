﻿

namespace ImpRock.JumpTo.Editor
{
	[System.Flags]
	internal enum Operation
	{
		Idle = 0,
		LoadingProjectLinks = 1,
		LoadingHierarchyLinks = 2,
		CreatingLinkViaDragAndDrop = 4
	}
}