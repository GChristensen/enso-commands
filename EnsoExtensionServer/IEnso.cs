using System;
using CookComputing.XmlRpc;

namespace EnsoExtension
{
    public interface IEnso : IXmlRpcProxy
    {
        [XmlRpcMethod("displayMessage")]
        bool DisplayMessage(string text);

        [XmlRpcMethod("getFileSelection")]
        string[] GetFileSelection();

        [XmlRpcMethod("getUnicodeSelection")]
        string GetUnicodeSelection();

        [XmlRpcMethod("insertUnicodeAtCursor")]
        bool InsertUnicodeAtCursor(string text, string fromCommand);

        [XmlRpcMethod("registerCommand")]
        bool RegisterCommand(string url, string name, string description, string help, string postfixType);

        [XmlRpcMethod("setCommandValidPostfixes")]
        bool SetCommandValidPostfixes(string url, string name, string[] postfixes);

        [XmlRpcMethod("setUnicodeSelection")]
        bool SetUnicodeSelection(string text, string fromCommand);

        [XmlRpcMethod("unregisterCommand")]
        bool UnregisterCommand(string url, string name);
    }
}
