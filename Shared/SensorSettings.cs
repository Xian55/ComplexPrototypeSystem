using System;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace ComplexPrototypeSystem.Shared
{
    public class SensorSettings
    {
        public Guid Guid { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Interval { get; set; }

        [Required, MinLength(4), MaxLength(16)]
        public byte[] IPAddressBytes { get; set; }

        [NotMapped]
        public IPAddress IPAddress
        {
            get { return new IPAddress(IPAddressBytes); }
            set { IPAddressBytes = value.GetAddressBytes(); }
        }
    }
}
