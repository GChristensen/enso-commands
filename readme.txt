A collection of commands for the frozen version of Humanized Enso Launcher
https://github.com/GChristensen/enso-commands

v0.1.0

(c) 2010 g/christensen (gchristnsn@gmail.com)


I haven't found any command package suitable for my needs, so I decided to make
my own one, since there is almost nothing to do with C#. 
If you like Enso, you can use the source code freely as you wish, see more at
the Enso Launcher page (http://humanized.com/enso/launcher) and Enso Wiki
(http://ensowiki.com).

The commands are based on a slightly modified version of C# Enso Extension 
framework  (http://sourceforge.net/projects/ensoextension), and you can enable 
or disable any extension by (un)commenting out its reference entry in the 
`ensoExtensions' section of the extension server configuration file.
Use `(un)install.bat' files to add or remove extension server to/from Windows
registry autostart key.


Dependencies

  * Humanized Enso Launcher (http://humanized.com/enso/launcher)
  * Enso Developer Prototype (http://humanized.com/enso/beta/enso-developer-prototype)
  * Microsoft .NET Framework 4.0 (http://microsoft.com/net)
  * Abbyy Lingvo dictionary software (optional)


Extension List

  UserSessionExtension
    Session/Power management commands (self explanatory):
      
      * log off
      * shut down
      * reboot
      * suspend
      * hibernate

  SystemExtension
    Several system commands:
  
      * kill [process name or id] - kill a process using its executable name 
                                   (without extension) or id
      * memory [process name or id] - query process memory usage

  NetworkExtension
    Network related commands:
  
      * dial [connection name] - connect to the Internet using a dialup connection
      * hangup [connection name] - close an Internet connection

  GUIDExtension
    Generate UUID in several formats (upper/lower case, numeric):

      * guid [format]

  RandomExtension
    Generate random number in the Int32 positive range [0, 2147483646]. 
    It possible to narrow the range using command arguments:

      * random [from num to num]

  LingvoExtension
    Control Abbyy Lingvo dictionary software with the Enso Launcher. It's 
    possible to specify translation direction attributes, see command help 
    for details.
     
      * lingvo [word from lang to lang] - translate a word
      * quit lingvo - close Lingvo
