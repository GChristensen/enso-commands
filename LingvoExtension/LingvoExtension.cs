using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Lingvo;
using System.Text.RegularExpressions;
using System.Threading;

namespace EnsoExtension
{
    public class LingvoExtension : IEnsoExtension
    {
        private static readonly String COMMAND_NAME = "lingvo";
        private static readonly String COMMAND_POSTFIX =
            "word [from lang to lang]";
        private static readonly String COMMAND_DESC = 
            "Translate a word with Abbyy Lingvo dictionary software";
        private static readonly String COMMAND_HELP =
            "Use this command to translate an argument word or "
            + "the current selection with the Abbyy Lingvo dictionary "
            + "software.<br/>"
            + "You can specify source and destination languages after "
            + "<i>from</i> or <i>to</i> words respectively, for example: "
            + "<br/><br/>lingvo espoir from fr to ru<br/><br/>"
            + "Supported language abbreviations are:<br/>"
            + "de - German<br/>"
            + "en - English<br/>"
            + "es - Spanish<br/>"
            + "fr - French<br/>"
            + "it - Italian<br/>"
            + "la - Latin<br/>"
            + "pt - Portuguese<br/>"
            + "ru - Russian<br/>"
            + "ua - Ukrainan<br/>";

        private static readonly String QUIT_NAME = "quit lingvo";
        private static readonly String QUIT_DESC = "Quit Lingvo";

        private static readonly String ERROR_MESSAGE =
            "Can not communicate with Lingvo";

        private static readonly String LINGVO_CORE_LANG = "ru";
        private static readonly String LINGVO_SECONDARY_LANG = "en";

        private static Dictionary<String, Int32> lang2code = 
            new Dictionary<String, Int32>
        {
            {"de", 32775},
            {"fr", 1036},
            {"it", 1040},
            {"es", 1034},
            {"ua", 1058},
            {"la", 1540},
            {"en", 1033},
            {"pt", 2070},
            {"ru", 1049}
        };

        private static Regex wordParser =
            new Regex(@"^(.*?)(?:\s*(?:(?=from)|(?=to)|$))");
        private static Regex directionParser =
            new Regex(@"(?:from ?(\w{2,3}))? ?(?:to ?(\w{2,3}))?$");
        private static Regex latinMatcher =
            new Regex(@"[\p{IsBasicLatin}\p{IsLatin-1Supplement}	"
                      + @"\p{IsLatinExtended-A}\p{IsLatinExtended-B}]+");

        private static void TranslateWord(String postfix, IEnsoService service)
        {
            Match m = wordParser.Match(postfix);

            String word = m.Groups[1].Value;
            if ("".Equals(word.Trim()))
                word = service.GetUnicodeSelection();

            bool isLatin = latinMatcher.IsMatch(word);

            m = directionParser.Match(postfix);

            String from = m.Groups[1].Value;
            if (!lang2code.Keys.Contains(from))
                from = isLatin ? LINGVO_SECONDARY_LANG : LINGVO_CORE_LANG;

            String to = m.Groups[2].Value;
            if (!lang2code.Keys.Contains(to))
                to = isLatin ? LINGVO_CORE_LANG : LINGVO_SECONDARY_LANG;

            try
            {
                ILingvoApplication lingvo = new CLingvoApplication();
                lingvo.TranslateTextInDirection(word, lang2code[from],
                    lang2code[to]);
            }
            catch (Exception)
            {
                service.DisplayMessage(new EnsoMessage(ERROR_MESSAGE));
            }
        }

        private static void QuitLingvo(String postfix, IEnsoService service)
        {
            try
            {
                ILingvoApplication lingvo = new CLingvoApplication();
                lingvo.Quit();
            }
            catch (Exception)
            {
                service.DisplayMessage(new EnsoMessage(ERROR_MESSAGE));
            }
        }

        private Dictionary<String, Action<String, IEnsoService>> 
            commandActions = 
                new Dictionary<String, Action<String, IEnsoService>>
                {
                    {COMMAND_NAME, TranslateWord},
                    {QUIT_NAME, QuitLingvo}
                };

        private IEnsoService service;
        private EnsoCommand translateCommand;
        private EnsoCommand quitCommand;

        public LingvoExtension()
        {
            translateCommand = new EnsoCommand(COMMAND_NAME, COMMAND_POSTFIX,
                COMMAND_DESC, COMMAND_HELP, EnsoPostfixType.Arbitrary);
            quitCommand = new EnsoCommand(QUIT_NAME, null, QUIT_DESC, 
                QUIT_DESC, EnsoPostfixType.None);
        }

        public void Load(IEnsoService service)
        {
            Debug.Assert(service != null);
            this.service = service;

            String uri = this.GetType().Name + ".rem";

            service.RegisterCommand(this, uri, translateCommand);
            service.RegisterCommand(this, uri, quitCommand);
        }

        public void OnCommand(EnsoCommand command, String postfix)
        {
            Action<String, IEnsoService> action = commandActions[command.Name];

            if (action != null)
                new Thread(() => action(postfix, service)).Start();
        }

        public void Unload()
        {
            service.UnregisterCommand(quitCommand);
            service.UnregisterCommand(translateCommand);
        }
    }
}
