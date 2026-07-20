namespace Translate;

public static class Tui
{
    public static T? Choice<T>(Option<T>[] options, int selected = 0, string title = "") 
        where T : struct, Enum
    {
        Console.CursorVisible = false;

        while (true)
        {
            DrawChoiceMenu(options, selected, title);

            var key = Console.ReadKey(true).Key;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.UpArrow:
                    selected = (selected - 1 + options.Length) % options.Length;
                    break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.DownArrow:
                    selected = (selected + 1) % options.Length;
                    break;

                case ConsoleKey.Enter:
                    Console.CursorVisible = true;
                    Console.Clear();
                    return options[selected].Id;
                case ConsoleKey.Escape:
                    return null;
            }
        }
    }

    private static void DrawChoiceMenu<T>(
        Option<T>[] options,
        int selected,
        string title
    ) where T : struct, Enum
    {
        Console.Clear();

        if (!string.IsNullOrWhiteSpace(title))
            Console.WriteLine($"--- {title} ---\n");

        for (var i = 0; i < options.Length; i++)
            DrawSelectableItem(options[i].Title, i == selected);
    }

    private static void DrawSelectableItem(string text, bool selected)
    {
        if (selected)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"> {text} <");

            Console.ResetColor();
        }
        else
            Console.WriteLine($"  {text}");
    }
    
    public class Option<T>(string title, T id)
        where T : struct, Enum
    {
        public string Title { get; } = title;
        public T Id { get; } = id;
    }
}