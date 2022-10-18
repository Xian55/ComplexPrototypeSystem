using System;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace ComplexPrototypeSystem.Shared
{
    public sealed class SensorSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public int Interval { get; set; } = 5000;

        [Required, MinLength(4), MaxLength(16)]
        public byte[] IPAddressBytes { get; set; } = new byte[4];

        [NotMapped]
        public IPAddress IPAddress
        {
            get { return new IPAddress(IPAddressBytes); }
            set { IPAddressBytes = value.GetAddressBytes(); }
        }
    }
}
