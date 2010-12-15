using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EnsoExtension
{
    public class GUIDExtension : IEnsoExtension
    {
        private readonly String COMMAND_URI = "guid.rem";
        private readonly String COMMAND_NAME = "guid";
        private readonly String COMMAND_POSTFIX = "case";
        private readonly String COMMAND_DESC = "Drop a fresh GUID at cursor";
        private readonly String POSTFIX_LOWER = "lowercase";
        private readonly String POSTFIX_UPPER = "uppercase";
        private readonly String POSTFIX_NUMERIC = "numeric";

        private IEnsoService service;
        private EnsoCommand command;

        public GUIDExtension()
        {
            command = new EnsoCommand(COMMAND_NAME, COMMAND_POSTFIX, 
                COMMAND_DESC, COMMAND_DESC, EnsoPostfixType.Bounded);
        }
        
        public void Load(IEnsoService service)
        {
            Debug.Assert(service != null);
            this.service = service;

            String uri = this.GetType().Name + ".rem";

            service.RegisterCommand(this, uri, command);
            service.SetCommandValidPostfixes(command,
                new String[] { "", POSTFIX_LOWER, POSTFIX_UPPER,
                                POSTFIX_NUMERIC });
        }

        public void OnCommand(EnsoCommand command, string postfix)
        {
            Guid guid = Guid.NewGuid();
            String result;

            if (POSTFIX_NUMERIC.Equals(postfix))
                result = guid.ToString("N").ToUpper();
            else
                result = POSTFIX_LOWER.Equals(postfix)
                                ? guid.ToString()
                                : guid.ToString().ToUpper();

            service.InsertUnicodeAtCursor(result, command);
        }

        public void Unload()
        {
            service.UnregisterCommand(command);
        }
    }
}
