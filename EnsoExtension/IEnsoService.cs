using System;

namespace EnsoExtension
{
    public interface IEnsoService
    {
        void DisplayMessage(EnsoMessage message);

        string[] GetFileSelection();

        string GetUnicodeSelection();

        void InsertUnicodeAtCursor(string text, EnsoCommand fromCommand);

        void RegisterCommand(IEnsoExtension extension, string uri, EnsoCommand command);

        void SetCommandValidPostfixes(EnsoCommand command, string[] postfixes);

        void SetUnicodeSelection(string text, EnsoCommand fromCommand);

        void UnregisterCommand(EnsoCommand command);
    }
}
