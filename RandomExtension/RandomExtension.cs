using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace EnsoExtension
{
    public class RandomExtension : IEnsoExtension
    {
        private static readonly String COMMAND_NAME = "random";
        private static readonly String COMMAND_POSTFIX = "[from num to num]";
        private static readonly String COMMAND_DESC =
            "Generate a random number";

        private static readonly String INVALID_BOUNDS_ERROR =
            "Invalid bound specifier";

        private static Regex boundsParser =
            new Regex(@"(?:from ?(\d+))? ?(?:to ?(\d+))?");

        private IEnsoService service;
        private EnsoCommand command;

        private Random random = new Random();

        public RandomExtension()
        {
            command = new EnsoCommand(COMMAND_NAME, COMMAND_POSTFIX,
                COMMAND_DESC, COMMAND_DESC, EnsoPostfixType.Arbitrary);
        }

        public void Load(IEnsoService service)
        {
            Debug.Assert(service != null);
            this.service = service;

            String uri = this.GetType().Name + ".rem";
            service.RegisterCommand(this, uri, command);
        }

        public void OnCommand(EnsoCommand command, String postfix)
        {
            Match m = boundsParser.Match(postfix);

            int from = 0;
            String s_from = m.Groups[1].Value;

            try
            {
                if (!"".Equals(s_from.Trim()))
                    from = Convert.ToInt32(s_from);
            }
            catch (Exception)
            {
                service.DisplayMessage(new EnsoMessage(INVALID_BOUNDS_ERROR));
            }

            int to = int.MaxValue;
            String s_to = m.Groups[2].Value;

            try
            {
                if (!"".Equals(s_to.Trim()))
                    to = Convert.ToInt32(s_to);
            }
            catch (Exception)
            {
                service.DisplayMessage(new EnsoMessage(INVALID_BOUNDS_ERROR));
            }

            int result = random.Next(from, to);

            service.InsertUnicodeAtCursor(result.ToString(), command);
        }

        public void Unload()
        {
            service.UnregisterCommand(command);
        }
    }
}
