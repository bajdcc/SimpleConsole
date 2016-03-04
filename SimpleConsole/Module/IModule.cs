namespace SimpleConsole.Module
{
    internal interface IModule
    {
        void Load(IInterpreter itpr, Env env);

        string Name { get; }
    }
}
