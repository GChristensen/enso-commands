using System;

namespace EnsoExtension
{
    public interface IEnsoExtension
    {
        void Load(IEnsoService enso);

        void OnCommand(EnsoCommand command, string postfix);

        void Unload();
    }
}
