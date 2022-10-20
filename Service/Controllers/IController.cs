using System;
using ComplexPrototypeSystem.Shared;

namespace ComplexPrototypeSystem.Service.Controllers
{
    public interface IController
    {
        void HandleOpcode(Opcode opCode, int size, ArraySegment<byte> payload);
    }
}
