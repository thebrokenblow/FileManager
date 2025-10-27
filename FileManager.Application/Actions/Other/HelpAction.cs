using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Utils;
using Spectre.Console;

namespace FileManager.Application.Actions.Other;

/// <summary>
/// Команда для отображения справочной информации по доступным командам
/// </summary>
public class HelpAction : IAction
{
    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        if (!command.Trim().Equals(CommandDictionary.Help, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        DisplayHelp();
    }

    private static void DisplayHelp()
    {
        var table = new Table()
            .Border(TableBorder.Simple)
            .BorderColor(Color.Green)
            .Title("[yellow]СПРАВОЧНИК КОМАНД[/]")
            .AddColumn(new TableColumn("[cyan]Команда[/]").Width(15))
            .AddColumn(new TableColumn("[cyan]Использование[/]").Width(30))
            .AddColumn(new TableColumn("[cyan]Описание[/]").Width(45));

        table.AddRow(
            "[green]ДИРЕКТОРИИ[/]",
            "------------------------------",
            "---------------------------------------------"
        );

        table.AddRow("cd", "cd <директория>", "Переход в указанную директорию");
        table.AddRow("cd..", "cd..", "Переход в родительскую директорию");
        table.AddRow("mkdir", "mkdir <имя>", "Создание новой директории");
        table.AddRow("rmdir", "rmdir <имя>", "Удаление директории");
        table.AddRow("mvd", "mvd <источник> <цель>", "Перемещение директории");
        table.AddRow("ls", "ls", "Показать содержимое текущей директории");

        table.AddEmptyRow();

        table.AddRow(
            "[blue]ФАЙЛЫ[/]",
            "------------------------------",
            "---------------------------------------------"
        );

        table.AddRow("cp", "cp <файл> <директория>", "Копирование файла в директорию");
        table.AddRow("cp", "cp <файл> -l", "Копирование файла на уровень ниже");
        table.AddRow("touch", "touch <имя>", "Создание нового файла");
        table.AddRow("rm", "rm <имя>", "Удаление файла");
        table.AddRow("cat", "cat <имя>", "Чтение и вывод содержимого файла");
        table.AddRow("echo", "echo <имя>", "Запись текста в файл (интерактивно)");
        table.AddRow("mvf", "mvf <файл> <директория>", "Перемещение файла в директорию");
        table.AddRow("mvf", "mvf <файл> -l", "Перемещение файла на уровень ниже");

        table.AddEmptyRow();

        table.AddRow(
            "[orange1]СЕРВИС[/]",
            "------------------------------",
            "---------------------------------------------"
        );

        table.AddRow("parsexml", "parsexml <файл>", "Десериализация XML файла");
        table.AddRow("parsejson", "parsejson <файл>", "Десериализация JSON файла");
        table.AddRow("zip+", "zip+ <архив> <файлы...>", "Создание ZIP архива");
        table.AddRow("zip-", "zip- <архив>", "Распаковка ZIP архива");
        table.AddRow("df", "df", "Информация о дисковых накопителях");
        table.AddRow("help", "help", "Показать этот справочник");

        AnsiConsole.Write(table);
    }
}