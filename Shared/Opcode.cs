namespace ComplexPrototypeSystem.Shared
{
    public enum Opcode : byte
    {
        Identify,
        SetInterval,
        Report
    }

    public static class OpCode_Extension
    {
        public static string ToStringF(this Opcode value) => value switch
        {
            Opcode.Identify => nameof(Opcode.Identify),
            Opcode.SetInterval => nameof(Opcode.SetInterval),
            Opcode.Report => nameof(Opcode.Report),
            _ => value.ToString()
        };

        public static bool IsDefined(Opcode value) => value switch
        {
            Opcode.Identify => true,
            Opcode.SetInterval => true,
            Opcode.Report => true,
            _ => false
        };
    }
}
