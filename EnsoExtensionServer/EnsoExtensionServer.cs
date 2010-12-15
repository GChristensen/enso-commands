using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using CookComputing.XmlRpc;
using System.Configuration;

namespace EnsoExtension
{
    class EnsoExtensionServer : IEnsoService, IDisposable
    {
        private readonly string[] postfixTypes = { "none", "bounded", "arbitrary" };
        private bool disposed;
        IChannelReceiver channel;
        private IEnso ensoProxy;
        private IDictionary<string, EnsoExtensionProxy> extensions = new Dictionary<string, EnsoExtensionProxy>();
        private IDictionary<EnsoCommand, EnsoExtensionProxy> commands = new Dictionary<EnsoCommand, EnsoExtensionProxy>();
        private NotifyIcon notifyIcon;

        public EnsoExtensionServer()
        {
            RemotingConfiguration.Configure(Application.ExecutablePath + ".config", false);
            channel = (IChannelReceiver)ChannelServices.RegisteredChannels[0];
            
            ensoProxy = XmlRpcProxyGen.Create<IEnso>();
            ensoProxy.Url = ConfigurationManager.AppSettings["ensoUrl"];
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            ToolStripMenuItem closeToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem.Text = Resource1.ResourceManager.GetString("closeToolStripMenuItem_Text");
            closeToolStripMenuItem.Click += new EventHandler(closeToolStripMenuItem_Click);

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { closeToolStripMenuItem });

            notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Icon = (Icon)Resource1.ResourceManager.GetObject("notifyIcon_Icon");
            notifyIcon.Text = Resource1.ResourceManager.GetString("notifyIcon_Text");
            notifyIcon.Visible = true;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        ~EnsoExtensionServer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                EnsoExtensionProxy[] extensionProxies = new EnsoExtensionProxy[extensions.Values.Count];
                extensions.Values.CopyTo(extensionProxies, 0);

                foreach (EnsoExtensionProxy extensionProxy in extensionProxies)
                {
                    try
                    {
                        extensionProxy.Extension.Unload();
                    }
                    catch (Exception e)
                    {
                        Debug.Fail(e.Message);
                    }
                }

                extensions = null;
                commands = null;
            }

            disposed = true;
        }

        public void RegisterExtension(IEnsoExtension extension)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (extension == null)
                throw new ArgumentNullException("extension");

            extension.Load(this);
        }

        public void UnegisterExtension(IEnsoExtension extension)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (extension == null)
                throw new ArgumentNullException("extension");

            extension.Unload();
        }

        private string GetUrlForUri(string uri)
        {
            return channel.GetUrlsForUri(uri)[0];
        }

        #region IEnsoService Members

        void IEnsoService.DisplayMessage(EnsoMessage message)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (message == null)
                throw new ArgumentNullException("message");

            try
            {
                ensoProxy.DisplayMessage(message.ToString());
            }
            catch (Exception e)
            {
                throw new EnsoException("DisplayMessage failed", e);
            }
        }

        string[] IEnsoService.GetFileSelection()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            try
            {
                return ensoProxy.GetFileSelection();
            }
            catch (Exception e)
            {
                throw new EnsoException("GetFileSelection failed", e);
            }
        }

        string IEnsoService.GetUnicodeSelection()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            try
            {
                return ensoProxy.GetUnicodeSelection();
            }
            catch (Exception e)
            {
                throw new EnsoException("GetUnicodeSelection failed", e);
            }
        }

        void IEnsoService.InsertUnicodeAtCursor(string text, EnsoCommand fromCommand)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (text == null)
                throw new ArgumentNullException("text");

            if (fromCommand == null)
                throw new ArgumentNullException("fromCommand");

            try
            {
                ensoProxy.InsertUnicodeAtCursor(text, fromCommand.ToString());
            }
            catch (Exception e)
            {
                throw new EnsoException("InsertUnicodeAtCursor failed", e);
            }
        }

        void IEnsoService.RegisterCommand(IEnsoExtension extension, string uri, EnsoCommand command)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (extension == null)
                throw new ArgumentNullException("extension");

            if (command == null)
                throw new ArgumentNullException("command");

            IDictionary<string, EnsoExtensionProxy> extensions = this.extensions;
            IDictionary<EnsoCommand, EnsoExtensionProxy> commands = this.commands;
            if (extensions == null || commands == null)
                throw new ObjectDisposedException(GetType().Name);

            lock (this)
            {
                EnsoExtensionProxy extensionProxy;
                if (!extensions.TryGetValue(uri, out extensionProxy))
                {
                    extensionProxy = new EnsoExtensionProxy(extension, uri);
                    RemotingServices.Marshal(extensionProxy, uri, typeof(EnsoExtensionProxy));
                    extensions.Add(uri, extensionProxy);
                }
                else if (extensionProxy.Extension != extension)
                    throw new EnsoException("Invalid uri.");

                try
                {
                    ensoProxy.RegisterCommand(GetUrlForUri(uri), command.ToString(),
                        command.Description, command.Help, postfixTypes[(int)command.PostfixType]);
                }
                catch (Exception e)
                {
                    throw new EnsoException("RegisterCommand failed", e);
                }

                commands.Add(command, extensionProxy);
                extensionProxy.Commands.Add(command.ToString(), command);
            }
        }

        void IEnsoService.SetCommandValidPostfixes(EnsoCommand command, string[] postfixes)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (command == null)
                throw new ArgumentNullException("command");

            if (postfixes == null)
                throw new ArgumentNullException("postfixes");

            IDictionary<EnsoCommand, EnsoExtensionProxy> commands = this.commands;
            if (commands == null)
                throw new ObjectDisposedException(GetType().Name);

            EnsoExtensionProxy extensionProxy;
            if (!commands.TryGetValue(command, out extensionProxy))
                throw new EnsoException("Command not registered.");

            try
            {
                ensoProxy.SetCommandValidPostfixes(GetUrlForUri(extensionProxy.Uri), command.ToString(), postfixes);
            }
            catch (Exception e)
            {
                throw new EnsoException("SetCommandValidPostfixes failed", e);
            }
        }

        void IEnsoService.SetUnicodeSelection(string text, EnsoCommand fromCommand)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (text == null)
                throw new ArgumentNullException("text");

            if (fromCommand == null)
                throw new ArgumentNullException("fromCommand");

            try
            {
                ensoProxy.SetUnicodeSelection(text, fromCommand.ToString());
            }
            catch (Exception e)
            {
                throw new EnsoException("SetUnicodeSelection failed", e);
            }
        }

        void IEnsoService.UnregisterCommand(EnsoCommand command)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (command == null)
                throw new ArgumentNullException("command");

            IDictionary<EnsoCommand, EnsoExtensionProxy> commands = this.commands;
            if (commands == null)
                throw new ObjectDisposedException(GetType().Name);

            lock (this)
            {
                EnsoExtensionProxy extensionProxy;
                if (!commands.TryGetValue(command, out extensionProxy))
                    throw new EnsoException("Command not registered.");

                try
                {
                    ensoProxy.UnregisterCommand(GetUrlForUri(extensionProxy.Uri), command.ToString());
                }
                catch (Exception e)
                {
                    throw new EnsoException("UnregisterCommand failed", e);
                }

                commands.Remove(command);
                extensionProxy.Commands.Remove(command.ToString());

                if (extensionProxy.Commands.Count == 0)
                    extensions.Remove(extensionProxy.Uri);
            }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private class EnsoExtensionProxy : MarshalByRefObject
        {
            private IEnsoExtension extension;
            private string uri;
            private IDictionary<string, EnsoCommand> commands = new Dictionary<string, EnsoCommand>();
            
            public EnsoExtensionProxy(IEnsoExtension extension, string uri)
            {
                this.extension = extension;
                this.uri = uri;
            }

            public IEnsoExtension Extension
            {
                get { return extension; }
            }

            public string Uri
            {
                get { return uri; }
            }

            public IDictionary<string, EnsoCommand> Commands
            {
                get { return commands; }
            }

            public override Object InitializeLifetimeService()
            {
                return null;
            }

            [XmlRpcMethod("callCommand")]
            public void OnCommand(string name, string postfix)
            {
                EnsoCommand command;
                if (!commands.TryGetValue(name, out command))
                    throw new XmlRpcFaultException(4, "Command not registered.");

                extension.OnCommand(command, postfix);
            }
        }
    }
}
