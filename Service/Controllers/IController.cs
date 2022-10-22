using System;
using ComplexPrototypeSystem.Shared;

namespace ComplexPrototypeSystem.Service.Controllers
{
    public interface IController
    {
        void HandleOpcode(Opcode opcode, int size, ArraySegment<byte> payload);

        public event Action OnAuthorized;
    }
}
