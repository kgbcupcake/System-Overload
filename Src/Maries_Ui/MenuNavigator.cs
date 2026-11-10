using static System.Console;

namespace Maries_Ui
{
	public static class MenuNavigator
	{
		public static int HandleVerticalNav(ConsoleKey key, int currentIndex, int itemCount)
		{
			if (key == ConsoleKey.UpArrow) return (currentIndex == 0) ? itemCount - 1 : currentIndex - 1;
			if (key == ConsoleKey.DownArrow) return (currentIndex == itemCount - 1) ? 0 : currentIndex + 1;
			return currentIndex;
		}

		public static (int Index, int ScrollOffset) HandleVerticalNavScroll(ConsoleKey key, int currentIndex, int itemCount, int scrollOffset, int maxVisibleItems)
		{
			int newIndex = HandleVerticalNav(key, currentIndex, itemCount);
			int newScrollOffset = scrollOffset;

			if (key == ConsoleKey.UpArrow)
			{
				if (newIndex < newScrollOffset) newScrollOffset = newIndex;
				if (newIndex == itemCount - 1) newScrollOffset = Math.Max(0, itemCount - maxVisibleItems);
			}
			else if (key == ConsoleKey.DownArrow)
			{
				if (newIndex >= newScrollOffset + maxVisibleItems) newScrollOffset = newIndex - maxVisibleItems + 1;
				if (newIndex == 0) newScrollOffset = 0;
			}

			return (newIndex, newScrollOffset);
		}
	}
}
