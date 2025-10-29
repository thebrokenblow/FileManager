using Terminal.Gui;

namespace FileManager.UI.ViewUI.Components;

public class InputPanel
{
    public static void Input(Action<string?> input, string contents)
    {
        InputText(input, contents);
    }

    public static void Input(Action<string?> input)
    {
        InputText(input, string.Empty);
    }

    private static void InputText(Action<string?> input, string contents)
    {
        Application.Init();

        var window = new Window("Редактор текста")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var textView = new TextView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 3,
            Multiline = true,
            Text = contents
        };

        var buttonPanel = new View()
        {
            X = 0,
            Y = Pos.Bottom(textView),
            Width = Dim.Fill(),
            Height = 3
        };

        var buttonSave = new Button("Сохранить")
        {
            X = Pos.Center() - 12,
            Y = 1,
            Width = 10,
        };

        var buttonExit = new Button("Выйти")
        {
            X = Pos.Center() + 2,
            Y = 1,
            Width = 10,
        };

        buttonSave.Clicked += () =>
        {
            var text = textView.Text.ToString();
            input.Invoke(text);
            Application.RequestStop();
        };

        buttonExit.Clicked += () =>
        {
            Application.RequestStop();
        };

        buttonPanel.Add(buttonSave, buttonExit);
        window.Add(textView, buttonPanel);

        Application.Run(window);
        Application.Shutdown();
    }
}
