using FileManager.Services.FileSystem.Other.Transient;
using Spectre.Console;

namespace FileManager.UI.ViewUI.Components;

public class HelpPanel
{
    public static void Output(HelpInfo helpInfo)
    {
        var table = CreateTable(helpInfo);

        AddDirectoryCommandsSection(table, helpInfo);
        AddFileCommandsSection(table, helpInfo);
        AddUtilityCommandsSection(table, helpInfo);

        AnsiConsole.Write(table);
    }

    private static Table CreateTable(HelpInfo helpInfo)
    {
        return new Table()
            .Border(TableBorder.Simple)
            .BorderColor(Color.Green)
            .Title($"[yellow]{helpInfo.Title}[/]")
            .AddColumn(
                new TableColumn($"[cyan]{helpInfo.TitleNameCommand}[/]")
                    .Width(15))
            .AddColumn(
                new TableColumn($"[cyan]{helpInfo.TitleUsageCommand}[/]")
                    .Width(30))
            .AddColumn(
                new TableColumn($"[cyan]{helpInfo.TitleDescriptionCommand}[/]")
                    .Width(45));
    }

    private static void AddDirectoryCommandsSection(Table table, HelpInfo helpInfo)
    {
        table.AddRow(
            $"[green]{helpInfo.TitleDirectoryChapter}[/]",
            "------------------------------",
            "---------------------------------------------"
        );

        foreach (var command in helpInfo.HelpDirectoryCommands)
        {
            table.AddRow(
                command.Name,
                command.Usage,
                command.Description
            );
        }

        table.AddEmptyRow();
    }

    private static void AddFileCommandsSection(Table table, HelpInfo helpInfo)
    {
        table.AddRow(
            $"[blue]{helpInfo.TitleFileChapter}[/]",
            "------------------------------",
            "---------------------------------------------"
        );

        foreach (var command in helpInfo.HelpFileCommands)
        {
            table.AddRow(
                command.Name,
                command.Usage,
                command.Description
            );
        }

        table.AddEmptyRow();
    }

    private static void AddUtilityCommandsSection(Table table, HelpInfo helpInfo)
    {
        table.AddRow(
            $"[orange1]{helpInfo.TitleUtilityChapter}[/]",
            "------------------------------",
            "---------------------------------------------"
        );

        foreach (var command in helpInfo.HelpUtilityCommands)
        {
            table.AddRow(
                command.Name,
                command.Usage,
                command.Description
            );
        }
    }
}