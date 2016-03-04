namespace SimpleConsole.Module
{
    internal class MathModule : IModule
    {
        public string Name => "Math";

        public void Load(IInterpreter itpr, Env env)
        {
            var code = @"
E = builtin E
PI = builtin PI
fnx square _ => builtin square _
fnx sqrt _ => builtin sqrt _
fnx sin _ => builtin sin _
fnx sin _ => builtin sin _
fnx cos _ => builtin cos _
fnx tan _ => builtin tan _
fnx asin _ => builtin asin _
fnx acos _ => builtin acos _
fnx atan _ => builtin atan _
fnx sinh _ => builtin sinh _
fnx cosh _ => builtin cosh _
fnx tanh _ => builtin tanh _
fnx sign _ => builtin sign _
fnx abs _ => builtin abs _
fnx exp _ => builtin exp _
fnx floor _ => builtin floor _
fnx ceiling _ => builtin ceiling _
fnx log _ => builtin log _
fnx log10 _ => builtin log10 _
fnx round _ => builtin round _
fn pow _ __ => builtin pow _ __
fnx max _ => builtin max _
fnx min _ => builtin min _
fnx sum _ => builtin sum _
fnx product _ => builtin product _
";
            env.LockVariable = true;
            foreach (var item in code.Split('\n'))
            {
                itpr.Input(item);
            }
            env.LockVariable = false;
        }
    }
}
